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
    private Socket serverSocket;
    EndPoint endPoint;
    private int channel1Port = 9050;
    private int channel2Port = 9051;
    // Chat & Lobby
    public string clientName;
    public string serverIP;
    string message = "";
    Dictionary<string, string> chat;
    Vector2 scrollPosition;
    IPEndPoint ipep;

    // Start is called before the first frame update
    void Start()
    {
        chat = new Dictionary<string, string>();

        InitializeSocket();
    }

    private void InitializeSocket()
    {
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        clientThread = new Thread(ClientSetupUDP);
        ipep = new IPEndPoint(IPAddress.Parse(serverIP), channel1Port) ;
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private void ClientSetupUDP()
    {
        byte[] data = new byte[1024];
      
        string welcome = "Hello, are you there?";
        data = Encoding.ASCII.GetBytes(welcome);
        serverSocket.SendTo(data, data.Length, SocketFlags.None, ipep);

        IPEndPoint senderIpep = new IPEndPoint(IPAddress.Any, 0);
        EndPoint endpoint = (EndPoint)(senderIpep);
        int recv;
        data = new byte[1024];

        recv = serverSocket.ReceiveFrom(data, ref endpoint);
        Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
       
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
           
                //SendChatMessageUDP(message + "\n");
           
            message = "";
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }



    private void OnDestroy()
    {
        Debug.Log("Destroying Scene");

        serverSocket.Close();
        clientThread.Abort();
    }
}
