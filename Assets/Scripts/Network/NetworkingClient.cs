using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor.PackageManager;
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
        throw new System.NotImplementedException();
    }

    public void OnPackageReceived(byte[] inputPacket,int recv, EndPoint fromAddress)
    {
        string receivedMessage = Encoding.ASCII.GetString(inputPacket, 0, recv);
        Debug.Log(receivedMessage);
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


        ipep = new IPEndPoint(IPAddress.Parse(PlayerData.connectToIP), channel1Port);

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
        data = Encoding.ASCII.GetBytes(PlayerData.username);
        clientSocket.SendTo(data, data.Length, SocketFlags.None, ipep);

        Debug.Log("[CLIENT] Server started listening");
        while (true)
        {
            data = new byte[1024];
            int recv = clientSocket.ReceiveFrom(data, ref endPoint);

            //Call OnPackageReceived
            OnPackageReceived(data, recv, endPoint);
        }
    }

}
