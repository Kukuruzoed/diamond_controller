using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public class CommandServer : MonoBehaviour
{
    public int port = 9001;

    private TcpListener listener;
    private List<TcpClient> clients = new List<TcpClient>();

    public UnityEvent<string> OnClientConnected = new UnityEvent<string>();
    public UnityEvent<string> OnClientDisconnected = new UnityEvent<string>();
    public UnityEvent<int> OnConnectedClientCountChanged = new UnityEvent<int>();
    public UnityEvent OnCommandSended = new UnityEvent();

    private int clientsConnected = 0;

    void Start()
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        listener.BeginAcceptTcpClient(OnClientConnect, null);
        Debug.Log($"TCP server {GetLocalIPAddress()} listening on port {port}");
        StartCoroutine(CheckConnectedClients());
    }

    void OnClientConnect(IAsyncResult ar)
    {
        try
        {
            TcpClient client = listener.EndAcceptTcpClient(ar);
            clients.Add(client);
            Debug.Log($"Client connected: {client.Client.RemoteEndPoint}");
            listener.BeginAcceptTcpClient(OnClientConnect, null);
            OnClientConnected?.Invoke(client.Client.RemoteEndPoint.ToString());
        }
        catch { }
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
        OnCommandSended?.Invoke();
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

    void OnApplicationQuit()
    {
        foreach (var client in clients)
        {
            try { client?.Close(); } catch { }
        }

        try { listener?.Stop(); } catch { }

        Debug.Log("[Server] Shutdown complete");
    }

    private IEnumerator CheckConnectedClients()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            Dictionary<string, bool> connectedIPs = new Dictionary<string, bool>(); 
            foreach (var client in clients)
            {
                try
                {
                    if (client.Connected)
                    {
                        string endPoint = client.Client.RemoteEndPoint.ToString();
                        string ip = endPoint.Substring(0, endPoint.IndexOf(":"));
                        connectedIPs.Add(ip, true);
                    }
                }
                catch
                { }
            }
            if(clientsConnected != connectedIPs.Count)
            {
                clientsConnected = connectedIPs.Count;
                OnConnectedClientCountChanged?.Invoke(clientsConnected);
            }
        }
    }
}
