namespace WinUtility;

using System;
using System.Linq;
using System.Management;                    //dotnet add package System.Management
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

public static class MachineFingerprint
{
    public static string GetFingerprint()
    {
        var parts = new[]
        {
            GetMachineGuid(),
            GetSystemDriveSerial(),
            GetCpuId(),
            //GetMacAddress()
        };

        string raw = string.Join("|", parts.Where(p => !string.IsNullOrEmpty(p)));
        return Sha256(raw);
    }

    // -----------------------------
    // Core components
    // -----------------------------

    private static string GetMachineGuid()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            return key?.GetValue("MachineGuid")?.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string GetSystemDriveSerial()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_LogicalDisk WHERE DeviceID='C:'");

            foreach (ManagementObject disk in searcher.Get())
            {
                return disk["SerialNumber"]?.ToString()?.Trim();
            }
        }
        catch { }

        return string.Empty;
    }

    private static string GetCpuId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");

            foreach (ManagementObject cpu in searcher.Get())
            {
                return cpu["ProcessorId"]?.ToString();
            }
        }
        catch { }

        return string.Empty;
    }

    //一般不使用MAC地址作为唯一标识，因为它可以被修改，但仍然提供以防万一
    private static string GetMacAddress()
    {
        try
        {
            var nic = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n =>
                    n.OperationalStatus == OperationalStatus.Up &&
                    n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    n.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                    !n.Description.ToLower().Contains("virtual") &&
                    !n.Description.ToLower().Contains("vpn") &&
                    !n.Description.ToLower().Contains("hyper-v"))
                .OrderByDescending(n => n.Speed)
                .FirstOrDefault();

            if (nic == null)
                return string.Empty;

            return nic.GetPhysicalAddress().ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    // -----------------------------
    // Hash
    // -----------------------------

    private static string Sha256(string input)
    {
        using var sha = SHA256.Create();
        byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    public static string Md5(string input)
    {
        using var md5 = MD5.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hash = md5.ComputeHash(bytes);

        // Convert to hex string
        var sb = new StringBuilder(hash.Length * 2);
        foreach (byte b in hash)
            sb.Append(b.ToString("x2"));

        return sb.ToString();
    }
}
