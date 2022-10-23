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

    // Network
    private Socket serverSocket;
    EndPoint endPoint;
    private int channel1Port = 9050;
    private int channel2Port = 9051;

    // Chat & Lobby
    public string serverName;
    string message = "";
    Dictionary<string, string> chat;
    Vector2 scrollPosition;

    // Start is called before the first frame update
    void Start()
    {
        chat = new Dictionary<string, string>();

        InitializeSocket();
    }

    private void InitializeSocket()
    {
        Debug.Log("INITIALIZE THREAD");

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, channel1Port);
        serverSocket.Bind(ipep);

        IPEndPoint sendIpep = new IPEndPoint(IPAddress.Any, channel2Port);
        endPoint = (EndPoint)(sendIpep);

        serverThread = new Thread(ServerSetupUDP);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    private void ServerSetupUDP()
    {
        Debug.Log("Server initialized listening...");

        byte[] data = new byte[1024];
        int recv = serverSocket.ReceiveFrom(data, ref endPoint);
        Debug.Log(Encoding.ASCII.GetString(data, 0, recv));

        data = Encoding.ASCII.GetBytes("Welcome to the " + serverName);
        serverSocket.SendTo(data, data.Length, SocketFlags.None, endPoint);

        while (true)
        {
            data = new byte[1024];
            recv = serverSocket.ReceiveFrom(data, ref endPoint);
            Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
        }
    }

    private void SendChatMessage(string messageToSend)
    {
        byte[] data = Encoding.ASCII.GetBytes(messageToSend);
        serverSocket.SendTo(data, data.Length, SocketFlags.None, endPoint);
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width / 2, Screen.height / 2, 300, 300));
        GUILayout.BeginVertical();
        scrollPosition = GUILayout.BeginScrollView(
            scrollPosition, GUILayout.Width(500), GUILayout.Height(100));

        foreach (var message in chat)
        {
            Debug.Log(chat.Count);
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