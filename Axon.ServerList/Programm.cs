using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.ServerList;

public static class Programm
{
    public static string ConfigPath { get; set; } = "";
    public static ServerList? ServerList { get; set; }

    public static void Main(string[] args)
    {
        ConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
        if (!File.Exists(ConfigPath))
            CreateConfig(ConfigPath);

        var content = File.ReadAllText(ConfigPath);
        var config = JsonConvert.DeserializeObject<ServerListConfiguration>(content);
        ServerList = new ServerList(config, args);

        Task.Delay(-1).GetAwaiter().GetResult();
    }

    private static void CreateConfig(string path)
    {
        var config = new ServerListConfiguration
        {
            Url = "http://*:8080/",
            VerifiedServers = new VerifiedServers[0] { }
        };

        var json = JsonConvert.SerializeObject(config);
        File.Create(path).Close();
        File.WriteAllText(path, json);
    }
}
