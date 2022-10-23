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
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverThread = new Thread(ServerSetupUDP);
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, receivePort);
        serverSocket.Bind(ipep);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    private void ServerSetupUDP()
    {
        IPEndPoint senderIpep = new IPEndPoint(IPAddress.Any, 0);
        EndPoint endpoint = (EndPoint)(senderIpep);
        int recv;
        byte[] data = new byte[1024];

        recv = serverSocket.ReceiveFrom(data, ref endpoint);
        Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
        string welcome = "Welcome to my test server";
        data = Encoding.ASCII.GetBytes(welcome);
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



    private void OnDestroy()
    {
        Debug.Log("Destroying Scene");

        serverSocket.Close();
        serverThread.Abort();
    }
}
