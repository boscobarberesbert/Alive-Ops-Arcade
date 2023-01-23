using AliveOpsArcade.OdinSerializer;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class NetworkingClient : INetworking
{
    // Thread and safety
    Thread clientThread;

    public object packetQueueLock { get; set; } = new object();

    public object loadSceneLock { get; set; } = new object();
    public bool triggerLoadScene { get; set; } = false;

    public object clientDisconnectLock { get; set; } = new object();
    public bool triggerClientDisconected { get; set; } = false;

    // Network
    Socket clientSocket;
    IPEndPoint ipep;
    EndPoint endPoint;

    int channel1Port = 9050;
    int channel2Port = 9051;

    // User & Player data of the Client
    public User myUserData { get; set; }
    PlayerObject myPlayerObject;

    // Queue of received packets
    public Queue<Packet> packetQueue { get; set; }

    float elapsedPingTime = 0f;
    float pingTime = 30f;
    bool hasSceneLoaded = false;

    public void Start()
    {
        packetQueue = new Queue<Packet>();

        myPlayerObject = new PlayerObject();

        InitializeSocket();
    }

    // Initializes the socket as a client in UDP and starts the thread that is constantly listening for messages
    private void InitializeSocket()
    {
        Debug.Log("[CLIENT] Client Initializing...");
        Debug.Log("[CLIENT] Creating Socket...");
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        Debug.Log("[CLIENT] Socket created...");

        ipep = new IPEndPoint(IPAddress.Parse(myUserData.connectToIP), channel1Port);

        IPEndPoint sendIpep = new IPEndPoint(IPAddress.Any, channel2Port);
        endPoint = (EndPoint)sendIpep;

        clientThread = new Thread(ClientListener);
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    // It sends a hello message to the server and when the server responds with a welcome packet it goes into listening mode
    private void ClientListener()
    {
        // Sending hello packet with user data
        HelloPacket packet = new HelloPacket(myUserData);

        byte[] data = SerializationUtility.SerializeValue(packet, DataFormat.JSON);

        SendPacketToServer(data);

        Debug.Log("[CLIENT] Server started listening");

        while (true)
        {
            data = new byte[8192];
            int recv = clientSocket.ReceiveFrom(data, ref endPoint);

            // Call OnPackageReceived
            // Whenever a package is received, we want to parse the message
            OnPackageReceived(data, recv, endPoint);
        }
    }

    // Handles packages received
    public void OnPackageReceived(byte[] inputPacket, int recv, EndPoint fromAddress)
    {
        ServerPacket serverPacket = SerializationUtility.DeserializeValue<ServerPacket>(inputPacket, DataFormat.JSON);
        
        if (serverPacket.type == PacketType.GAME_START)
        {
            LoadScene("Game");
        }

        lock (packetQueueLock)
        {
            packetQueue.Enqueue(serverPacket);
        }
    }

    public void UpdateMyPlayerState()
    {
        if (NetworkingManager.Instance.myPlayerGO)
        {
            if (NetworkingManager.Instance.myPlayerGO.transform.position.x != myPlayerObject.position.x || NetworkingManager.Instance.myPlayerGO.transform.rotation != myPlayerObject.rotation || myPlayerObject.hasShot)
            {
                myPlayerObject.action = PlayerObject.Action.UPDATE;
                myPlayerObject.position = NetworkingManager.Instance.myPlayerGO.transform.position;
                myPlayerObject.rotation = NetworkingManager.Instance.myPlayerGO.transform.rotation;
                myPlayerObject.isRunning = NetworkingManager.Instance.myPlayerGO.GetComponent<PlayerController>().isMovementPressed;

                ClientPacket clientPacket = new ClientPacket(PacketType.WORLD_STATE, myUserData.networkID, myPlayerObject);
                byte[] dataToBroadcast = SerializationUtility.SerializeValue(clientPacket, DataFormat.JSON);

                SendPacketToServer(dataToBroadcast);

                myPlayerObject.hasShot = false;
            }
        }
    }

    public void OnUpdate()
    {
        UpdateMyPlayerState();
    }

    public void SendPacket(byte[] outputPacket, EndPoint toAddress)
    {
        clientSocket.SendTo(outputPacket, outputPacket.Length, SocketFlags.None, toAddress);
    }

    public void SendPacketToServer(byte[] outputPacket)
    {
        clientSocket.SendTo(outputPacket, outputPacket.Length, SocketFlags.None, ipep);
    }

    public void LoadScene(string sceneName)
    {
        lock (loadSceneLock)
        {
            triggerLoadScene = true;
        }
    }

    public void OnSceneLoaded()
    {
        if (!hasSceneLoaded)
        {
            HelloPacket packet = new HelloPacket(myUserData);

            byte[] data = SerializationUtility.SerializeValue(packet, DataFormat.JSON);

            SendPacketToServer(data);
            hasSceneLoaded = true;
        }
    }

    public void OnShoot()
    {
        myPlayerObject.hasShot = true;
    }

    public void OnConnectionReset(EndPoint fromAddress)
    {
        throw new System.NotImplementedException();
    }

    public void OnDisconnect()
    {
        Debug.Log("Destroying Scene");

        clientSocket.Close();
        clientThread.Abort();
    }

    public void reportError()
    {
        throw new System.NotImplementedException();
    }
}