using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPServer : MonoBehaviour
{
    Thread serverThread;
    private object chatLock;

    // Network
    private Socket serverSocket;
    EndPoint endPoint;
    private int channel1Port = 9050;
    private int channel2Port = 9051;

    Dictionary<EndPoint, string> clients;

    // Chat & Lobby
    public string serverName;
    string message = "";
    List<ChatMessage> chat;
    Vector2 scrollPosition = new Vector2(0, 0);

    // Start is called before the first frame update
    void Start()
    {
        chat = new List<ChatMessage>();
        chatLock = new object();

        clients = new Dictionary<EndPoint, string>();

        InitializeSocket();
    }

    private void InitializeSocket()
    {
        Debug.Log("INITIALIZE THREAD");

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, channel1Port);
        serverSocket.Bind(ipep);

        endPoint = new IPEndPoint(IPAddress.Any, channel2Port);

        serverThread = new Thread(ServerRoomBroadcast);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    private void ServerRoomBroadcast()
    {
        Debug.Log("Server initialized listening...");

        while (true)
        {
            byte[] data = new byte[1024];
            int recv = serverSocket.ReceiveFrom(data, ref endPoint);
            string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

            if (!clients.ContainsKey(endPoint))
            {
                // As it is a new client, the received message is its username
                clients.Add(endPoint, receivedMessage);

                receivedMessage += " joined the room.";

                data = Encoding.ASCII.GetBytes("Welcome to the " + serverName);
                serverSocket.SendTo(data, data.Length, SocketFlags.None, endPoint);
            }

            lock (chatLock)
            {
                chat.Add(new ChatMessage("client", receivedMessage, clients[endPoint]));
            }
            Debug.Log(receivedMessage);

            foreach (KeyValuePair<EndPoint, string> entry in clients)
            {
                if (!entry.Key.Equals(endPoint))
                {
                    data = Encoding.ASCII.GetBytes(clients[endPoint] + ": " + receivedMessage);
                    serverSocket.SendTo(data, data.Length, SocketFlags.None, entry.Key);
                }
            }
        }
    }

    private void SendChatMessage(string messageToSend)
    {
        byte[] data = Encoding.ASCII.GetBytes(messageToSend);
        serverSocket.SendTo(data, data.Length, SocketFlags.None, endPoint);
        lock (chatLock)
        {
            chat.Add(new ChatMessage("server", messageToSend, serverName));
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
            foreach (var c in chat)
            {
                if (c.sender.Contains("server"))
                    style.alignment = TextAnchor.MiddleRight;
                else
                    style.alignment = TextAnchor.MiddleLeft;

                GUILayout.BeginHorizontal(style);
                GUILayout.Label(c.username);
                GUILayout.EndHorizontal();

                GUILayout.Label(c.message, style);
            }
        }

        GUILayout.EndScrollView();

        GUILayout.BeginVertical();
        GUILayout.Label("Write your message:");
        message = GUILayout.TextField(message);
        GUILayout.EndVertical();

        if (GUILayout.Button("Send"))
        {
            SendChatMessage(message + "\n");
            message = "";
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void OnDestroy()
    {
        Debug.Log("Destroying Scene");

        serverSocket.Close();
        serverThread.Abort();
    }
}