using AliveOpsArcade.OdinSerializer;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class NetworkingServer : INetworking
{
    class EndPointState
    {
        public EndPoint ep;
        public bool isPrepared;

        public EndPointState(EndPoint ep, bool isPrepared)
        {
            this.ep = ep;
            this.isPrepared = isPrepared;
        }
    }

    // Thread and safety
    Thread serverThread;
    public object playerMapLock { get; set; } = new object();

    public object loadSceneLock { get; set; } = new object();
    public bool triggerLoadScene { get; set; } = false;

    public object clientDisconnectLock { get; set; } = new object();
    public bool triggerClientDisconected { get; set; } = false;

    // Network
    Socket serverSocket;
    EndPoint endPoint;

    int channel1Port = 9050;
    int channel2Port = 9051;

    // UserData & Players
    public User myUserData { get; set; }
    public Dictionary<string, PlayerObject> playerMap { get; set; }

    // Dictionary to link a network ID (of a client) with an endpoint (server not included)
    Dictionary<string, EndPointState> clients;

    // Queue of received packets
    //Queue<ClientPacket> packetQueue = new Queue<ClientPacket>();

    int totalNumPlayers = 0;

    float elapsedUpdateTime = 0f;

    public void Start()
    {
        clients = new Dictionary<string, EndPointState>();

        playerMap = new Dictionary<string, PlayerObject>();

        InitializeSocket();
    }

    //Initializes the server socket in UDP and starts the thread that is constantly listening for messages
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

    //Servers starts listening and when a package is received it executes OnPackageReceived function
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
    //handles packages received
    public void OnPackageReceived(byte[] inputPacket, int recv, EndPoint fromAddress)
    {
        Packet packet = SerializationUtility.DeserializeValue<Packet>(inputPacket, DataFormat.JSON);

        // If the client that sent a message to this server is new, add it to the list of clients.
        if (packet.type == PacketType.HELLO)
        {
            HelloPacket helloPacket = SerializationUtility.DeserializeValue<HelloPacket>(inputPacket, DataFormat.JSON);

            if (!clients.ContainsKey(helloPacket.clientData.networkID))
                clients.Add(helloPacket.clientData.networkID, new EndPointState(fromAddress, false));

            SpawnPlayer(helloPacket.clientData);
        }
        else if (packet.type == PacketType.WORLD_STATE)
        {
            ClientPacket clientPacket = SerializationUtility.DeserializeValue<ClientPacket>(inputPacket, DataFormat.JSON);

            lock (playerMapLock)
            {
                if (playerMap.ContainsKey(clientPacket.networkID))
                    playerMap[clientPacket.networkID] = clientPacket.playerObject;
                else
                    playerMap.Add(clientPacket.networkID, clientPacket.playerObject);
            }

            // TODO: process update packets
            //packetQueue.Enqueue(clientPacket);
        }
        else if (packet.type == PacketType.PING)
        {
            // TODO: What to do when we are pinged
        }
    }

    public void UpdatePlayerState()
    {
        if (NetworkingManager.Instance.myPlayerGO)
        {
            if (NetworkingManager.Instance.myPlayerGO.GetComponent<PlayerController>().isMovementPressed || NetworkingManager.Instance.myPlayerGO.GetComponent<MouseAim>().isAiming)
            {
                NetworkingManager.Instance.playerMap[myUserData.networkID].action = PlayerObject.Action.UPDATE;
                NetworkingManager.Instance.playerMap[myUserData.networkID].position = NetworkingManager.Instance.myPlayerGO.transform.position;
                NetworkingManager.Instance.playerMap[myUserData.networkID].rotation = NetworkingManager.Instance.myPlayerGO.transform.rotation;
            }
        }
    }

    public void OnUpdate()
    {
        elapsedUpdateTime += Time.deltaTime;
        if (elapsedUpdateTime >= NetworkingManager.Instance.updateTime)
        {
            ServerPacket serverPacket = new ServerPacket(PacketType.WORLD_STATE, NetworkingManager.Instance.playerMap);

            byte[] dataBroadcast = SerializationUtility.SerializeValue(serverPacket, DataFormat.JSON);

            BroadcastPacket(dataBroadcast, false);

            elapsedUpdateTime = elapsedUpdateTime % 1f;
        }
    }

    public void SendPacket(byte[] outputPacket, EndPoint toAddress)
    {
        if (clients.Count != 0)
        {
            serverSocket.SendTo(outputPacket, outputPacket.Length, SocketFlags.None, toAddress);
        }
    }

    public void BroadcastPacket(byte[] data, bool fromClient) // True: doesn't include the client that sent the packet in the broadcast
    {
        // Broadcast the message to the other clients
        foreach (var entry in clients)
        {
            if (fromClient && entry.Value.Equals(endPoint) && !entry.Value.isPrepared)
                continue;

            SendPacket(data, entry.Value.ep);
        }
    }

    public void SpawnPlayer(User userData)
    {
        lock (playerMapLock)
        {
            // Add the player to our map (includes server player)
            Vector3 spawnPosition = new Vector3(NetworkingManager.Instance.startSpawnPosition.x + totalNumPlayers * 3, 1, 0);
            ++totalNumPlayers;

            playerMap.Add(userData.networkID, new PlayerObject(PlayerObject.Action.CREATE, spawnPosition, new Quaternion(0, 0, 0, 0)));
        }
    }

    public void NotifySpawn(string networkID)
    {
        // If it's a client
        if (clients.ContainsKey(networkID))
        {
            clients[networkID].isPrepared = true;

            // Broadcast to all active clients that a player has joined
            if (clients.Count != 0)
            {
                ServerPacket serverPacket = new ServerPacket(PacketType.WORLD_STATE, NetworkingManager.Instance.playerMap);

                byte[] dataBroadcast = SerializationUtility.SerializeValue(serverPacket, DataFormat.JSON);

                BroadcastPacket(dataBroadcast, true);
            }

            // A copy of the players' map to be sent to the new client
            Dictionary<string, PlayerObject> welcomePlayerMap = new Dictionary<string, PlayerObject>();

            foreach (var entry in NetworkingManager.Instance.playerMap)
            {
                // Set all player objects to be created
                PlayerObject newObj = new PlayerObject(PlayerObject.Action.CREATE, entry.Value.position, entry.Value.rotation);
                welcomePlayerMap.Add(entry.Key, newObj);
            }

            // Prepare the packet to be sent back notifying the connection
            ServerPacket welcomePacket = new ServerPacket(PacketType.WELCOME, welcomePlayerMap);

            byte[] dataWelcome = SerializationUtility.SerializeValue(welcomePacket, DataFormat.JSON);

            SendPacket(dataWelcome, clients[networkID].ep);
        }
    }

    public void NotifySceneChange(string sceneName)
    {
        // TODO: add the scene to where we change
        lock (playerMapLock)
        {
            ServerPacket serverPacket = new ServerPacket(PacketType.GAME_START, NetworkingManager.Instance.playerMap);

            byte[] data = SerializationUtility.SerializeValue(serverPacket, DataFormat.JSON);

            BroadcastPacket(data, false);
        }
    }

    public void OnConnectionReset(EndPoint fromAddress)
    {
        throw new System.NotImplementedException();
    }

    public void OnDisconnect()
    {
        Debug.Log("Destroying Scene");

        serverSocket.Close();
        serverThread.Abort();
    }

    public void reportError()
    {
        throw new System.NotImplementedException();
    }
}