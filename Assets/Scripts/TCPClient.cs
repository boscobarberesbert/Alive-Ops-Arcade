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
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        ipep = new IPEndPoint(IPAddress.Parse(serverIP), channel1Port);

        IPEndPoint sendIpep = new IPEndPoint(IPAddress.Any, channel2Port);
        endPoint = (EndPoint)sendIpep;

        clientThread = new Thread(ClientSetupTCP);
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private void ClientSetupTCP()
    {
        try
        {
            clientSocket.Connect(ipep);
        }
        catch (SocketException e)
        {
            Debug.Log("Unable to connect to server.");
            Debug.Log(e.ToString());
        }

        byte[] data = new byte[1024];
        data = Encoding.ASCII.GetBytes(clientName + " joined the room.");
        clientSocket.Send(data, data.Length, SocketFlags.None);

        while (true)
        {
            data = new byte[1024];
            int recv = clientSocket.Receive(data);
            Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
            chat.Add(new ChatMessage("server", Encoding.ASCII.GetString(data, 0, recv)));

        }
        clientSocket.Close();

    }

    private void SendChatMessage(string messageToSend)
    {
        byte[] data = Encoding.ASCII.GetBytes(messageToSend);
        clientSocket.Send(data, data.Length, SocketFlags.None);
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
                style.alignment = TextAnchor.MiddleLeft;
                GUILayout.Label(c.message, style);
            }
            else
            {

                GUIStyle style = GUI.skin.textArea;
                style.alignment = TextAnchor.MiddleRight;
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

        clientSocket.Close();
        clientThread.Abort();
    }
}