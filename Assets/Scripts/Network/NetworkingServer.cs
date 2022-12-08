using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;

using AliveOpsArcade.OdinSerializer;
using UnityEngine.Networking.Types;

public class NetworkingServer : INetworking
{
    Thread serverThread;
    private object receiverLock;

    // Network
    private Socket serverSocket;
    EndPoint endPoint;
    private int channel1Port = 9050;
    private int channel2Port = 9051;

    public NetworkUser myNetworkUser { get; set; }

    // List that stores information about player states
    public List<NetworkUser> networkUserList { get; set; }

    // Dictionary to link an endpoint with a client (server not included)
    public Dictionary<EndPoint, int> clients;

    public bool triggerClientAdded { get; set; } = false;
    public bool triggerClientDisconected { get; set; } = false;
    public bool triggerLoadScene { get; set; } = false;

    public void Start()
    {
        receiverLock = new object();

        clients = new Dictionary<EndPoint, int>();

        InitializeSocket();
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

        Debug.Log("PLAYER IS SPAWNING");
        myNetworkUser.playerData.action = PlayerData.Action.CREATE;
        SpawnPlayer(myNetworkUser);

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

    public void OnPackageReceived(byte[] inputPacket, int recv, EndPoint fromAddress)
    {
        // Whenever a package is received, we want to parse the message
        ClientPacket packet = SerializationUtility.DeserializeValue<ClientPacket>(inputPacket, DataFormat.JSON);

        if (packet.type == PacketType.CLIENT_JOIN)
        {
            Debug.Log("[Client Data]" +
            " IP: " + packet.networkUser.connectToIP +
            " Client: " + packet.networkUser.isClient +
            " Username: " + packet.networkUser.username);

            // If the client that sent a message to this server is new, add it to the list of clients.
            if (!clients.ContainsKey(endPoint))
            {
                // Assign playerID and the action to perform respectively
                NetworkUser newClient = packet.networkUser;
                newClient.playerData.playerID = networkUserList.Count + 1;
                newClient.playerData.action = PlayerData.Action.CREATE;
                
                // TODO: ASSIGN ID TO NEW CLIENT
                clients.Add(endPoint, newClient.playerData.playerID);

                SpawnPlayer(newClient);
            }
        }
        //else if (packet.type == PacketType.WORLD_STATE)
        //{
        //    BroadcastPacket(inputPacket);
        //}
    }

    public void SpawnPlayer(NetworkUser networkUser)
    {
        networkUserList.Add(networkUser);

        ServerPacket packet = new ServerPacket(networkUserList, PacketType.CLIENT_JOIN);

        byte[] data = SerializationUtility.SerializeValue(packet, DataFormat.JSON);

        BroadcastPacket(data, false);

        // Trigger the onClientAddedEvent
        lock (receiverLock)
        {
            triggerClientAdded = true;
        }
    }

    public void BroadcastPacket(byte[] packet, bool fromClient = true) // Doesn't include the client that sent the packet
    {
        // Broadcast the message to the other clients
        foreach (KeyValuePair<EndPoint, int> entry in clients)
        {
            if (fromClient && entry.Key.Equals(endPoint))
                continue;

            SendPacket(packet, entry.Key);
        }
    }

    public void SendPacket(byte[] outputPacket, EndPoint toAddress)
    {
        if (clients.Count != 0)
        {
            serverSocket.SendTo(outputPacket, outputPacket.Length, SocketFlags.None, toAddress);
        }
    }

    public void LoadScene()
    {
        ServerPacket packet = new ServerPacket(networkUserList, PacketType.GAME_START);

        byte[] data = SerializationUtility.SerializeValue(packet, DataFormat.JSON);

        BroadcastPacket(data, false);

        lock (receiverLock)
        {
            triggerLoadScene = true;
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