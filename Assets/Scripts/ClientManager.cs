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
    private int recv;

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

        //serverThread = new Thread(ServerSetupUDP);
        //serverThread.IsBackground = true;
        //serverThread.Start();
    }

    private void ClientSetupUDP()
    {
        try
        {
            Debug.Log(MainMenu.serverIp.ToString());
            Debug.Log("192.168.204.19");
            string ipaddr = MainMenu.serverIp.Substring(0,MainMenu.serverIp.Length -1 );

            Assert.AreEqual(ipaddr, "192.168.204.19");
            var ipparse = IPAddress.Parse(ipaddr);
            IPEndPoint serverIEP = new IPEndPoint(ipparse, serverPort);
            EndPoint remote = (EndPoint)serverIEP;

            byte[] data = new byte[64];
            data = Encoding.ASCII.GetBytes("Hola yeray eres tontico.");
            socketUDP.SendTo(data, data.Length, SocketFlags.None, remote);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        
    }

    private void ServerSetupUDP()
    {
        IPEndPoint clientIEP = new IPEndPoint(IPAddress.Any, clientPort);
        EndPoint remote = (EndPoint)clientIEP;
        socketUDP.Bind(clientIEP);
        byte[] data = new byte[64];
        recv = socketUDP.ReceiveFrom(data, ref remote);
        Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
    }
}