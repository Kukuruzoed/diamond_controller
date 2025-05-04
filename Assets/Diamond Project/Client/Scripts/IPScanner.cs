using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class IPScanner : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private float scanTimeoutMs = 2000f;
    [SerializeField] private float retryIntervalSeconds = 1f;
    [SerializeField] private int maxRetries = 10;
    [SerializeField] private int port = 9001;

    private TcpClient client;
    private NetworkStream stream;
    private readonly byte[] buffer = new byte[1024];
    private readonly ConcurrentQueue<string> commandQueue = new ConcurrentQueue<string>();

    public UnityEvent<string> OnCommandReceived = new UnityEvent<string>();

    async void Start()
    {
        string serverIP = await FindServerWithRetriesAsync();
        if (string.IsNullOrEmpty(serverIP))
        {
            Debug.LogWarning("Could not find server in time.");
            return;
        }

        Debug.Log($"Connected to server: {serverIP}");
        infoText.text = $"Подключено к {serverIP}\nОжидание запуска видео.";

        client = new TcpClient();
        try
        {
            await client.ConnectAsync(serverIP, port);
            stream = client.GetStream();
            stream.BeginRead(buffer, 0, buffer.Length, OnDataReceived, null);
        }
        catch (Exception e)
        {
            Debug.LogError("[Client] Connection failed: " + e.Message);
            infoText.text = $"Подключение оборвалось:\n{e.Message}";
        }
    }

    public async Task<string> FindServerWithRetriesAsync()
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            Debug.Log($"Scan attempt {attempt + 1}/{maxRetries}");
            infoText.text = $"Попытка подключения {attempt + 1}/{maxRetries}...";

            string ip = await FindServerOnce();
            if (!string.IsNullOrEmpty(ip)) return ip;

            infoText.text = "Сервер не найден. Повтор поиска...";
            await Task.Delay(TimeSpan.FromSeconds(retryIntervalSeconds));
        }

        Debug.LogWarning("Server not found after max attempts.");
        infoText.text = "Сервер не найден.";
        return null;
    }

    private async Task<string> FindServerOnce()
    {
        string localIP = GetLocalWiFiIP();
        if (string.IsNullOrEmpty(localIP))
        {
            Debug.LogError("Failed to get local IP.");
            infoText.text = "Не найден локальный IP.";
            return null;
        }

        string subnet = localIP.Substring(0, localIP.LastIndexOf('.') + 1);
        List<Task<string>> scanTasks = new List<Task<string>>();

        for (int i = 1; i <= 241; i++)
        {
            string ip = subnet + i;
            scanTasks.Add(TryConnectToHost(ip, port, (int)scanTimeoutMs));
            infoText.text = $"Scanning: {ip}";
            await Task.Delay(5); // небольшая задержка для обновления UI
        }

        while (scanTasks.Count > 0)
        {
            Task<string> finished = await Task.WhenAny(scanTasks);
            scanTasks.Remove(finished);

            string result = await finished;
            if (result != null) return result;
        }

        return null;
    }

    private async Task<string> TryConnectToHost(string ip, int port, int timeoutMs)
    {
        using var tcpClient = new TcpClient();
        var connectTask = tcpClient.ConnectAsync(ip, port);
        var delayTask = Task.Delay(timeoutMs);

        var completed = await Task.WhenAny(connectTask, delayTask);
        return (completed == connectTask && tcpClient.Connected) ? ip : null;
    }

    public static string GetLocalWiFiIP()
    {
        foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (adapter.OperationalStatus != OperationalStatus.Up ||
                adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                adapter.Description.ToLower().Contains("virtual") ||
                adapter.Description.ToLower().Contains("tunnel"))
                continue;

            foreach (var ua in adapter.GetIPProperties().UnicastAddresses)
            {
                if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    string ip = ua.Address.ToString();
                    if (ip.StartsWith("192.") || ip.StartsWith("10.") || ip.StartsWith("172."))
                        return ip;
                }
            }
        }
        return null;
    }

    void OnDataReceived(IAsyncResult ar)
    {
        int bytesRead = stream.EndRead(ar);
        if (bytesRead <= 0) return;

        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Debug.Log("[Client] Received command: " + message);
        commandQueue.Enqueue(message);

        stream.BeginRead(buffer, 0, buffer.Length, OnDataReceived, null);
    }

    void Update()
    {
        while (commandQueue.TryDequeue(out var cmd))
            OnCommandReceived?.Invoke(cmd);
    }
}