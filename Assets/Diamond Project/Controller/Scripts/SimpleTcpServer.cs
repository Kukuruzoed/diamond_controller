// --- TCP SERVER (управляющее устройство) ---
// Запустить в Unity на ПК или Android

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class SimpleTcpServer : MonoBehaviour
{
    public int port = 9000;
    private TcpListener listener;
    private List<TcpClient> clients = new List<TcpClient>();

    void Start()
    {
        string localIP = GetLocalIPAddress();
        Debug.Log("[Server] Local IP address: " + localIP);

        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        listener.BeginAcceptTcpClient(OnClientConnected, null);
        Debug.Log("[Server] Listening on port " + port);
    }
    string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "IP not found";
    }
    void OnClientConnected(IAsyncResult ar)
    {
        TcpClient client = listener.EndAcceptTcpClient(ar);
        clients.Add(client);
        Debug.Log("[Server] Client connected from: " + client.Client.RemoteEndPoint);

        listener.BeginAcceptTcpClient(OnClientConnected, null);
    }

    public void SendToAll(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        foreach (var client in clients)
        {
            if (client.Connected)
            {
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
            }
        }
        Debug.Log("[Server] Sent to all: " + message);
    }

    void OnApplicationQuit()
    {
        foreach (var client in clients)
            client.Close();
        listener.Stop();
    }
}
