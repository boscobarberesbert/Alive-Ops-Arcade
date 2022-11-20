using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using AliveOpsArcade.OdinSerializer;

public class NetworkingServer : INetworking
{
    Thread serverThread;
    private object receiverLock;

    // Network
    private Socket serverSocket;
    EndPoint endPoint;
    private int channel1Port = 9050;
    private int channel2Port = 9051;

    public Dictionary<EndPoint, string> clients; //Map to link an endpoint with a client (server not included)
    public Dictionary<string, int> players; //Map to link a user with a game object (including server)


    public bool triggerClientAdded { get; set; }

    public void Start()
    {
        receiverLock = new object();
        clients = new Dictionary<EndPoint,string>();
        players = new Dictionary<string,int>();
        InitializeSocket();
    }

    public void OnConnectionReset(EndPoint fromAddress)
    {
        throw new System.NotImplementedException();
    }

    public void OnDisconnect()
    {
        serverSocket.Close();
        serverThread.Abort();
        
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
            SpawnPlayer(receivedMessage);
            receivedMessage += "joined the room.";
            //Trigger the onClientAddedEvent
            lock (receiverLock)
            {
                triggerClientAdded = true;
            }
            BroadcastPacket(Encoding.ASCII.GetBytes(receivedMessage));
        }
        else
        {
            Debug.Log("[SERVER] Message received: " + receivedMessage);
            BroadcastPacket(inputPacket);
        }
        
       
    }
    private void BroadcastPacket(byte[] packet)
    {
        //Broadcast the message to the other clients
        foreach (KeyValuePair<EndPoint, string> entry in clients)
        {
            if (!entry.Key.Equals(endPoint))
            {
         
                serverSocket.SendTo(packet, packet.Length, SocketFlags.None, entry.Key);
            }
        }
    }
    public void OnUpdate()
    {
        
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
        SpawnPlayer(MainMenuInfo.username);
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

            Debug.Log("[SERVER] package received");
            //Call OnPackageReceived
            OnPackageReceived(data,recv, endPoint);

        }
    }

    public void SpawnPlayer(string username)
    {
        players.Add(username,players.Count);
        byte[] bytes = SerializationUtility.SerializeValue(players, DataFormat.Binary);
        
       
        //Broadcast the message to ALL the clients (including the one that was created)
        foreach (KeyValuePair<EndPoint, string> entry in clients)
        {
          
                serverSocket.SendTo(bytes, bytes.Length, SocketFlags.None, entry.Key);
         
        }
    }

}
