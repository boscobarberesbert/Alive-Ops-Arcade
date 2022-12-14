using AliveOpsArcade.OdinSerializer;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetworkingClient : INetworking
{
    // Thread and safety
    Thread clientThread;
    public object playerMapLock { get; set; } = new object();

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

    // User & Players
    public User myUserData { get; set; }
    public Dictionary<string, PlayerObject> playerMap { get; set; }

    // Data to send to the server
    PlayerObject myPlayerData;

    // Queue of received packets
    //Queue<ServerPacket> packetQueue = new Queue<ServerPacket>();

    float elapsedPingTime = 0f;
    float pingTime = 30f;

    public void Start()
    {
        //Defining myPlayerData
        myPlayerData = new PlayerObject();

        //Defining playerMap
        playerMap = new Dictionary<string, PlayerObject>();


        InitializeSocket();
    }

    //Initializes the socket as a client in UDP and starts the thread that is constantly listening for messages
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

    //It sends a hello message to the server and when the server responds with a welcome packet it goes into listening mode
    private void ClientListener()
    {
        // Sending hello packet with user data
        HelloPacket packet = new HelloPacket(myUserData);

        byte[] data = SerializationUtility.SerializeValue(packet, DataFormat.JSON);

        SendPacketToServer(data);

        Debug.Log("[CLIENT] Server started listening");

        while (true)
        {
            data = new byte[5024];
            int recv = clientSocket.ReceiveFrom(data, ref endPoint);

            // Call OnPackageReceived
            // Whenever a package is received, we want to parse the message
            OnPackageReceived(data, recv, endPoint);
        }
    }

    //handles packages received
    public void OnPackageReceived(byte[] inputPacket, int recv, EndPoint fromAddress)
    {
        ServerPacket serverPacket = SerializationUtility.DeserializeValue<ServerPacket>(inputPacket, DataFormat.JSON);

        if (serverPacket.type == PacketType.WELCOME || serverPacket.type == PacketType.WORLD_STATE)
        {
            lock (playerMapLock)
            {
                playerMap = serverPacket.playerMap;
            }
        }
        else if (serverPacket.type == PacketType.GAME_START)
        {
            lock (playerMapLock)
            {
                playerMap = serverPacket.playerMap;
            }

            lock (loadSceneLock)
            {
                triggerLoadScene = true;
            }
        }
        else if (serverPacket.type == PacketType.PING)
        {
            // TODO: What to do when we are pinged
        }
    }

    public void UpdatePlayerState()
    {
        if (NetworkingManager.Instance.myPlayerGO)
        {
            if (NetworkingManager.Instance.myPlayerGO.transform.position.x != playerMap[myUserData.networkID].position.x || NetworkingManager.Instance.myPlayerGO.transform.rotation != playerMap[myUserData.networkID].rotation)
            {
                playerMap[myUserData.networkID].action = PlayerObject.Action.UPDATE;
                playerMap[myUserData.networkID].position = NetworkingManager.Instance.myPlayerGO.transform.position;
                playerMap[myUserData.networkID].rotation = NetworkingManager.Instance.myPlayerGO.transform.rotation;

                ClientPacket clientPacket = new ClientPacket(PacketType.WORLD_STATE, myUserData.networkID, playerMap[myUserData.networkID]);
                byte[] dataToBroadcast = SerializationUtility.SerializeValue(clientPacket, DataFormat.JSON);

                SendPacketToServer(dataToBroadcast);
            }
        }
    }

    public void OnUpdate()
    {
        //elapsedPingTime += Time.deltaTime;
        //if (elapsedPingTime >= pingTime)
        //{
        //    elapsedPingTime = elapsedPingTime % 1f;
        //    PingServer();
        //}
    }

    public void SendPacket(byte[] outputPacket, EndPoint toAddress)
    {
        clientSocket.SendTo(outputPacket, outputPacket.Length, SocketFlags.None, toAddress);
    }

    public void SendPacketToServer(byte[] outputPacket)
    {
        clientSocket.SendTo(outputPacket, outputPacket.Length, SocketFlags.None, ipep);
    }

    public void PingServer()
    {
        Packet pingPacket = new Packet(PacketType.PING);
        byte[] data = SerializationUtility.SerializeValue(pingPacket, DataFormat.JSON);
        SendPacketToServer(data);
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