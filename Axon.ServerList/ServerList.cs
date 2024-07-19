using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Axon.ServerList;

public class ServerList
{
    public static Action<string> Log = Console.WriteLine;

    private string[] _args = new string[0];
    private string _adminToken = string.Empty;

    private readonly HttpListener _listener = new HttpListener();
    private readonly ConcurrentDictionary<string, DateTime> _ipTracker = new ConcurrentDictionary<string, DateTime>();
    private readonly TimeSpan rateLimit = TimeSpan.FromSeconds(3);

    public ServerListConfiguration Configuration { get; set; }
    public List<ServerEntry> ServerEntries { get; set; } = new();
    public IEnumerable<Server> Servers => ServerEntries.Select(x => x.Server);


    public ServerList(ServerListConfiguration configuration, string[] args)
    {
        _args = args;
        Configuration = configuration;
        Start();
    }

    private void Start()
    {
        var token = Environment.GetEnvironmentVariable("ADMINTOKEN");
        if(token != null)
        {
            _adminToken = token;
        }
        else
        {
            var arg = _args.FirstOrDefault(x => x.StartsWith("-admintoken="));
            var argToken = arg?.Split('=')[1];
            if (!string.IsNullOrWhiteSpace(argToken))
            {
                _adminToken = argToken;
            }
            else
            {
                Console.WriteLine("No admin token found.\nPlease provide a Token otherwise the verification of servers are not possible");
            }
        }

        _listener.Prefixes.Add(Configuration.Url+ ":8080/");
        _listener.Start();

        Task.Run(Listen);
        Task.Run(CheckList);
        Log("Server list node started");
    }

    private void CheckList()
    {
        while(_listener.IsListening)
        {
            foreach(var server in ServerEntries.ToList())
            {
                if((DateTime.Now - server.LastUpdate).TotalMinutes > 2)
                {
                    ServerEntries.Remove(server);
                }
            }
            Thread.Sleep(180000);
        }
    }

    private async void Listen()
    {
        while(_listener.IsListening)
        {
            var context = await _listener.GetContextAsync();
            _ = Task.Run(() => HandleRequestAsync(context));
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        var ip = request.RemoteEndPoint.Address.ToString();
        response.ContentEncoding = Encoding.UTF8;
        response.ContentType = "application/json";
        response.StatusCode = 500;

        var requestBody = await new StreamReader(request.InputStream, request.ContentEncoding).ReadToEndAsync();
        var responseBody = "{}";

        if (IsRateLimited(context.Request.RemoteEndPoint.Address.ToString()))
        {
            response.StatusCode = 429;
            response.StatusDescription = $"Wait atleast {rateLimit.TotalSeconds} seconds";
            goto finish;
        }

        switch (request.Url?.AbsolutePath)
        {
            case "/serverlist":
                switch (request.HttpMethod)
                {
                    case "GET":
                        try
                        {
                            response.StatusCode = 200;
                            responseBody = JsonConvert.SerializeObject(Servers);
                            break;
                        }
                        catch
                        {
                            response.StatusCode = 500;
                            response.StatusDescription = "Couldn't serialize server list";
                            break;
                        }

                    case "POST":
                        //Check Token
                        var token = request.QueryString["token"];
                        if (string.IsNullOrWhiteSpace(token))
                        {
                            response.StatusCode = 401;
                            response.StatusDescription = "You need a token to post a server";
                            break;
                        }

                        if (!Configuration.VerifiedServers.Any(x => x.Token == token))
                        {
                            Log("Invalid Token: " + token);
                            response.StatusCode = 403;
                            response.StatusDescription = "This token is not valid";
                            break;
                        }

                        try
                        {
                            var server = JsonConvert.DeserializeObject<Server>(requestBody);
                            var identifier = Configuration.VerifiedServers.First(x => x.Token == token).Identifier.ToString();
                            server.Identifier = identifier;
                            identifier += "-" + server.Ip + ":" + server.Port;

                            //Check if Server is already on list
                            if (ServerEntries.Any(x => x.Identifier == identifier))
                            {
                                var entry = ServerEntries.First(x => x.Identifier == identifier);

                                //Check if the Server wants to update everything
                                var updateBitString = request.QueryString["updatebit"];
                                if (string.IsNullOrWhiteSpace(updateBitString))
                                {
                                    ServerEntries.Remove(entry);
                                    ServerEntries.Add(new ServerEntry
                                    {
                                        LastUpdate = DateTime.Now,
                                        Server = server,
                                    });
                                    response.StatusCode = 201;
                                    break;
                                }

                                if (!uint.TryParse(updateBitString, out var updateBit))
                                {
                                    response.StatusCode = 400;
                                    response.StatusDescription = "updateBit is not a uint";
                                    break;
                                }

                                entry.LastUpdate = DateTime.Now;

                                if ((updateBit & 1) != 0)
                                    entry.Server.Version = server.Version;

                                if ((updateBit & 2) != 0)
                                    entry.Server.Info = server.Info;

                                if ((updateBit & 4) != 0)
                                    entry.Server.Patsebin = server.Patsebin;

                                if ((updateBit & 8) != 0)
                                    entry.Server.Geoblocking = server.Geoblocking;

                                if ((updateBit & 16) != 0)
                                    entry.Server.Whitelist = server.Whitelist;

                                if ((updateBit & 32) != 0)
                                    entry.Server.AccessRestriction = server.AccessRestriction;

                                if ((updateBit & 64) != 0)
                                    entry.Server.FriendlyFire = server.FriendlyFire;

                                if ((updateBit & 128) != 0)
                                    entry.Server.Players = server.Players;

                                if ((updateBit & 256) != 0)
                                    entry.Server.MaxPlayers = server.MaxPlayers;

                                if ((updateBit & 512) != 0)
                                    entry.Server.PlayerList = server.PlayerList;

                                if ((updateBit & 1024) != 0)
                                    entry.Server.Mods = server.Mods;

                                response.StatusCode = 200;
                                break;
                            }

                            ServerEntries.Add(new ServerEntry
                            {
                                LastUpdate = DateTime.Now,
                                Server = server,
                            });
                            response.StatusCode = 201;
                            break;
                        }
                        catch
                        {
                            response.StatusDescription = "Couldn't deserialize your Server";
                            response.StatusCode = 400;
                            break;
                        }

                    default:
                        response.StatusCode = 400;
                        response.StatusDescription = "You can only GET or POST on the server list";
                        break;
                }
                break;

            case "/admin":
                var adminToken = request.QueryString["adminToken"];
                if (string.IsNullOrWhiteSpace(adminToken))
                {
                    response.StatusCode = 401;
                    response.StatusDescription = "You must provide a token";
                    break;
                }

                if (adminToken != _adminToken)
                {
                    Log("Invalid Token: " + adminToken);
                    response.StatusCode = 403;
                    response.StatusDescription = "This token is not valid";
                    break;
                }

                Console.WriteLine("Got valid admin request from endpoint: " + ip);

                switch (request.HttpMethod)
                {
                    case "GET":
                        try
                        {
                            response.StatusCode = 200;
                            responseBody = JsonConvert.SerializeObject(Configuration.VerifiedServers);
                            break;
                        }
                        catch
                        {
                            response.StatusCode = 500;
                            response.StatusDescription = "Couldn't serialize verified Servers";
                            break;
                        }

                    case "POST":
                        try
                        {
                            var requestContent = JObject.Parse(requestBody);
                            var discord = (string)(requestContent["discord"] ?? throw new NullReferenceException())!;
                            var eMail = (string)(requestContent["email"] ?? throw new NullReferenceException())!;
                            if (string.IsNullOrWhiteSpace(discord) || string.IsNullOrWhiteSpace(eMail))
                                throw new NullReferenceException();

                            var conf = Configuration;
                            var list = conf.VerifiedServers.ToList();
                            var entry = new VerifiedServers
                            {
                                Identifier = Guid.NewGuid(),
                                Token = CreateToken(),
                                Discord = discord,
                                EMail = eMail,
                            };
                            list.Add(entry);
                            conf.VerifiedServers = list.ToArray();
                            Configuration = conf;
                            File.WriteAllText(Programm.ConfigPath, JsonConvert.SerializeObject(conf));
                            Console.WriteLine("New Server verified\nToken:" + entry.Token + "\nIdentifier:" + entry.Identifier+"\n");
                            responseBody = JsonConvert.SerializeObject(entry);
                            response.StatusCode = 201;
                        }
                        catch
                        {
                            response.StatusCode = 400;
                            response.StatusDescription = "Couldn't deserialize body";
                            break;
                        }
                        break;

                    case "DELETE":
                        try
                        {
                            var requestContent = JObject.Parse(requestBody);
                            var identifier = requestContent["identifier"] ?? throw new NullReferenceException();
                            var guid = Guid.Parse((string)identifier!);

                            var config = Configuration;
                            var list = config.VerifiedServers.ToList();
                            var entry = list.FirstOrDefault(x => x.Identifier == guid);

                            if(entry == null)
                            {
                                response.StatusCode = 404;
                                response.StatusDescription = "No Server with that Identifier found";
                                break;
                            }

                            list.Remove(entry);
                            config.VerifiedServers = list.ToArray();
                            File.WriteAllText(Programm.ConfigPath, JsonConvert.SerializeObject(config));
                            Configuration = config;
                            Console.WriteLine("Removed Server with identifier: " + identifier + "\n");
                            response.StatusCode = 200;
                        }
                        catch
                        {
                            response.StatusCode = 400;
                            response.StatusDescription = "Couldn't deserialize identifier";
                            break;
                        }
                        break;

                    default:
                        response.StatusCode = 400;
                        response.StatusDescription = "You can only Get,Post or Delete";
                        break;
                }
                break;

            default:
                response.StatusCode = 400;
                response.StatusDescription = "The requested URL is not vaild";
                break;
        }

        finish:
        var buffer = Encoding.UTF8.GetBytes(responseBody);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.Close();
    }

    private bool IsRateLimited(string clientIp)
    {
        var now = DateTime.UtcNow;

        if (_ipTracker.TryGetValue(clientIp, out var lastRequestTime))
        {
            if ((now - lastRequestTime) < rateLimit)
            {
                return true;
            }
        }

        _ipTracker[clientIp] = now;
        return false;
    }

    private static string CreateToken()
    {
        var random = new Random();
        return new string(Enumerable.Repeat(_letters, 50).Select(x => x[random.Next(x.Length)]).ToArray());
    }

    private const string _letters = "abcdefghijklmopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
}