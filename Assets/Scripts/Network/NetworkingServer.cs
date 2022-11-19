using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using System.Text;


public class NetworkingServer : INetworking
{
    Thread serverThread;
    private object receiverLock;

    // Network
    private Socket serverSocket;
    EndPoint endPoint;
    private int channel1Port = 9050;
    private int channel2Port = 9051;

    public Dictionary<EndPoint, string> clients;

    public delegate void OnClientAdded();
    public event OnClientAdded onClientAdded;
    bool triggerOnClientAdded = false;

    public void Start()
    {
        receiverLock = new object();
        clients = new Dictionary<EndPoint,string>();
        InitializeSocket();
    }

    public void OnConnectionReset(EndPoint fromAddress)
    {
        throw new System.NotImplementedException();
    }

    public void OnDisconnect()
    {
        throw new System.NotImplementedException();
    }

    public void OnPackageReceived(byte[] inputPacket,int recv, EndPoint fromAddress)
    {
        //Whenever a package is received, we want to parse the message
        string receivedMessage = Encoding.ASCII.GetString(inputPacket, 0, recv);
        Debug.Log(receivedMessage);
        //If the client that sent a message to this server is new, add it to the list of clients.
        if (!clients.ContainsKey(endPoint))
        {
            clients.Add(endPoint, receivedMessage);
            receivedMessage += " joined the room.";
        }
        //Trigger the onClientAddedEvent
        lock(receiverLock)
        {
            triggerOnClientAdded = true;
        }
        Debug.Log("[SERVER] Message received: "+receivedMessage);
        //Broadcast the message to the other clients
        foreach (KeyValuePair<EndPoint, string> entry in clients)
        {
            if (!entry.Key.Equals(endPoint))
            {
               byte[] data = Encoding.ASCII.GetBytes(clients[endPoint] + ": " + receivedMessage);
                serverSocket.SendTo(data, data.Length, SocketFlags.None, entry.Key);
            }
        }
    }

    public void OnUpdate()
    {
        if(triggerOnClientAdded)
        {
            if(onClientAdded !=null)
            {
                onClientAdded();
            }
            triggerOnClientAdded = false;
        }
    }

    public void reportError()
    {
        throw new System.NotImplementedException();
    }

    public void SendPacket(byte[] outputPacket,EndPoint toAddress)
    {
        if(clients.Count!= 0)
        {
            serverSocket.SendTo(outputPacket, outputPacket.Length, SocketFlags.None, toAddress);
        }
    }

    private void InitializeSocket()
    {
        Debug.Log("[SERVER] Server Initializing...");
        Debug.Log("[SERVER] Creating Socket...");

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        Debug.Log("[SERVER] Socket Created...");

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, channel1Port);
        serverSocket.Bind(ipep);
        Debug.Log("[SERVER] Socket Binded...");

        endPoint = new IPEndPoint(IPAddress.Any, channel2Port);

        serverThread = new Thread(ServerListener);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    private void ServerListener()
    {
        Debug.Log("[SERVER] Server started listening");
        while (true)
        {
            //Listen for data
            byte[] data = new byte[1024];
            int recv = serverSocket.ReceiveFrom(data, ref endPoint);
           

            //Call OnPackageReceived
            OnPackageReceived(data,recv, endPoint);

        }
    }

}
