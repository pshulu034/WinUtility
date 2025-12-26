using System;
using WinUtility;

namespace TWinService
{
    public class TNetworkAdapter
    {
        public static void Test()
        {
            var nm = new NetworkAdapter();
            var list = nm.GetAdapters();
            foreach (var a in list)
            {
                Console.WriteLine($"{a.Name} | {a.Description}");
                Console.WriteLine($"MAC={a.MacAddress} Enabled={a.Enabled} Type={a.Type} Status={a.Status}");
                Console.WriteLine($"IPv4=[{string.Join(", ", a.IPv4Addresses)}]");
                Console.WriteLine($"IPv6=[{string.Join(", ", a.IPv6Addresses)}]");
                Console.WriteLine($"Gateways=[{string.Join(", ", a.Gateways)}]");
                Console.WriteLine($"DNS=[{string.Join(", ", a.DnsServers)}]");
                Console.WriteLine(new string('-', 60));
            }

            nm.Enable("VMware Network Adapter VMnet1");
        }
    }
}
