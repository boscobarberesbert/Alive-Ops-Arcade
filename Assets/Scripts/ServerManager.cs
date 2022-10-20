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

    private string clientIP;

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
        IPEndPoint serverIEP = new IPEndPoint(IPAddress.Any, serverPort);
        EndPoint remote = (EndPoint)(serverIEP);

        socketUDP.Bind(serverIEP);

        bool hasStartedClientThread = false;
        bool hasFinishedRoom = false; // We'll finish the chat with a button or a player left.

        while(!hasFinishedRoom)
        {
            byte[] data = new byte[64];
            int recv = socketUDP.ReceiveFrom(data, ref remote);
            Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
            Debug.Log("CLIENT HAS SENT IP");

            if (!hasStartedClientThread && recv > 0)
            {
                hasStartedClientThread = true;
                clientIP = Encoding.ASCII.GetString(data, 0, recv);

                // Start client thread
                clientThread = new Thread(ClientSetupUDP);
                clientThread.IsBackground = true;
                clientThread.Start();
            }
        }
    }

    void ClientSetupUDP()
    {
        IPEndPoint clientIEP = new IPEndPoint(IPAddress.Parse(/*"192.168.204.20"*/clientIP), clientPort);
        EndPoint remote = (EndPoint)(clientIEP);

        try
        {
            byte[] data = new byte[64];
            data = Encoding.ASCII.GetBytes("I am the SERVER!!!");
            socketUDP.SendTo(data, data.Length, SocketFlags.None, remote);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    //void ServerSetupTDP()
    //{
        
    //}
}