using System.Net.Sockets;
using System.Text;
using System;
using UnityEngine;

public class SimpleTcpClient : MonoBehaviour
{
    public string serverIP = "192.168.3.85"; // Укажи IP сервера
    public int port = 9000;

    private TcpClient client;
    private NetworkStream stream;
    private byte[] buffer = new byte[1024];

    public Renderer targetRenderer; // Объект, которому будем менять цвет

    async void Start()
    {
        client = new TcpClient();
        try
        {
            await client.ConnectAsync(serverIP, port);
            stream = client.GetStream();
            stream.BeginRead(buffer, 0, buffer.Length, OnDataReceived, null);
            Debug.Log("[Client] Connected to server: " + serverIP);
        }
        catch (Exception e)
        {
            Debug.LogError("[Client] Connection failed: " + e.Message);
        }
    }

    void OnDataReceived(IAsyncResult ar)
    {
        int bytesRead = stream.EndRead(ar);
        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Debug.Log("[Client] Received command: " + message);

        HandleCommand(message);
        stream.BeginRead(buffer, 0, buffer.Length, OnDataReceived, null);
    }

    void HandleCommand(string cmd)
    {
        if (cmd == "jump")
        {
            transform.position += Vector3.up * 2f;
        }
        else if (cmd.StartsWith("color:"))
        {
            string colorName = cmd.Substring("color:".Length);
            Color color;
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}