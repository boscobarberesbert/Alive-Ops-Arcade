using AliveOpsArcade.OdinSerializer;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
public class NetworkClient : INetworking
{
    Thread clientThread;

    // Network
    private Socket clientSocket;

    IPEndPoint ipep;
    EndPoint endPoint;

    private int channel1Port = 9050;
    private int channel2Port = 9051;

    public bool triggerClientAdded { get; set; }

    public UserData myUserData = new UserData();

    public void Start()
    {
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

    public void OnPackageReceived(byte[] inputPacket,int recv, EndPoint fromAddress)
    {
        Dictionary<string,int> players = SerializationUtility.DeserializeValue<Dictionary<string,int>>(inputPacket, DataFormat.JSON);
        foreach (KeyValuePair<string, int> kvp in players)
            Debug.Log("Key = "+ kvp.Key+ " Value = " + kvp.Value);
    }

    public void OnUpdate()
    {
        //throw new System.NotImplementedException();
    }

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
        //Sending hello packet with username
        byte[] data = new byte[1024];
        data = Encoding.ASCII.GetBytes(myUserData.username);
        clientSocket.SendTo(data, data.Length, SocketFlags.None, ipep);

        Debug.Log("[CLIENT] Server started listening");
        while (true)
        {
            data = new byte[5024];
            int recv = clientSocket.ReceiveFrom(data, ref endPoint);

            //Call OnPackageReceived
            OnPackageReceived(data, recv, endPoint);
        }
    }

}
