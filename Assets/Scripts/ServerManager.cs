using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ServerManager : MonoBehaviour
{
    Thread serverThread;

    Socket socket;

    byte[] data = new byte[1024];
    int recv;

    // Start is called before the first frame update
    void Start()
    {
        InitializeThread();
    }

    private void OnDestroy()
    {
        Debug.Log("Destroying Scene");

        socket.Close();
        serverThread.Abort();
    }

    private void InitializeThread()
    {
        Debug.Log("INITIALIZE THREAD");

        serverThread = new Thread(ServerSetup);
        //serverThread.IsBackground = true;
        serverThread.Start();
    }

    void ServerSetup()
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        socket.Bind(ipep);

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)(sender);

        string welcome = "Welcome to my test server";
        data = Encoding.ASCII.GetBytes(welcome);
        socket.SendTo(data, data.Length, SocketFlags.None, Remote);

        recv = socket.ReceiveFrom(data, ref Remote);
        Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
    }
}