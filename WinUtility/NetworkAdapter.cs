using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace WinUtil
{
    public class NetworkAdapter
    {
        public class AdapterInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string? MacAddress { get; set; }
            public bool Enabled { get; set; }
            public NetworkInterfaceType Type { get; set; }
            public OperationalStatus Status { get; set; }
            public IReadOnlyList<string> IPv4Addresses { get; set; } = Array.Empty<string>();
            public IReadOnlyList<string> IPv6Addresses { get; set; } = Array.Empty<string>();
            public IReadOnlyList<string> Gateways { get; set; } = Array.Empty<string>();
            public IReadOnlyList<string> DnsServers { get; set; } = Array.Empty<string>();
        }

        public IReadOnlyList<AdapterInfo> GetAdapters()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Select(ToInfo)
                .Where(a => a != null)
                .Cast<AdapterInfo>()
                .OrderBy(a => a.Name)
                .ToList();
        }

        public AdapterInfo? GetAdapter(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            return GetAdapters().FirstOrDefault(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public bool Enable(string name)
        {
            return RunNetsh($"interface set interface name=\"{name}\" admin=enabled");
        }

        public bool Disable(string name)
        {
            return RunNetsh($"interface set interface name=\"{name}\" admin=disabled");
        }

        public bool SetDhcp(string name)
        {
            var ok1 = RunNetsh($"interface ip set address name=\"{name}\" source=dhcp");
            var ok2 = RunNetsh($"interface ip set dns name=\"{name}\" source=dhcp");
            return ok1 && ok2;
        }

        public bool SetStaticIp(string name, string ip, string mask, string? gateway = null, int gwMetric = 1)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (string.IsNullOrWhiteSpace(ip)) return false;
            if (string.IsNullOrWhiteSpace(mask)) return false;
            string gw = string.IsNullOrWhiteSpace(gateway) ? "none" : gateway;
            return RunNetsh($"interface ip set address name=\"{name}\" static {ip} {mask} {gw} {gwMetric}");
        }

        public bool SetDnsDhcp(string name)
        {
            return RunNetsh($"interface ip set dns name=\"{name}\" source=dhcp");
        }

        public bool SetDnsServers(string name, IEnumerable<string> servers)
        {
            var list = servers?.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList() ?? new List<string>();
            if (string.IsNullOrWhiteSpace(name) || list.Count == 0) return false;
            var ok = RunNetsh($"interface ip set dns name=\"{name}\" static addr={list[0]}");
            for (int i = 1; i < list.Count; i++)
            {
                ok = RunNetsh($"interface ip add dns name=\"{name}\" addr={list[i]} index={i + 1}") && ok;
            }
            return ok;
        }

        private static AdapterInfo ToInfo(NetworkInterface ni)
        {
            var props = ni.GetIPProperties();
            var unicast = props.UnicastAddresses;
            var dns = props.DnsAddresses;
            var gw = props.GatewayAddresses;

            var ipv4 = unicast
                .Where(u => u.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(u => u.Address.ToString())
                .ToList();

            var ipv6 = unicast
                .Where(u => u.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                .Select(u => u.Address.ToString())
                .ToList();

            var gateways = gw.Select(g => g.Address.ToString()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            var dnsList = dns.Select(d => d.ToString()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            string? mac = null;
            try
            {
                var macBytes = ni.GetPhysicalAddress()?.GetAddressBytes();
                if (macBytes != null && macBytes.Length > 0)
                    mac = string.Join("-", macBytes.Select(b => b.ToString("X2")));
            }
            catch { }

            return new AdapterInfo
            {
                Name = ni.Name,
                Description = ni.Description,
                MacAddress = mac,
                Enabled = ni.OperationalStatus != OperationalStatus.Down && ni.OperationalStatus != OperationalStatus.NotPresent,
                Type = ni.NetworkInterfaceType,
                Status = ni.OperationalStatus,
                IPv4Addresses = ipv4,
                IPv6Addresses = ipv6,
                Gateways = gateways,
                DnsServers = dnsList
            };
        }

        private static bool RunNetsh(string args)
        {
            var executor = new CommandExecutor();
            var result = executor.Execute("netsh", args, timeout: 30000);
            return result.IsSuccess;
        }
    }
}
