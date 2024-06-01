using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;

namespace Axon.Installer;

public static class Programm
{
    [STAThread]
    public static void Main(string[] args)
    {
        var gameVersion = "";
        var selectDirectory = false;
        var installMelonLoader = true;
        var installAxon = true;

        //Select Game version
        var client = new HttpClient();
        if (!Uri.TryCreate("https://raw.githubusercontent.com/AxonSL/SLClientPatches/main/versions.txt", UriKind.Absolute, out var uriResult))
            throw new InvalidOperationException("URI is invalid.");
        Console.WriteLine("Getting compatible versions...");
        var versionsString = client.GetStringAsync(uriResult).GetAwaiter().GetResult();
        var versions = versionsString.Split('\n');
        Console.WriteLine("Which game Version do you want to patch?");
        Console.WriteLine("These versions are available: " + string.Join(",", versions));
        for(; ; )
        {
            gameVersion = Console.ReadLine();

            if (!versions.Any(x => x == gameVersion))
            {
                Console.WriteLine("Invalid Version");
                continue;
            }
            break;
        }

        //Configuration
        Console.WriteLine($"Current configuration:\nGameVersion: {gameVersion}\nManualDirectory: {selectDirectory}\nInstall MelonLoader: {installMelonLoader}\nInstall Axon: {installAxon}");
        for(; ; )
        {
            Console.WriteLine("Do you want to edit the installation Configuration?[edit/keep]");
            var input = Console.ReadLine().ToLower();

            if (input == "keep")
                break;

            if (input != "edit") continue;

            Console.WriteLine("Do you want to select a directory yourself?(yes/no)");

            switch (Console.ReadLine().ToLower())
            {
                case "y":
                case "yes":
                    selectDirectory = true;
                    break;

                case "n":
                case "no":
                    selectDirectory = false;
                    break;
            }

            Console.WriteLine("Do you want to install Melonloader(yes/no)");

            switch (Console.ReadLine().ToLower())
            {
                case "y":
                case "yes":
                    installMelonLoader = true;
                    break;

                case "n":
                case "no":
                    installMelonLoader = false;
                    break;
            }

            if (!installMelonLoader)
            {
                installAxon = false;
                break;
            }

            Console.WriteLine("Do you want to install Axon(yes/no)");

            switch (Console.ReadLine().ToLower())
            {
                case "y":
                case "yes":
                    installAxon = true;
                    break;

                case "n":
                case "no":
                    installAxon = false;
                    break;
            }
            break;
        }
        Console.WriteLine($"Current configuration:\nGameVersion: {gameVersion}\nManualDirectory: {selectDirectory}\nInstall MelonLoader: {installMelonLoader}\nInstall Axon: {installAxon}");

        //Ask for SCPSL.Exe
        var dialog = new OpenFileDialog();
        dialog.Multiselect = false;
        dialog.Title = "Select SCPSL.exe";
        dialog.Filter = "SCPSL.exe | SCPSL.exe";
        if (dialog.ShowDialog() != DialogResult.OK) return;
        var exePath = dialog.FileName;
        var vanillaPath = Path.GetDirectoryName(exePath);

        //Install directroy
        var moddedPath = "";
        if (selectDirectory)
        {
            var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Select a Directory in which the modded version will be installed";
            if (folderDialog.ShowDialog() != DialogResult.OK) return;
            moddedPath = folderDialog.SelectedPath;
        }
        else
        {
            moddedPath = Path.Combine(Directory.GetParent(exePath).Parent.FullName, "SCP Secret Laboratory AxonClient");
        }

        Console.WriteLine("Client will be installed at:");
        Console.WriteLine(moddedPath);

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
        if(installMelonLoader)
        {
            var zip = Path.Combine(moddedPath, "MelonLoader.x64.zip");
            if (!File.Exists(zip))
                File.Create(zip).Close();
            Console.WriteLine("Downloading Melonloader");
            DownloadFile("https://github.com/LavaGang/MelonLoader/releases/latest/download/MelonLoader.x64.zip", zip);
            Console.WriteLine("Extracting Melonloader");
            System.IO.Compression.ZipFile.ExtractToDirectory(zip, moddedPath);
            Console.WriteLine("Deleting zip");
            File.Delete(zip);
        }

        //TODO: Install Axon

        //Finally
        Console.WriteLine("Your game is now patched." + (installAxon ? "" : " In order to start it open a cmd and type \"SCPSL.exe -noauth\" in it."));
        Console.WriteLine("We also recommend to add the game to your Steam library");
        Console.WriteLine("Press any Key to Close");
        Process.Start("explorer.exe", moddedPath);
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

        foreach (var patch in patches)
            Console.WriteLine(patch);

        var fileBytes = File.ReadAllBytes(file);

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