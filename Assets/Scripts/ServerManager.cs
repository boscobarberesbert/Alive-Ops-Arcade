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
    Thread clientThread;

    private Socket socketUDP;
    private Socket socketTDP;

    private int serverPort = 9050;
    private int clientPort = 9051;

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
        clientThread.Abort();
    }

    private void InitializeSocket()
    {
        Debug.Log("INITIALIZE THREAD");

        socketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        serverThread = new Thread(ServerSetupUDP);
        serverThread.IsBackground = true;
        serverThread.Start();

        clientThread = new Thread(ClientSetupUDP);
        clientThread.IsBackground = true;
        clientThread.Start();

        //if (m_Protocol == Protocol.UDP)
        //{
        //    serverThread = new Thread(ServerSetupUDP);
        //}
        //else if(m_Protocol == Protocol.TDP)
        //{
        //    serverThread = new Thread(ServerSetupTDP);
        //}
    }

    void ServerSetupUDP()
    {
        IPEndPoint clientIEP = new IPEndPoint(IPAddress.Any, clientPort);
        EndPoint remote = (EndPoint)(clientIEP);

        socketUDP.Bind(clientIEP);

        byte[] data = new byte[64];
        int recv = socketUDP.ReceiveFrom(data, ref remote);
        Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
    }

    void ClientSetupUDP()
    {
        IPEndPoint serverIEP = new IPEndPoint(IPAddress.Parse("192.168.204.20"), serverPort);
        EndPoint remote = (EndPoint)(serverIEP);

        byte[] data = new byte[64];
        data = Encoding.ASCII.GetBytes("Server Name is SI");
        socketUDP.SendTo(data, data.Length, SocketFlags.None, remote);
    }

    //void ServerSetupTDP()
    //{

    //}
}