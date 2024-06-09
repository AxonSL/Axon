using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Client.API.Features;

public static class Paths
{
    public static string ApplicationDataPath { get; }
    public static string AxonPath { get; }

    static Paths()
    {
        ApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        AxonPath = Path.Combine(ApplicationDataPath, "Axon");
        if(!Directory.Exists(AxonPath))
            Directory.CreateDirectory(AxonPath);
    }
}
