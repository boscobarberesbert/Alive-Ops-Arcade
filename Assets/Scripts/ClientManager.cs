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
    Dictionary<string,string> chat;
    Vector2 scrollPosition;

    // Start is called before the first frame update
    void Start()
    {
        chat = new Dictionary<string, string>();

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
        //Rect rectObj = new Rect(40, 380, 200, 400);
        //GUIStyle style = new GUIStyle();
        //style.alignment = TextAnchor.UpperLeft;
        //GUI.Box(rectObj, "Chat", style);

        //foreach (var message in chat)
        //{
        //    Rect textRect = new Rect(40, 410 + 35 * chatList.IndexOf(chat), 200, 35);
        //    GUI.TextArea(textRect, chat);
        //}

        //message = GUI.TextField(new Rect(40, 600, 140, 20), message);
        //if (GUI.Button(new Rect(190, 600, 40, 20), "send"))
        //{
        //    if (m_Protocol == Protocol.UDP)
        //        SendChatMessageUDP(message + "\n");
        //    else if (m_Protocol == Protocol.TCP)
        //        SendChatMessageTCP(message + "\n");

        //    message = "";
        //}
        GUILayout.BeginArea(new Rect(Screen.width / 2, Screen.height / 2, 300, 300));
        GUILayout.BeginVertical();
        scrollPosition = GUILayout.BeginScrollView(
            scrollPosition, GUILayout.Width(Screen.width - 100), GUILayout.Height(Screen.height - 100));
        foreach (var message in chat)
        {
            if (message.Key.Contains("server"))
            {
                GUILayout.TextArea("server: " + message.Value);
            }
            else
            {
                GUILayout.TextArea("client: " + message.Value);

            }
        }
        GUILayout.EndScrollView();
        message = GUILayout.TextField(message);
        if (GUILayout.Button("Send"))
        {
            if (m_Protocol == Protocol.UDP)
            {
                SendChatMessageUDP(message + "\n");
            }
            else
            {
                SendChatMessageTCP(message + "\n");

            }
            message = "";
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();
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
        chat.Add("client+" + System.Guid.NewGuid().ToString(), userName + " joined the room.");
        data = new byte[1024];
        int recv = socket.ReceiveFrom(data, ref clientEndPointUDP);
        Debug.Log("Message received: " + Encoding.ASCII.GetString(data, 0, recv));
        chat.Add("server+" + System.Guid.NewGuid().ToString(), Encoding.ASCII.GetString(data, 0, recv));
        while (true)
        {
            data = new byte[1024];
            int recv = socket.ReceiveFrom(data, ref clientEndPointUDP);
            Debug.Log("Message received: " + Encoding.ASCII.GetString(data, 0, recv));
            chat.Add("server+" + System.Guid.NewGuid().ToString(), Encoding.ASCII.GetString(data, 0, recv));
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
            chat.Add("server+" + System.Guid.NewGuid().ToString(), Encoding.ASCII.GetString(data, 0, recv));
        }
    }

    private void SendChatMessageUDP(string messageToSend)
    {
        byte[] data = Encoding.ASCII.GetBytes(messageToSend);
        socket.SendTo(data, data.Length, SocketFlags.None, ipep);
        chat.Add("client+" + System.Guid.NewGuid().ToString(), messageToSend);
    }

    private void SendChatMessageTCP(string messageToSend)
    {
        byte[] data = Encoding.ASCII.GetBytes(messageToSend);
        socket.Send(data, data.Length, SocketFlags.None);
        chat.Add("client+" + System.Guid.NewGuid().ToString(), messageToSend);

    }
}