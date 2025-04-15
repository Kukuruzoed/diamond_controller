using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class IPScanner : MonoBehaviour
{
    public int serverPort = 9000;
    public float scanTimeoutMs = 2000f;
    public float retryIntervalSeconds = 1f;
    public int maxRetries = 10;

    public async Task<string> FindServerWithRetriesAsync()
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            Debug.Log($"🔎 Scan attempt {attempt + 1}/{maxRetries}");
            string found = await FindServerOnce();
            if (!string.IsNullOrEmpty(found))
                return found;

            await Task.Delay((int)(retryIntervalSeconds * 1000));
        }

        Debug.LogWarning("⏱ Server not found after max attempts.");
        return null;
    }

    private async Task<string> FindServerOnce()
    {
        string localIP = GetLocalWiFiIP();
        if (string.IsNullOrEmpty(localIP))
        {
            Debug.LogError("❌ Failed to get local IP.");
            return null;
        }

        string subnet = GetSubnet(localIP);
        List<Task<string>> scanTasks = new List<Task<string>>();

        for (int i = 1; i <= 241; i++)
        {
            string ip = subnet + i;
            scanTasks.Add(TryConnectToHost(ip, serverPort, (int)scanTimeoutMs));
        }

        while (scanTasks.Count > 0)
        {
            Task<string> finished = await Task.WhenAny(scanTasks);
            scanTasks.Remove(finished);

            string result = await finished;
            if (result != null)
            {
                Debug.Log("✅ Server found at: " + result);
                return result;
            }
        }

        return null;
    }

    private async Task<string> TryConnectToHost(string ip, int port, int timeoutMs)
    {
        using (var client = new TcpClient())
        {
            var connectTask = client.ConnectAsync(ip, port);
            var delayTask = Task.Delay(timeoutMs);

            var completed = await Task.WhenAny(connectTask, delayTask);

            if (completed == connectTask && client.Connected)
                return ip;
        }

        return null;
    }

    private string GetLocalIPAddress()
    {
        foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }
        return null;
    }

    private string GetSubnet(string ipAddress)
    {
        int lastDot = ipAddress.LastIndexOf('.');
        return ipAddress.Substring(0, lastDot + 1); // e.g., "192.168.0."
    }

    public static string GetLocalWiFiIP()
    {
        foreach (var netInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
        {
            if (netInterface.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up)
                continue;

            // Пропускаем loopback и туннели
            if (netInterface.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Loopback ||
                netInterface.Description.ToLower().Contains("virtual") ||
                netInterface.Description.ToLower().Contains("tunnel"))
                continue;

            foreach (var ua in netInterface.GetIPProperties().UnicastAddresses)
            {
                if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    string ip = ua.Address.ToString();

                    // Проверка, что это локальный IP (192.168.x.x или 10.x.x.x)
                    if (ip.StartsWith("192.") || ip.StartsWith("10.") || ip.StartsWith("172."))
                    {
                        return ip;
                    }
                }
            }
        }

        return null;
    }
}
