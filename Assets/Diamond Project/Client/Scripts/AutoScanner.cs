using System.IO;
using System.Net.Sockets;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
using UnityEngine;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using UnityEngine.LightTransport;
using UnityEngine.Events;
using System.Collections.Concurrent;

public class AutoScanner : MonoBehaviour
{
    public IPScanner scanner;
    private TcpClient client;
    private NetworkStream stream;
    private byte[] buffer = new byte[1024];
    public int port = 9000;

    public UnityEvent<string> OnCommandReceived = new UnityEvent<string>();


    private ConcurrentQueue<string> commandQueue = new ConcurrentQueue<string>();

    async void Start()
    {
        string serverIP = await scanner.FindServerWithRetriesAsync();
        if (serverIP != null)
        {
            Debug.Log("🎯 Connected to server: " + serverIP);
            client = new TcpClient();
            try
            {
                await client.ConnectAsync(serverIP, port);
                stream = client.GetStream();
                Debug.Log("[Client] Connected to server: " + serverIP);
                stream.BeginRead(buffer, 0, buffer.Length, OnDataReceived, null);
            }
            catch (Exception e)
            {
                Debug.LogError("[Client] Connection failed: " + e.Message);
            }
        }
        else
        {
            Debug.LogWarning("❌ Could not find server in time.");
        }
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
        {
            OnCommandReceived?.Invoke(cmd);
        }
    }
}
