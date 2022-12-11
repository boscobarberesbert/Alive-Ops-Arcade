using AliveOpsArcade.OdinSerializer;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetworkingClient : INetworking
{
    Thread clientThread;

    // Network
    private Socket clientSocket;
    private object receiverLock;

    IPEndPoint ipep;
    EndPoint endPoint;

    private int channel1Port = 9050;
    private int channel2Port = 9051;

    public NetworkUser myNetworkUser { get; set; }
    public List<NetworkUser> networkUserList { get; set; }

    public bool triggerClientAdded { get; set; } = false;
    public bool triggerClientDisconected { get; set; } = false;
    public bool triggerLoadScene { get; set; } = false;

    float elapsedTime = 0f;
    float pingTime = 30f;

    public void Start()
    {
        receiverLock = new object();

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
        ClientPacket packet = new ClientPacket(myNetworkUser, PacketType.PLAYER_JOIN);

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

        if (packet.type == PacketType.PLAYER_JOIN)
        {
            lock (receiverLock)
            {
                triggerClientAdded = true;
            }
            networkUserList = packet.networkUserList;
        }
        else if (packet.type == PacketType.WORLD_STATE)
        {
            foreach (var player in packet.networkUserList)
            {
                Debug.Log("[Client Data] ID: " + player.networkID +
                            " | IP: " + player.connectToIP +
                            " | Client: " + player.isClient +
                            " | Username: " + player.username);

                if (player.playerData.action == PlayerData.Action.UPDATE)
                {
                    // TODO: update players from our world

                    // This is not viable
                    //else if (packet.type == PacketType.WORLD_STATE)
                    //{
                    //    myPlayerData = SerializationUtility.DeserializeValue<PlayerData>(inputPacket, DataFormat.JSON);
                    //}
                }
                else if (player.playerData.action == PlayerData.Action.DESTROY)
                {
                    // TODO: destroy players from our world
                }
                else
                {
                    Debug.Log("[WARNING] Player Action is NONE.");
                }
            }
            networkUserList = packet.networkUserList;
        }
        else if (packet.type == PacketType.GAME_START)
        {
            lock (receiverLock)
            {
                triggerLoadScene = true;
            }
        }
        else if (packet.type == PacketType.PING)
        {
            // TODO: What to do when we are pinged
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