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
    public enum Protocol { 
        UDP,
        TCP
    }
    public Protocol m_Protocol = Protocol.UDP;
    
    private Socket socketUDP;

    Thread clientThread;
    Thread serverThread;
    private int serverPort = 9050;
    private int clientPort = 9051;

    public string clientIP;
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
        serverThread.Abort();
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
        //Debug.Log(MainMenu.serverIp.ToString());
        //Debug.Log("192.168.204.19");
        //string ipaddr = MainMenu.serverIp.Substring(0,MainMenu.serverIp.Length -1 );

        //Assert.AreEqual(ipaddr, "192.168.204.19");
        //var ipparse = IPAddress.Parse(ipaddr);
        IPEndPoint serverIPEP = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
        EndPoint remote = (EndPoint)serverIPEP;

        try
        {
            byte[] data = new byte[64];
            data = Encoding.ASCII.GetBytes(clientIP);
            socketUDP.SendTo(data, data.Length, SocketFlags.None, remote);
            Debug.Log("CLIENT HAS SENT MESSAGE");

            // Start server thread
            serverThread = new Thread(ServerSetupUDP);
            serverThread.IsBackground = true;
            serverThread.Start();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void ServerSetupUDP()
    {
        IPEndPoint serverIPEP = new IPEndPoint(IPAddress.Parse(serverIP), clientPort);
        EndPoint remote = (EndPoint)serverIPEP;

        bool hasFinishedRoom = false; // We'll finish the chat with a button or a player left.

        while (!hasFinishedRoom)
        {
            byte[] data = new byte[64];
            int recv = socketUDP.ReceiveFrom(data, ref remote);
            Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
            Debug.Log("CLIENT HAS RECEIVED MESSAGE");
        }
    }
}