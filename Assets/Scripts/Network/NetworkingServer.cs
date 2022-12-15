using AliveOpsArcade.OdinSerializer;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class NetworkingServer : INetworking
{
    // Thread and safety
    Thread serverThread;
    public object userListLock { get; set; } = new object();
    public object clientAddLock { get; set; } = new object();
    public object loadSceneLock { get; set; } = new object();
    public object clientDisconnectLock { get; set; } = new object();

    // Network
    Socket serverSocket;
    EndPoint endPoint;
    int channel1Port = 9050;
    int channel2Port = 9051;

    // Dictionary to link a client ID with an endpoint (server not included)
    Dictionary<int, EndPoint> clients;

    public UserData myUserData { get; set; }
    public DynamicObject myPlayer { get; set; }
    public Dictionary<int, string> playerMap { get; set; }

    public bool triggerClientAdded { get; set; } = false;
    public bool triggerClientDisconected { get; set; } = false;
    public bool triggerLoadScene { get; set; } = false;

    // Queue of received packets
    Queue<ClientPacket> packetQueue = new Queue<ClientPacket>();

    public void Start()
    {
        clients = new Dictionary<int, EndPoint>();

        playerMap = new Dictionary<int, string>();

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
        myUserData.clientID = playerMap.Count;
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
            // Whenever a package is received, we want to parse the message
            OnPackageReceived(data, recv, endPoint);
        }
    }

    public void OnPackageReceived(byte[] inputPacket, int recv, EndPoint fromAddress)
    {
        Packet packet = SerializationUtility.DeserializeValue<Packet>(inputPacket, DataFormat.JSON);

        // If the client that sent a message to this server is new, add it to the list of clients.
        if (packet.type == PacketType.HELLO)
        {
            HelloPacket helloPacket = SerializationUtility.DeserializeValue<HelloPacket>(inputPacket, DataFormat.JSON);

            // Assign a new client ID to the user
            helloPacket.clientData.clientID = playerMap.Count + 1;

            if (!clients.ContainsKey(helloPacket.clientData.clientID) && helloPacket.clientData.clientID == -1)
                clients.Add(helloPacket.clientData.clientID, fromAddress);

            SpawnPlayer(helloPacket.clientData);
        }
        else if (packet.type == PacketType.WORLD_STATE)
        {
            ClientPacket clientPacket = SerializationUtility.DeserializeValue<ClientPacket>(inputPacket, DataFormat.JSON);

            packetQueue.Enqueue(clientPacket);

            NetworkingManager.Instance.ProcessPacketQueue(ref packetQueue);

            // TODO: update players from our world
            //if (clientPacket.player.action == DynamicObject.Action.NONE)
            //    Debug.Log("[WARNING] Player Action is NONE.");
            //else
            //{
            //    int index = playerList.FindIndex(it => it.networkID == packet.player.networkID);

            //    if (index != -1)
            //    {
            //        playerList[index] = packet.player;
            //    }
            //    else
            //        Debug.Log("[ERROR] Player to be updated was not found in list.");
            //}
        }
        else if (packet.type == PacketType.PING)
        {
            // TODO: What to do when we are pinged
        }
    }

    public void SpawnPlayer(UserData user)
    {
        // Generate a networkID for its player
        string generatedNetID = System.Guid.NewGuid().ToString();

        // Add the user to our list of users (includes server)
        lock (userListLock)
        {
            playerMap.Add(user.clientID, generatedNetID);
        }

        // If it's a client
        if (clients.ContainsKey(user.clientID))
        {
            // Prepare the packet to be sent notifying the assigned IDs
            WelcomePacket packet = new WelcomePacket(user.clientID, generatedNetID);

            byte[] data = SerializationUtility.SerializeValue(packet, DataFormat.JSON);

            SendPacket(data, clients[user.clientID]);
        }
        // If it's the server
        else
        {
            myUserData.clientID = user.clientID;
            myPlayer.networkID = generatedNetID;
        }

        // Trigger the onClientAddedEvent
        lock (clientAddLock)
        {
            triggerClientAdded = true;
        }
    }

    public void BroadcastPacket(byte[] data, bool fromClient = true) // True: doesn't include the client that sent the packet in the broadcast
    {
        // Broadcast the message to the other clients
        foreach (KeyValuePair<int, EndPoint> entry in clients)
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

    public void SetActionInList(List<DynamicObject> players, DynamicObject.Action action)
    {
        for (int i = 0; i < players.Count; ++i)
        {
            players[i].action = action;
        }
    }

    public void LoadScene()
    {
        // Set all player objects to be created
        lock (userListLock)
        {
            // TODO
            //SetActionInList(playerList, DynamicObject.Action.CREATE);
        }

        // TODO: Notify that the game is going to start
        //ServerPacket packet = new ServerPacket(PacketType.GAME_START, playerList);

        //byte[] data = SerializationUtility.SerializeValue(packet, DataFormat.JSON);

        //BroadcastPacket(data, false);

        lock (loadSceneLock)
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

    void OnDisable()
    {
        Debug.Log("Destroying Scene");

        serverSocket.Close();
        serverThread.Abort();
    }
}