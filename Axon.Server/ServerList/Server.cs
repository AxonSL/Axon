using System;
using System.Collections.Generic;

namespace Axon.Server.ServerList;

[Serializable]
public class Server
{
    public string Version;                          //1
    public string Info;                             //2
    public string Patsebin;                         //4

    public bool Geoblocking;                        //8
    public bool Whitelist;                          //16
    public bool AccessRestriction;                  //32

    public bool FriendlyFire;                       //64
    public int Players;                             //128
    public int MaxPlayers;                          //256
    public Dictionary<string, string> PlayerList;   //512

    public DownloadInfo[] Downloads;                //1024

    public string Ip;
    public ushort Port;
}