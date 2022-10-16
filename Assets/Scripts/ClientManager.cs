using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ClientManager : MonoBehaviour
{
    private Socket socket;

    Thread serverThread;
    private int port = 3005;
    private int recv;

    // Start is called before the first frame update
    void Start()
    {
        InitializeSocket();
    }

    private void OnDestroy()
    {
        Debug.Log("Destroying Scene!!!!!!");

        socket.Close();
        serverThread.Abort();
    }

    private void InitializeSocket()
    {
        serverThread = new Thread(ClientSetup);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    private void ClientSetup()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
        EndPoint remote = (EndPoint)ipep;
        socket.Bind(ipep);

        byte[] data = new byte[64];
        data = Encoding.ASCII.GetBytes("Hola yeray eres tontico.");
        socket.SendTo(data, data.Length, SocketFlags.None, remote);
    }
}