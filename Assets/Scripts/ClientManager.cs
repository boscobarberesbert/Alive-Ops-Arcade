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

    private Socket socket;

    IPEndPoint ipep;
    EndPoint clientEndPointUDP;

    private int receivePort = 9050;
    private int sendPort = 9051;

    public string serverIP;
    public string userName;
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

        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        clientThread.Abort();
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
        ipep = new IPEndPoint(IPAddress.Parse(serverIP), receivePort);

        if (m_Protocol == Protocol.UDP)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            clientThread = new Thread(ClientSetupUDP);
        }
        else if (m_Protocol == Protocol.TCP)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            clientThread = new Thread(ClientSetupTCP);
        }

        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private void ClientSetupUDP()
    {
        IPEndPoint clientIpep = new IPEndPoint(IPAddress.Any, sendPort);
        clientEndPointUDP = (EndPoint)clientIpep;

        byte[] data = new byte[1024];
        data = Encoding.ASCII.GetBytes(userName + " joined the room.");
        socket.SendTo(data, data.Length, SocketFlags.None, ipep);

        while (true)
        {
            data = new byte[1024];
            int recv = socket.ReceiveFrom(data, ref clientEndPointUDP);
            Debug.Log("Message received: " + Encoding.ASCII.GetString(data, 0, recv));
            chatList.Add(Encoding.ASCII.GetString(data, 0, recv));
        }
    }

    private void ClientSetupTCP()
    {
        try
        {
            socket.Connect(ipep);
        }
        catch (SocketException e)
        {
            Debug.Log("Unable to connect to server.");
            Debug.Log(e.ToString());
        }

        byte[] data = new byte[1024];
        data = Encoding.ASCII.GetBytes(userName + " joined the room.");
        socket.Send(data, data.Length, SocketFlags.None);

        while (true)
        {
            data = new byte[1024];
            int recv = socket.Receive(data);
            Debug.Log("Message received: " + Encoding.ASCII.GetString(data, 0, recv));
            chatList.Add(Encoding.ASCII.GetString(data, 0, recv));
        }
    }

    private void SendChatMessageUDP(string messageToSend)
    {
        byte[] data = Encoding.ASCII.GetBytes(messageToSend);
        socket.SendTo(data, data.Length, SocketFlags.None, ipep);
    }

    private void SendChatMessageTCP(string messageToSend)
    {
        //byte[] data = Encoding.ASCII.GetBytes(messageToSend);
        //socketUDP.SendTo(data, data.Length, SocketFlags.None, ipep);
    }
}