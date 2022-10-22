using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.Assertions;

public class ClientManager : MonoBehaviour
{
    public enum Protocol
    {
        UDP,
        TCP
    }

    public Protocol m_Protocol = Protocol.UDP;

    private Socket socketUDP;

    Thread clientThread;

    private int receivePort = 9050;
    private int sendPort = 9051;

    public string serverIP;

    // Start is called before the first frame update
    void Start()
    {
        InitializeSocket();
    }

    private void OnDestroy()
    {
        Debug.Log("Destroying Scene!!!!!!");

        socketUDP.Close();
        clientThread.Abort();
        //serverThread.Abort();
    }

    private void InitializeSocket()
    {
        socketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        clientThread = new Thread(ClientSetupUDP);
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private void ClientSetupUDP()
    {
        byte[] data = new byte[128];

        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(serverIP), receivePort);

        data = Encoding.ASCII.GetBytes("Hi server!");
        socketUDP.SendTo(data, data.Length, SocketFlags.None, ipep);

        IPEndPoint sendIpep = new IPEndPoint(IPAddress.Any, sendPort);
        EndPoint endPoint = (EndPoint)sendIpep;

        data = new byte[128];
        int recv = socketUDP.ReceiveFrom(data, ref endPoint);
        Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
    }
}