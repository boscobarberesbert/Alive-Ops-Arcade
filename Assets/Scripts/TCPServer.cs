using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TCPServer : MonoBehaviour
{
    Thread serverThread;

    // Network
    private Socket serverSocket;
    private Socket clientSocket;

    private int channel1Port = 9050;

    // Chat & Lobby
    public string serverName;
    string message = "";
    List<ChatMessage> chat;
    Vector2 scrollPosition;

    // Start is called before the first frame update
    void Start()
    {
        chat = new List<ChatMessage>();

        InitializeSocket();
    }

    private void InitializeSocket()
    {
        Debug.Log("INITIALIZE THREAD");

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, channel1Port);
        serverSocket.Bind(ipep);

        serverThread = new Thread(ServerSetupTCP);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    private void ServerSetupTCP()
    {
        Debug.Log("Server initialized listening...");

        serverSocket.Listen(10);

        clientSocket = serverSocket.Accept();
        IPEndPoint clientIpep = (IPEndPoint)clientSocket.RemoteEndPoint;

        Debug.Log("Connected with " + clientIpep.Address.ToString() + " at port: " + clientIpep.Port);

        byte[] data = new byte[1024];
        int recv = clientSocket.Receive(data);
        Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
        chat.Add(new ChatMessage("client", Encoding.ASCII.GetString(data, 0, recv)));

        data = Encoding.ASCII.GetBytes("Welcome to the " + serverName);
        clientSocket.Send(data, data.Length, SocketFlags.None);

        while (true)
        {
            data = new byte[1024];
            recv = clientSocket.Receive(data);
            Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
            chat.Add(new ChatMessage("client", Encoding.ASCII.GetString(data, 0, recv)));
        }
    }

    private void SendChatMessage(string messageToSend)
    {
        byte[] data = Encoding.ASCII.GetBytes(messageToSend);
        serverSocket.Send(data, data.Length, SocketFlags.None);
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 225, Screen.height / 2 - 111, 450, 222));
        GUILayout.BeginVertical();
        scrollPosition = GUILayout.BeginScrollView(
           new Vector2(0, scrollPosition.y + chat.Count), GUI.skin.box, GUILayout.Width(450), GUILayout.Height(100));

        foreach (var c in chat)
        {
            if (c.sender.Contains("server"))
            {
                GUIStyle style = GUI.skin.textArea;
                style.alignment = TextAnchor.MiddleRight;
                GUILayout.Label(c.message, style);
            }
            else
            {

                GUIStyle style = GUI.skin.textArea;
                style.alignment = TextAnchor.MiddleLeft;
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
        clientSocket.Close();
        serverThread.Abort();
    }
}