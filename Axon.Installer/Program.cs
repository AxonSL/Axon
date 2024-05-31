using System;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;

namespace Axon.Installer;

public static class Programm
{
    //Just my personal reference :D
    //D:\Programme\Steam\steamapps\common\SCP Secret Laboratory
    [STAThread]
    public static void Main(string[] args)
    {
        string gameVersion = "13.4.2";

        //Ask for SCPSL.Exe
        var dialog = new OpenFileDialog();
        dialog.Multiselect = false;
        dialog.Title = "Select SCPSL.exe";
        dialog.Filter = "SCPSL.exe | SCPSL.exe";
        if (dialog.ShowDialog() != DialogResult.OK) return;
        var exePath = dialog.FileName;
        var vanillaPath = Path.GetDirectoryName(exePath);

        //Ask for new install directroy
        var folderDialog = new FolderBrowserDialog();
        folderDialog.Description = "Select a Directory in which the modded version will be installed";
        if (folderDialog.ShowDialog() != DialogResult.OK) return;
        var moddedPath = folderDialog.SelectedPath;

        //Copy Sl vanilla to the modded Path
        CopyFilesRecursively(new DirectoryInfo(vanillaPath), new DirectoryInfo(moddedPath));
        Console.WriteLine("Copied all files");

        //Download custom SCPSL.exe
        var newExePath = Path.Combine(moddedPath, "SCPSL.exe");
        File.Create(newExePath).Close();
        DownloadFile($"https://github.com/AxonSL/SLClientPatches/raw/main/{gameVersion}/SCPSL.exe", newExePath);

        //Patch GameAssembly.dll
        var success = PatchFile($"https://raw.githubusercontent.com/AxonSL/SLClientPatches/main/{gameVersion}/gameassembly.patches", Path.Combine(moddedPath, "GameAssembly.dll"));
        if (!success)
        {
            Console.WriteLine("Abort...");
            Console.ReadKey();
            return;
        }

        //Patch global-metadata.dat
        var dataPath = Path.Combine(moddedPath, "SCPSL_Data");
        var il2cppPath = Path.Combine(dataPath, "il2cpp_data");
        var metaDataPath = Path.Combine(il2cppPath, "Metadata");
        success = PatchFile($"https://raw.githubusercontent.com/AxonSL/SLClientPatches/main/{gameVersion}/global-metadata.patches", Path.Combine(metaDataPath, "global-metadata.dat"));
        if (!success)
        {
            Console.WriteLine("Abort...");
            Console.ReadKey();
            return;
        }

        //Install Melonloader
        var zip = Path.Combine(moddedPath, "MelonLoader.x64.zip");
        if (!File.Exists(zip))
            File.Create(zip).Close();
        Console.WriteLine("Downloading Melonloader");
        DownloadFile("https://github.com/LavaGang/MelonLoader/releases/latest/download/MelonLoader.x64.zip", zip);
        Console.WriteLine("Extracting Melonloader");
        System.IO.Compression.ZipFile.ExtractToDirectory(zip, moddedPath);
        File.Delete(zip);

        //Finally
        Console.WriteLine("You game is now patched. In order to start it open a cmd and type \"SCPSL.exe -noauth\" in it.");
        Console.WriteLine("Have Fun :D");
        Console.WriteLine("Press any Key to Close");
        Console.ReadKey();
    }

    private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
    {
        foreach (DirectoryInfo dir in source.GetDirectories())
            CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));

        foreach (FileInfo file in source.GetFiles())
        {
            if (file.Name == "SL-AC.dll") continue;
            if (file.Name == "SCPSL.exe") continue;

            Console.WriteLine("Copying " + file.Name);
            file.CopyTo(Path.Combine(target.FullName, file.Name));
        }
    }

    private static void DownloadFile(string uri, string outputPath)
    {
        var client = new HttpClient();

        if (!Uri.TryCreate(uri, UriKind.Absolute, out var uriResult))
            throw new InvalidOperationException("URI is invalid.");

        if (!File.Exists(outputPath))
            throw new FileNotFoundException("File not found.", nameof(outputPath));

        byte[] fileBytes = client.GetByteArrayAsync(uri).GetAwaiter().GetResult();
        File.WriteAllBytes(outputPath, fileBytes);
    }

    private static bool PatchFile(string uri, string file)
    {
        var client = new HttpClient();
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var uriResult))
            throw new InvalidOperationException("URI is invalid.");

        Console.WriteLine("Getting patches...");

        var gameassemblyPatches = client.GetStringAsync(uriResult).GetAwaiter().GetResult();
        var patches = gameassemblyPatches.Split("\n");

        Console.WriteLine(patches.Length);

        foreach (var patch in patches)
            Console.WriteLine(patch);

        var fileBytes = File.ReadAllBytes(file);

        Console.WriteLine("Found " + fileBytes.Length + " bytes");

        for(var i = 1; i < patches.Length; i++)
        {
            var patch = patches[i].Trim();
            if (string.IsNullOrWhiteSpace(patch)) continue;

            var data = patch.Split(':');
            var position = Convert.ToInt32(data[0], 16);
            var expected = Convert.ToByte(data[1], 16);
            var toWrite = Convert.ToByte(data[2], 16);

            var read = fileBytes[position];

            if(read != expected)
            {
                Console.WriteLine("Found unexpected byte in GameAssembly.dll ... are you using the wrong version?");
                return false;
            }

            fileBytes[position] = toWrite;
            Console.WriteLine($"Applied patch {data[0]} from {data[1]} to {data[2]}");
        }

        File.WriteAllBytes(file, fileBytes);

        return true;
    }
}