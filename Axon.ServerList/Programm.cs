using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.ServerList;

public static class Programm
{
    public const string Letters = "abcdefghijklmopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string ConfigPath { get; set; }
    public static ServerList ServerList { get; set; }

    public static void Main(string[] args)
    {
        ConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
        if (!File.Exists(ConfigPath))
            CreateConfig(ConfigPath);

        var content = File.ReadAllText(ConfigPath);
        var config = JsonConvert.DeserializeObject<ServerListConfiguration>(content);
        ServerList = new ServerList(config);

        while (true)
        {
            OnCommand(Console.ReadLine());
        }
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

    private static string CreateToken()
    {
        var random = new Random();
        return new string(Enumerable.Repeat(Letters, 50).Select(x => x[random.Next(x.Length)]).ToArray());
    }

    private static void OnCommand(string? command)
    {
        if (string.IsNullOrWhiteSpace(command)) return;

        var args = command.Split(' ');

        switch (args[0].ToLower())
        {
            case "reload":
                var content = File.ReadAllText(ConfigPath);
                var config = JsonConvert.DeserializeObject<ServerListConfiguration>(content);
                ServerList.Configuration = config;
                Console.WriteLine("Verified Server List reloaded");
                break;

            case "verify":
                if(args.Length < 3)
                {
                    Console.WriteLine("Usage: verify discord E-Mail");
                    break;
                }
                var conf = ServerList.Configuration;
                var list = conf.VerifiedServers.ToList();
                var entry = new VerifiedServers
                {
                    Identifier = Guid.NewGuid(),
                    Token = CreateToken(),
                    Discord = args[1],
                    EMail = args[2],
                };
                list.Add(entry);
                conf.VerifiedServers = list.ToArray();
                ServerList.Configuration = conf;
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(conf));
                Console.WriteLine("New Server verified\nToken:" + entry.Token + "\nIdentifier:" + entry.Identifier);
                break;

            case "remove":
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: remove identifier");
                    break;
                }

                try
                {
                    var guid = Guid.Parse(args[1]);
                    content = File.ReadAllText(ConfigPath);
                    config = JsonConvert.DeserializeObject<ServerListConfiguration>(content);
                    list = config.VerifiedServers.ToList();
                    list.Remove(list.First(x => x.Identifier == guid));
                    config.VerifiedServers = list.ToArray();
                    File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(config));
                    Console.WriteLine("Removed Server");
                }
                catch(Exception e)
                {
                    Console.WriteLine("Failed to remove Server");
                }
                break;

            default:
                Console.WriteLine("Command not found");
                break;
        }
    }
}
