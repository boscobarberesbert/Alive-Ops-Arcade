using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TCPClient : MonoBehaviour
{
    Thread clientThread;
    private object chatLock;

    // Network
    private Socket serverSocket;

    IPEndPoint ipep;

    private int port = 9050;

    // Lobby & Chat
    public string serverIP;
    public string username;

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
        Debug.Log(MainMenu.serverIp.ToString());
        serverIP = MainMenu.serverIp.Substring(0, MainMenu.serverIp.Length - 1);

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        ipep = new IPEndPoint(IPAddress.Parse(serverIP), port);

        clientThread = new Thread(ClientSetupTCP);
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private void ClientSetupTCP()
    {
        try
        {
            serverSocket.Connect(ipep);
        }
        catch (SocketException e)
        {
            Debug.Log("Unable to connect to server.");
            Debug.Log(e.ToString());
        }

        byte[] data = new byte[1024];
        data = Encoding.ASCII.GetBytes(username + " joined the room.");
        serverSocket.Send(data, data.Length, SocketFlags.None);

        while (true)
        {
            data = new byte[1024];
            int recv = serverSocket.Receive(data);
            Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
            lock (chatLock)
            {
                chat.Add(new ChatMessage("server", Encoding.ASCII.GetString(data, 0, recv), username));
            }
        }
    }

    private void SendChatMessage(string messageToSend)
    {
        byte[] data = Encoding.ASCII.GetBytes(messageToSend);
        serverSocket.Send(data, data.Length, SocketFlags.None);
        lock (chat)
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

                GUILayout.Label(chatEntry.message, style);
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

        serverSocket.Close();
        clientThread.Abort();
    }
}