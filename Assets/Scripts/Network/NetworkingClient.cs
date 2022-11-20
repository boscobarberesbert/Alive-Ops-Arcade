using AliveOpsArcade.OdinSerializer;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : INetworking
{
    Thread clientThread;

    // Network
    private Socket clientSocket;
    private object receiverLock;

    IPEndPoint ipep;
    EndPoint endPoint;

    private int channel1Port = 9050;
    private int channel2Port = 9051;

    public bool triggerClientAdded { get; set; } = false;
    public bool triggerLoadScene { get; set; } = false;

    public UserData myUserData { get; set; } = new UserData();
    public LobbyState lobbyState { get; set; } = new LobbyState();

    public void Start()
    {
        receiverLock = new object();

        InitializeSocket();
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

    public void OnPackageReceived(byte[] inputPacket, int recv, EndPoint fromAddress)
    {
        Packet packet = SerializationUtility.DeserializeValue<Packet>(inputPacket, DataFormat.JSON);

        // Whenever a package is received, we want to parse the message
        if (packet.type == Packet.PacketType.LOBBY_STATE)
        {
            LobbyState lobbyState = SerializationUtility.DeserializeValue<LobbyState>(inputPacket, DataFormat.JSON);
            this.lobbyState = lobbyState;
            foreach (KeyValuePair<UserData, int> player in lobbyState.players)
            {
                Debug.Log("[Client Data] Type: " + player.Key.type +
                            " IP: " + player.Key.connectToIP +
                            " Client: " + player.Key.isClient +
                            " Username: " + player.Key.username);
            }

            lock (receiverLock)
            {
                triggerClientAdded = true;
            }
        }
        else if (packet.type == Packet.PacketType.GAME_START)
        {
            lock (receiverLock)
            {
                triggerLoadScene = true;
            }
        }
    }

    public void OnUpdate() { }

    public void reportError()
    {
        throw new System.NotImplementedException();
    }

    public void SendPacket(byte[] outputPacket, EndPoint toAddress)
    {
        clientSocket.SendTo(outputPacket, outputPacket.Length, SocketFlags.None, toAddress);
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
        byte[] data = myUserData.SerializeJson();

        clientSocket.SendTo(data, data.Length, SocketFlags.None, ipep);

        Debug.Log("[CLIENT] Server started listening");

        while (true)
        {
            data = new byte[5024];
            int recv = clientSocket.ReceiveFrom(data, ref endPoint);

            // Call OnPackageReceived
            OnPackageReceived(data, recv, endPoint);
        }
    }

    private void OnDisable()
    {
        Debug.Log("Destroying Scene");

        clientSocket.Close();
        clientThread.Abort();
    }
}