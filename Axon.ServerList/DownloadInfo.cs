namespace Axon.ServerList;

[Serializable]
public class DownloadInfo
{
    public string Name { get; set; }
    public ulong Version { get; set; }
    public string DownloadLink { get; set; }
}
