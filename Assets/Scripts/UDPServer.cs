using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPServer : MonoBehaviour
{
    Thread serverThread;

    // Network
    private Socket serverSocket;
    EndPoint endPoint;
    private int channel1Port = 9050;
    private int channel2Port = 9051;

    Dictionary<EndPoint, string> clients;

    public string serverName;

    // Start is called before the first frame update
    void Start()
    {
        clients = new Dictionary<EndPoint, string>();

        InitializeSocket();
    }

    private void InitializeSocket()
    {
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, channel1Port);
        serverSocket.Bind(ipep);

        endPoint = new IPEndPoint(IPAddress.Any, channel2Port);

        serverThread = new Thread(ServerRoomBroadcast);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ServerRoomBroadcast()
    {
        while (true)
        {
            byte[] data = new byte[1024];
            int recv = serverSocket.ReceiveFrom(data, ref endPoint);
            string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

            Debug.Log(receivedMessage);

            foreach (KeyValuePair<EndPoint, string> entry in clients)
            {
                if (!entry.Key.Equals(endPoint))
                {
                    data = Encoding.ASCII.GetBytes(clients[endPoint] + ": " + receivedMessage);
                    serverSocket.SendTo(data, data.Length, SocketFlags.None, entry.Key);
                }
            }
        }
    }

    private void OnDisable()
    {
        Debug.Log("Destroying Scene");

        serverSocket.Close();
        serverThread.Abort();
    }
}
