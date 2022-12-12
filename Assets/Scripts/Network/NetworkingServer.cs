using AliveOpsArcade.OdinSerializer;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class NetworkingServer : INetworking
{
    Thread serverThread;
    private object receiverLock;

    // Network
    private Socket serverSocket;
    EndPoint endPoint;
    private int channel1Port = 9050;
    private int channel2Port = 9051;

    public NetworkUser myNetworkUser { get; set; } = new NetworkUser();

    // List that stores information about player states
    public List<NetworkUser> networkUserList { get; set; }

    // Dictionary to link an endpoint with a client networkID (server not included)
    public Dictionary<string, EndPoint> clients;

    public bool triggerClientAdded { get; set; } = false;
    public bool triggerClientDisconected { get; set; } = false;
    public bool triggerLoadScene { get; set; } = false;

    public void Start()
    {
        receiverLock = new object();

        clients = new Dictionary<string, EndPoint>();
        networkUserList = new List<NetworkUser>();

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

        Debug.Log("SERVER PLAYER IS SPAWNING");
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
            // Whenever a package is received, we want to parse the message
            OnPackageReceived(data, recv, endPoint);
        }
    }

    public void OnPackageReceived(byte[] inputPacket, int recv, EndPoint fromAddress)
    {
        ClientPacket packet = SerializationUtility.DeserializeValue<ClientPacket>(inputPacket, DataFormat.JSON);

        // If the client that sent a message to this server is new, add it to the list of clients.
        if (packet.type == PacketType.HELLO)
        {
            if (!clients.ContainsKey(packet.networkUser.networkID))
                clients.Add(packet.networkUser.networkID, fromAddress);

            SpawnPlayer(packet.networkUser);
        }
        else if (packet.type == PacketType.WORLD_STATE)
        {
            Debug.Log("[Client Data]" +
            " IP: " + packet.networkUser.connectToIP +
            " Client: " + packet.networkUser.isClient +
            " Username: " + packet.networkUser.username);

            if (packet.networkUser.player.action == DynamicObject.Action.UPDATE)
            {
                // TODO: update players from our world
                //BroadcastPacket(inputPacket);
            }
            else if (packet.networkUser.player.action == DynamicObject.Action.DESTROY)
            {
                // TODO: destroy players from our world
            }
            else
            {
                Debug.Log("[WARNING] Player Action is NONE.");
            }
        }
        else if (packet.type == PacketType.PING)
        {
            // TODO: What to do when we are pinged
        }
    }

    public void SpawnPlayer(NetworkUser networkUser)
    {
        // Add the user to our list of users (includes server)
        networkUserList.Add(networkUser);

        if (networkUser.isClient)
        {
            List<NetworkUser> welcomeUserList = new List<NetworkUser>(networkUserList);

            CreateWelcomePacket(welcomeUserList);

            // Prepare the packet to be sent notifying to spawn the necessary objects
            ServerPacket packet = new ServerPacket(PacketType.WELCOME, welcomeUserList);

            byte[] data = SerializationUtility.SerializeValue(packet, DataFormat.JSON);

            SendPacket(data, clients[networkUser.networkID]);
        }

        // Trigger the onClientAddedEvent
        lock (receiverLock)
        {
            triggerClientAdded = true;
        }
    }

    public void BroadcastPacket(byte[] data, bool fromClient = true) // True: doesn't include the client that sent the packet in the broadcast
    {
        // Broadcast the message to the other clients
        foreach (KeyValuePair<string, EndPoint> entry in clients)
        {
            if (fromClient && entry.Value.Equals(endPoint))
                continue;

            SendPacket(data, entry.Value);
        }
    }

    public void SendPacket(byte[] outputPacket, EndPoint toAddress)
    {
        if (clients.Count != 0)
        {
            serverSocket.SendTo(outputPacket, outputPacket.Length, SocketFlags.None, toAddress);
        }
    }

    public void CreateWelcomePacket(List<NetworkUser> userList)
    {
        for (int i = 0; i < userList.Count; ++i)
        {
            userList[i].player.action = DynamicObject.Action.CREATE;
        }
    }

    public void LoadScene()
    {
        // Notify that the game is going to start
        ServerPacket packet = new ServerPacket(PacketType.GAME_START, networkUserList);

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