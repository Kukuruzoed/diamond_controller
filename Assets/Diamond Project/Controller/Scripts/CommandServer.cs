using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class CommandServer : MonoBehaviour
{
    public int port = 9000;

    private TcpListener listener;
    private List<TcpClient> clients = new List<TcpClient>();

    void Start()
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        listener.BeginAcceptTcpClient(OnClientConnected, null);
        Debug.Log($"TCP server {GetLocalIPAddress()} listening on port {port}");
    }

    void OnClientConnected(IAsyncResult ar)
    {
        TcpClient client = listener.EndAcceptTcpClient(ar);
        clients.Add(client);
        Debug.Log($"Client connected: {client.Client.RemoteEndPoint}");

        listener.BeginAcceptTcpClient(OnClientConnected, null);
    }

    public void SendCommandToAll(string command)
    {
        byte[] data = Encoding.UTF8.GetBytes(command);
        List<TcpClient> disconnected = new List<TcpClient>();

        foreach (var client in clients)
        {
            try
            {
                if (client.Connected)
                {
                    client.GetStream().Write(data, 0, data.Length);
                }
                else
                {
                    disconnected.Add(client);
                }
            }
            catch
            {
                disconnected.Add(client);
            }
        }

        foreach (var dead in disconnected)
        {
            clients.Remove(dead);
            dead.Close();
        }

        Debug.Log($"Sent command to {clients.Count} clients: {command}");
    }

    void OnApplicationQuit()
    {
        foreach (var c in clients) c.Close();
        listener.Stop();
    }
    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString(); // например: "192.168.0.42"
            }
        }
        return "IP not found";
    }
}
