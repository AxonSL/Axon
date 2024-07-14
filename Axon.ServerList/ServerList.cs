using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Axon.ServerList;

public class ServerList
{
    public static Action<string> Log = Console.WriteLine;

    private HttpListener _listener = new HttpListener();
    private ConcurrentDictionary<string, DateTime> _ipTracker = new ConcurrentDictionary<string, DateTime>();
    private readonly TimeSpan rateLimit = TimeSpan.FromSeconds(10);

    public ServerListConfiguration Configuration { get; set; }
    public List<ServerEntry> ServerEntries { get; set; } = new();
    public IEnumerable<Server> Servers => ServerEntries.Select(x => x.Server);


    public ServerList(ServerListConfiguration configuration)
    {
        Configuration = configuration;
        Start();
    }

    private void Start()
    {
        _listener.Prefixes.Add(Configuration.Url);
        _listener.Start();

        Task.Run(Listen);
        Task.Run(CheckList);
        Log("Server list node started");
    }

    private void CheckList()
    {
        while(_listener.IsListening)
        {
            Log("Check List");
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
            response.StatusDescription = "Wait atleast 10 seconds";
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
                        catch (Exception ex)
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

                                /*
                                if ((updateBit & 1024) != 0)
                                    entry.Server.Downloads = server.Downloads;
                                */

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
}