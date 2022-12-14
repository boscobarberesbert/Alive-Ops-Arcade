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
    public object _userListLock { get; set; } = new object();
    public object _clientAddLock { get; set; } = new object();
    public object _loadSceneLock { get; set; } = new object();
    public object _clientDisconnectLock { get; set; } = new object();

    // Network
    Socket clientSocket;
    IPEndPoint ipep;
    EndPoint endPoint;

    int channel1Port = 9050;
    int channel2Port = 9051;

    public NetworkUser myNetworkUser { get; set; }

    // List that stores information about player states
    public List<NetworkUser> networkUserList { get; set; }

    public bool triggerClientAdded { get; set; } = false;
    public bool triggerClientDisconected { get; set; } = false;
    public bool triggerLoadScene { get; set; } = false;

    float elapsedTime = 0f;
    float pingTime = 30f;

    public void Start()
    {
        networkUserList = new List<NetworkUser>();

        InitializeSocket();
    }

    private void InitializeSocket()
    {
        Debug.Log("[CLIENT] Client Initializing...");
        Debug.Log("[CLIENT] Creating Socket...");
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        Debug.Log("[CLIENT] Socket created...");

        ipep = new IPEndPoint(IPAddress.Parse(myNetworkUser.connectToIP), channel1Port);

        IPEndPoint sendIpep = new IPEndPoint(IPAddress.Any, channel2Port);
        endPoint = (EndPoint)sendIpep;

        clientThread = new Thread(ClientListener);
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private void ClientListener()
    {
        // Sending hello packet with user data
        ClientPacket packet = new ClientPacket(PacketType.HELLO, myNetworkUser);

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
        ServerPacket packet = SerializationUtility.DeserializeValue<ServerPacket>(inputPacket, DataFormat.JSON);

        if (packet.type == PacketType.WELCOME)
        {
            lock (_clientAddLock)
            {
                triggerClientAdded = true;
            }
        }
        else if (packet.type == PacketType.WORLD_STATE)
        {
            foreach (var user in packet.networkUserList)
            {
                Debug.Log("[Client Data] ID: " + user.networkID + " | IP: " + user.connectToIP +
                    " | Client: " + user.isClient + " | Username: " + user.username);
            }
        }
        else if (packet.type == PacketType.GAME_START)
        {
            lock (_loadSceneLock)
            {
                triggerLoadScene = true;
            }
        }
        else if (packet.type == PacketType.PING)
        {
            // TODO: What to do when we are pinged
        }

        lock (_userListLock)
        {
            networkUserList = packet.networkUserList;
        }
    }

    public void OnUpdate()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= pingTime)
        {
            elapsedTime = elapsedTime % 1f;
            PingServer();
        }
    }

    public void PingServer()
    {
        byte[] packet = Encoding.ASCII.GetBytes("Hello");
        SendPacketToServer(packet);
    }

    // TODO: IS IT NECESSARY?
    public void SendPacket(byte[] outputPacket, EndPoint toAddress)
    {
        clientSocket.SendTo(outputPacket, outputPacket.Length, SocketFlags.None, toAddress);
    }

    public void SendPacketToServer(byte[] outputPacket)
    {
        clientSocket.SendTo(outputPacket, outputPacket.Length, SocketFlags.None, ipep);
    }

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
        clientSocket.Close();
        clientThread.Abort();
    }
    private void OnDisable()
    {
        Debug.Log("Destroying Scene");

        clientSocket.Close();
        clientThread.Abort();
    }
}