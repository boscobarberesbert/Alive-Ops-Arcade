using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPClient : MonoBehaviour
{
    Thread clientThread;
    private object chatLock;

    // Network
    private Socket clientSocket;

    IPEndPoint ipep;
    EndPoint endPoint;

    private int channel1Port = 9050;
    private int channel2Port = 9051;

    // Lobby & Chat
    public string serverIP;
    public string username;
    private string serverName;

    string message = "";
    List<ChatMessage> chat;
    Vector2 scrollPosition;

    // Start is called before the first frame update
    void Start()
    {
        chat = new List<ChatMessage>();
        chatLock = new object();

        InitializeSocket();
    }

    private void InitializeSocket()
    {
        serverIP = MainMenu.serverIp.Substring(0, MainMenu.serverIp.Length - 1);
        Debug.Log(serverIP);

        username = MainMenu.username.ToString();
        Debug.Log(username);

        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        ipep = new IPEndPoint(IPAddress.Parse(serverIP), channel1Port);

        IPEndPoint sendIpep = new IPEndPoint(IPAddress.Any, channel2Port);
        endPoint = (EndPoint)sendIpep;

        clientThread = new Thread(ClientSetupUDP);
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private void ClientSetupUDP()
    {
        byte[] data = new byte[1024];
        data = Encoding.ASCII.GetBytes(username);
        clientSocket.SendTo(data, data.Length, SocketFlags.None, ipep);

        data = new byte[1024];
        int recv = clientSocket.ReceiveFrom(data, ref endPoint);
        serverName = Encoding.ASCII.GetString(data, 0, recv);
        Debug.Log(serverName);

        lock (chatLock)
        {
            chat.Add(new ChatMessage("server", "Welcome to the " + serverName, serverName));
        }

        while (true)
        {
            data = new byte[1024];
            recv = clientSocket.ReceiveFrom(data, ref endPoint);
            Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
            lock (chatLock)
            {
                chat.Add(new ChatMessage("server", Encoding.ASCII.GetString(data, 0, recv), serverName));
            }
        }
    }

    private void SendChatMessage(string messageToSend)
    {
        byte[] data = Encoding.ASCII.GetBytes(messageToSend);
        clientSocket.SendTo(data, data.Length, SocketFlags.None, ipep);
        lock (chatLock)
        {
            chat.Add(new ChatMessage("client", messageToSend, username));
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 225, Screen.height / 2 - 111, 450, 222));
        GUILayout.BeginVertical();

        lock (chatLock)
        {
            scrollPosition = GUILayout.BeginScrollView(
               new Vector2(0, scrollPosition.y + chat.Count), GUI.skin.box, GUILayout.Width(450), GUILayout.Height(100));

            GUIStyle style = GUI.skin.textArea;
            foreach (var chatEntry in chat)
            {
                if (chatEntry.senderType.Contains("server"))
                    style.alignment = TextAnchor.MiddleLeft;
                else
                    style.alignment = TextAnchor.MiddleRight;

                GUILayout.Label(chatEntry.senderName + ": " + chatEntry.message, style);
            }
        }

        GUILayout.EndScrollView();
        GUILayout.BeginVertical();
        GUILayout.Label("Write your message:");
        message = GUILayout.TextField(message);
        GUILayout.EndVertical();

        if (GUILayout.Button("Send") && message != "")
        {
            SendChatMessage(message + "\n");
            message = "";
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void OnDisable()
    {
        Debug.Log("Destroying Scene");

        clientSocket.Close();
        clientThread.Abort();
    }
}