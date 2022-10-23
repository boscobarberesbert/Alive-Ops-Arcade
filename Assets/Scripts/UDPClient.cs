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

    // Network
    private Socket clientSocket;

    IPEndPoint ipep;
    EndPoint endPoint;

    private int channel1Port = 9050;
    private int channel2Port = 9051;

    // Chat & Lobby
    public string serverIP;
    public string clientName;
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
        data = Encoding.ASCII.GetBytes(clientName + " joined the room.");
        clientSocket.SendTo(data, data.Length, SocketFlags.None, ipep);
        chat.Add(new ChatMessage("client", clientName + " joined the room."));

        while (true)
        {
            data = new byte[1024];
            int recv = clientSocket.ReceiveFrom(data, ref endPoint);
            Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
            chat.Add(new ChatMessage("server", Encoding.ASCII.GetString(data, 0, recv)));

        }
    }

    private void SendChatMessage(string messageToSend)
    {
        byte[] data = Encoding.ASCII.GetBytes(messageToSend);
        clientSocket.SendTo(data, data.Length, SocketFlags.None, ipep);
        chat.Add(new ChatMessage("client",messageToSend));

    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width / 2-225, Screen.height / 2-111, 450, 222));
        GUILayout.BeginVertical();
        scrollPosition = GUILayout.BeginScrollView(
            scrollPosition, GUI.skin.box, GUILayout.Width(450), GUILayout.Height(100));

        foreach (var c in chat)
        {
            if (c.sender == "server")
            {
               
                GUILayout.Label(c.message,GUI.skin.textArea);
            }
            else
            {
                GUIStyle style = GUI.skin.textArea;
                style.alignment = TextAnchor.MiddleRight;
                GUILayout.Label(c.message,style);
            }
        }
        GUI.contentColor = Color.black;

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

        clientSocket.Close();
        clientThread.Abort();
    }
}