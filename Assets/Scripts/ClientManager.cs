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

    Thread clientThread;

    private Socket socketUDP;

    IPEndPoint ipep;
    EndPoint endPoint;

    private int receivePort = 9050;
    private int sendPort = 9051;

    public string serverIP;
    public string userName;
    string message = "";

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

    void OnGUI()
    {
        Rect rectObj = new Rect(40, 380, 200, 400);
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        GUI.Box(rectObj, "Hello madafaka", style);

        message = GUI.TextField(new Rect(40, 420, 140, 20), message);
        if (GUI.Button(new Rect(190, 420, 40, 20), "send"))
        {
            SendChatMessage(message + "\n");
        }
    }

    private void InitializeSocket()
    {
        socketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        ipep = new IPEndPoint(IPAddress.Parse(serverIP), receivePort);

        IPEndPoint sendIpep = new IPEndPoint(IPAddress.Any, sendPort);
        endPoint = (EndPoint)sendIpep;

        clientThread = new Thread(ClientSetupUDP);
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private void ClientSetupUDP()
    {
        byte[] data = new byte[1024];
        data = Encoding.ASCII.GetBytes(userName + " joined the room.");
        socketUDP.SendTo(data, data.Length, SocketFlags.None, ipep);

        while (true)
        {
            data = new byte[1024];
            int recv = socketUDP.ReceiveFrom(data, ref endPoint);
            Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
        }
    }

    private void SendChatMessage(string messageToSend)
    {
        byte[] data = Encoding.ASCII.GetBytes(messageToSend);
        socketUDP.SendTo(data, data.Length, SocketFlags.None, ipep);
    }
}