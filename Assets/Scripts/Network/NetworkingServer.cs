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
    public object playerMapLock { get; set; } = new object();
    public object loadSceneLock { get; set; } = new object();
    public object clientDisconnectLock { get; set; } = new object();

    // Network
    Socket serverSocket;
    EndPoint endPoint;
    int channel1Port = 9050;
    int channel2Port = 9051;

    // Dictionary to link a network ID (of a client) with an endpoint (server not included)
    Dictionary<string, EndPoint> clients;

    // Queue of received packets
    Queue<ClientPacket> packetQueue = new Queue<ClientPacket>();

    public User myUserData { get; set; }
    public PlayerObject myPlayerData { get; set; }
    public Dictionary<string, PlayerObject> playerMap { get; set; }

    public bool triggerClientDisconected { get; set; } = false;
    public bool triggerLoadScene { get; set; } = false;

    public void Start()
    {
        clients = new Dictionary<string, EndPoint>();

        playerMap = new Dictionary<string, PlayerObject>();

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

            if (!clients.ContainsKey(helloPacket.clientData.networkID))
                clients.Add(helloPacket.clientData.networkID, fromAddress);

            SpawnPlayer(helloPacket.clientData);
        }
        else if (packet.type == PacketType.WORLD_STATE)
        {
            ClientPacket clientPacket = SerializationUtility.DeserializeValue<ClientPacket>(inputPacket, DataFormat.JSON);

            packetQueue.Enqueue(clientPacket);

            //NetworkingManager.Instance.ProcessPacketQueue(ref packetQueue);

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

    public void SpawnPlayer(User userData)
    {
        // Add the player to our map (includes server)
        lock (playerMapLock)
        {
            Vector3 spawnPosition = new Vector3(NetworkingManager.Instance.startSpawnPosition.x + playerMap.Count * 3, 1, 0);
            playerMap.Add(userData.networkID, new PlayerObject(PlayerObject.Action.CREATE, spawnPosition, new Quaternion(0, 0, 0, 0)));
        }


        // If it's a client
        if (clients.ContainsKey(userData.networkID))
        {
            // TODO: Make sure all player are set to CREATE if the client is new

            // Prepare the packet to be sent back notify the connection
            ServerPacket packet = new ServerPacket(PacketType.WELCOME, playerMap);

            byte[] data = SerializationUtility.SerializeValue(packet, DataFormat.JSON);

            SendPacket(data, clients[userData.networkID]);
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

    public void SetActionInList(Dictionary<string, PlayerObject> playerMap, PlayerObject.Action action)
    {
        for (int i = 0; i < playerMap.Count; ++i)
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