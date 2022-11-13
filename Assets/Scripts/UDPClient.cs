using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPClient : MonoBehaviour
{
    Thread clientThread;
    private object chatLock;

    // Network
    private Socket clientSocket;

    IPEndPoint ipep;
    EndPoint endPoint;

    private int channel1Port = 9050;
    private int channel2Port = 9051;




    // Start is called before the first frame update
    void Start()
    {
        chatLock = new object();

        InitializeSocket();
    }

    private void InitializeSocket()
    {

        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        string serverIP = PlayerData.connectToIP.Substring(0,PlayerData.connectToIP.Length - 1);

        ipep = new IPEndPoint(IPAddress.Parse(serverIP), channel1Port);

        IPEndPoint sendIpep = new IPEndPoint(IPAddress.Any, channel2Port);
        endPoint = (EndPoint)sendIpep;

        clientThread = new Thread(ClientSetupUDP);
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private void ClientSetupUDP()
    {
        byte[] data = new byte[1024];
        data = Encoding.ASCII.GetBytes(PlayerData.username);
        clientSocket.SendTo(data, data.Length, SocketFlags.None, ipep);

        data = new byte[1024];
        int recv = clientSocket.ReceiveFrom(data, ref endPoint);
        string serverName = Encoding.ASCII.GetString(data, 0, recv);
        Debug.Log(serverName);

        lock (chatLock)
        {
        }

        while (true)
        {
            data = new byte[1024];
            recv = clientSocket.ReceiveFrom(data, ref endPoint);
            Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
            lock (chatLock)
            {
            }
        }
    }

    private void SendChatMessage(string messageToSend)
    {
        byte[] data = Encoding.ASCII.GetBytes(messageToSend);
        clientSocket.SendTo(data, data.Length, SocketFlags.None, ipep);
        lock (chatLock)
        {
        }
    }


    private void OnDisable()
    {
        Debug.Log("Destroying Scene");

        clientSocket.Close();
        clientThread.Abort();
    }
}