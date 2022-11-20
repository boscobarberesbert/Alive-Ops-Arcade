using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;

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

    // Map to link an endpoint with a client (server not included)
    public Dictionary<EndPoint, UserData> clients;

    // Dictionary to link a user with its PlayerID
    public LobbyState lobbyState { get; set; } = new LobbyState();
    public bool triggerClientAdded { get; set; }
    public UserData myUserData { get; set; } = new UserData();

    public void Start()
    {
        receiverLock = new object();
        clients = new Dictionary<EndPoint, UserData>();

        InitializeSocket();
    }

    public void OnPackageReceived(byte[] inputPacket, int recv, EndPoint fromAddress)
    {
        // Whenever a package is received, we want to parse the message
        Packet packet = SerializationUtility.DeserializeValue<Packet>(inputPacket, DataFormat.JSON);

        if (packet.type == Packet.PacketType.CLIENT_NEW)
        {
            UserData userData = SerializationUtility.DeserializeValue<UserData>(inputPacket, DataFormat.JSON);
            Debug.Log("[Client Data]" +
            " Type: " + userData.type +
            " IP: " + userData.connectToIP +
            " Client: " + userData.isClient +
            " Username: " + userData.username);

            // If the client that sent a message to this server is new, add it to the list of clients.
            if (!clients.ContainsKey(endPoint))
            {
                clients.Add(endPoint, userData);

                SpawnPlayer(userData);

            }
        }
    }

    private void BroadcastPacket(byte[] packet)
    {
        // Broadcast the message to the other clients
        foreach (KeyValuePair<EndPoint, UserData> entry in clients)
        {
            if (!entry.Key.Equals(endPoint))
            {
                serverSocket.SendTo(packet, packet.Length, SocketFlags.None, entry.Key);
            }
        }
    }

    public void SendPacket(byte[] outputPacket, EndPoint toAddress)
    {
        if (clients.Count != 0)
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
        SpawnPlayer(myUserData);
        serverThread = new Thread(ServerListener);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    private void ServerListener()
    {
        Debug.Log("[SERVER] Server started listening");

        while (true)
        {
            // Listen for data
            byte[] data = new byte[1024];
            int recv = serverSocket.ReceiveFrom(data, ref endPoint);

            Debug.Log("[SERVER] package received");

            // Call OnPackageReceived
            OnPackageReceived(data, recv, endPoint);
        }
    }

    public void SpawnPlayer(UserData userData)
    {
        lobbyState.players.Add(userData, lobbyState.players.Count);
        byte[] bytes = SerializationUtility.SerializeValue(lobbyState, DataFormat.JSON);

        // Broadcast the message to ALL the clients (including the one that was created)
        foreach (KeyValuePair<EndPoint, UserData> entry in clients)
        {
            SendPacket(bytes, entry.Key);
            //serverSocket.SendTo(bytes, bytes.Length, SocketFlags.None, entry.Key);
        }

        // Trigger the onClientAddedEvent
        lock (receiverLock)
        {
            triggerClientAdded = true;
        }
    }

    public void OnUpdate() { }

    public void reportError()
    {
        throw new System.NotImplementedException();
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

    private void OnDisable()
    {
        Debug.Log("Destroying Scene");

        serverSocket.Close();
        serverThread.Abort();
    }
}