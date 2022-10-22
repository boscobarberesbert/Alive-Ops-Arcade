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
        TCP
    }

    public Protocol m_Protocol = Protocol.UDP;

    Thread serverThread;

    // Network
    private Socket socket;
    private Socket clientSocket;
    EndPoint clientEndPointUDP;

    private int receivePort = 9050;
    private int sendPort = 9051;

    // Chat & Lobby
    public string serverName;

    string message = "";
    List<string> chatList;

    // Start is called before the first frame update
    void Start()
    {
        chatList = new List<string>();

        InitializeSocket();
    }

    private void OnDestroy()
    {
        Debug.Log("Destroying Scene");

        socket.Close();
        serverThread.Abort();
    }

    void OnGUI()
    {
        Rect rectObj = new Rect(40, 380, 200, 400);
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        GUI.Box(rectObj, "Chat", style);

        foreach (var chat in chatList)
        {
            Rect textRect = new Rect(40, 410 + 35 * chatList.IndexOf(chat), 200, 35);
            GUI.TextArea(textRect, chat);
        }

        message = GUI.TextField(new Rect(40, 600, 140, 20), message);
        if (GUI.Button(new Rect(190, 600, 40, 20), "send"))
        {
            if (m_Protocol == Protocol.UDP)
                SendChatMessageUDP(message + "\n");
            else if (m_Protocol == Protocol.TCP)
                SendChatMessageTCP(message + "\n");

            message = "";
        }
    }

    private void InitializeSocket()
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, receivePort);

        if (m_Protocol == Protocol.UDP)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            serverThread = new Thread(ServerSetupUDP);
        }
        else if (m_Protocol == Protocol.TCP)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            serverThread = new Thread(ServerSetupTCP);
        }

        socket.Bind(ipep);

        serverThread.IsBackground = true;
        serverThread.Start();
    }

    private void ServerSetupUDP()
    {
        Debug.Log("Server UDP initialized listening....");

        IPEndPoint clientIpep = new IPEndPoint(IPAddress.Any, sendPort);
        clientEndPointUDP = (EndPoint)(clientIpep);

        byte[] data = new byte[1024];
        int recv = socket.ReceiveFrom(data, ref clientEndPointUDP);
        Debug.Log(Encoding.ASCII.GetString(data, 0, recv));

        string welcomeMessage = "Welcome to the " + serverName + " server";
        data = Encoding.ASCII.GetBytes(welcomeMessage);
        socket.SendTo(data, data.Length, SocketFlags.None, clientEndPointUDP);

        while (true)
        {
            data = new byte[1024];
            recv = socket.ReceiveFrom(data, ref clientEndPointUDP);
            Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
            chatList.Add(Encoding.ASCII.GetString(data, 0, recv));
        }
    }

    private void ServerSetupTCP()
    {
        Debug.Log("Server TCP initialized listening....");

        socket.Listen(10);

        clientSocket = socket.Accept();
        IPEndPoint clientIpep = (IPEndPoint)clientSocket.RemoteEndPoint;

        Debug.Log("Connected with " + clientIpep.Address.ToString() + " at port: " + clientIpep.Port);

        byte[] data = new byte[1024];
        string welcomeMessage = "Welcome to the " + serverName + " server";
        data = Encoding.ASCII.GetBytes(welcomeMessage);
        clientSocket.Send(data, data.Length, SocketFlags.None);

        clientSocket.Close();
    }

    private void SendChatMessageUDP(string messageToSend)
    {
        byte[] data = Encoding.ASCII.GetBytes(messageToSend);
        socket.SendTo(data, data.Length, SocketFlags.None, clientEndPointUDP);
    }

    private void SendChatMessageTCP(string messageToSend)
    {
        byte[] data = Encoding.ASCII.GetBytes(messageToSend);
        clientSocket.Send(data, data.Length, SocketFlags.None);
    }
}