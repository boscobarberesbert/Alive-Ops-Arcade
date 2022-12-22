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
    public object clientDisconnectLock { get; set; } = new object();

    // Network
    Socket clientSocket;
    IPEndPoint ipep;
    EndPoint endPoint;

    int channel1Port = 9050;
    int channel2Port = 9051;

    // User & Players
    public User myUserData { get; set; }
    public Dictionary<string, PlayerObject> playerMap { get; set; }
    PlayerObject myPlayerData; // Data to send

    public bool triggerClientDisconected { get; set; } = false;
    public bool triggerLoadScene { get; set; } = false;

    float elapsedUpdateTime = 0f;

    float elapsedPingTime = 0f;
    float pingTime = 30f;

    public void Start()
    {
        playerMap = new Dictionary<string, PlayerObject>();
        myPlayerData = new PlayerObject();

        InitializeSocket();
    }

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

    public void OnPackageReceived(byte[] inputPacket, int recv, EndPoint fromAddress)
    {
        ServerPacket serverPacket = SerializationUtility.DeserializeValue<ServerPacket>(inputPacket, DataFormat.JSON);

        if (serverPacket.type == PacketType.WELCOME)
        {
            lock (playerMapLock)
            {
                playerMap = serverPacket.playerMap;
            }
        }
        else if (serverPacket.type == PacketType.WORLD_STATE)
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
        if (NetworkingManager.Instance.myPlayerGO.GetComponent<PlayerController>().isMovementPressed)
        {
            myPlayerData.action = PlayerObject.Action.UPDATE;
            myPlayerData.position = NetworkingManager.Instance.myPlayerGO.transform.position;
            myPlayerData.rotation = NetworkingManager.Instance.myPlayerGO.transform.rotation;
        }
    }

    public void OnUpdate()
    {
        elapsedUpdateTime += Time.deltaTime;
        if (elapsedUpdateTime >= NetworkingManager.Instance.updateTime)
        {
            ClientPacket packet = new ClientPacket(PacketType.WORLD_STATE, myUserData.networkID, myPlayerData);

            byte[] data = SerializationUtility.SerializeValue(packet, DataFormat.JSON);

            SendPacketToServer(data);
            elapsedUpdateTime = elapsedUpdateTime % 1f;
        }

        elapsedPingTime += Time.deltaTime;
        if (elapsedPingTime >= pingTime)
        {
            elapsedPingTime = elapsedPingTime % 1f;
            //PingServer();
        }
    }

    public void OnConnectionReset(EndPoint fromAddress)
    {
        throw new System.NotImplementedException();
    }

    public void SendPacket(byte[] outputPacket, EndPoint toAddress)
    {
        clientSocket.SendTo(outputPacket, outputPacket.Length, SocketFlags.None, toAddress);
    }

    public void SendPacketToServer(byte[] outputPacket)
    {
        clientSocket.SendTo(outputPacket, outputPacket.Length, SocketFlags.None, ipep);
    }

    public void OnDisconnect()
    {
        clientSocket.Close();
        clientThread.Abort();
    }
    public void reportError()
    {
        throw new System.NotImplementedException();
    }

    public void PingServer()
    {
        byte[] packet = Encoding.ASCII.GetBytes("Hello");
        SendPacketToServer(packet);
    }

    private void OnDisable()
    {
        Debug.Log("Destroying Scene");

        clientSocket.Close();
        clientThread.Abort();
    }
}