using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ServerManager : MonoBehaviour
{
    public enum Protocol
    {
        UDP,
        TDP
    }

    public Protocol m_Protocol = Protocol.UDP;

    Thread serverThread;

    private Socket socketUDP;
    //private Socket socketTDP;

    private int receivePort = 9050;
    private int sendPort = 9051;

    // Start is called before the first frame update
    void Start()
    {
        InitializeSocket();
    }

    private void OnDestroy()
    {
        Debug.Log("Destroying Scene");

        socketUDP.Close();
        serverThread.Abort();
        //clientThread.Abort();
    }

    private void InitializeSocket()
    {
        Debug.Log("INITIALIZE THREAD");

        socketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        serverThread = new Thread(ServerSetupUDP);
        serverThread.IsBackground = true;
        serverThread.Start();

    }

    public void ServerSetupUDP()
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, receivePort);
        socketUDP.Bind(ipep);

        Debug.Log("Server initialized listening....");

        IPEndPoint sendIpep = new IPEndPoint(IPAddress.Any, sendPort); //Maybe the port must be different??
        EndPoint endPoint = (EndPoint)(sendIpep);

        byte[] data = new byte[128];
        int recv = socketUDP.ReceiveFrom(data, ref endPoint);
        Debug.Log(Encoding.ASCII.GetString(data, 0, recv));

        data = Encoding.ASCII.GetBytes("Hello Client");
        socketUDP.SendTo(data, recv, SocketFlags.None, endPoint);

        // while (true)
        // {
        //     data = new byte[128];
        //     recv = socketUDP.ReceiveFrom(data, ref endPoint);
        //     Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
        //     socketUDP.SendTo(data, recv, SocketFlags.None, endPoint);
        // }
    }
}