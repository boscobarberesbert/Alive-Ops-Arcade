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

    // Network
    private Socket clientSocket;

    IPEndPoint ipep;
    EndPoint endPoint;

    private int channel1Port = 9050;
    private int channel2Port = 9051;

    public string username;
    public string serverIP;

    // Start is called before the first frame update
    void Start()
    {
        InitializeSocket();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitializeSocket()
    {
        serverIP = MainMenu.serverIp.Substring(0, MainMenu.serverIp.Length - 1);
        Debug.Log(serverIP);

        username = MainMenu.username.Substring(0, MainMenu.username.Length - 1);
        Debug.Log(username);

        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        ipep = new IPEndPoint(IPAddress.Parse(serverIP), channel1Port);

        IPEndPoint sendIpep = new IPEndPoint(IPAddress.Any, channel2Port);
        endPoint = (EndPoint)sendIpep;

        clientThread = new Thread(ClientSetup);
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private void ClientSetup()
    {
        byte[] data = new byte[1024];
        //data = Encoding.ASCII.GetBytes("");
        //clientSocket.SendTo(data, data.Length, SocketFlags.None, ipep);

        while (true)
        {
            data = new byte[1024];
            int recv = clientSocket.ReceiveFrom(data, ref endPoint);

            Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
        }
    }

    private void OnDisable()
    {
        Debug.Log("Destroying Scene");

        clientSocket.Close();
        clientThread.Abort();
    }
}
