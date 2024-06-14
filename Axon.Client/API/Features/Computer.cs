using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Axon.Client.API.Features;

public static class Computer
{
    private static string _mac;

    public static string Mac
    {
        get
        {
            if (_mac == null)
            {
                _mac = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .Select(nic => nic.GetPhysicalAddress().ToString())
                    .FirstOrDefault() ?? "Unknown";
            }

            return _mac;
        }
    }

    public static string PcName => Environment.MachineName ?? "Unknown";
    public static string UserName => Environment.UserName ?? "Unknown";

    public static void OpenUrl(string url) => Application.OpenURL(url);
}
