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
        }
    }

    private void SendChatMessage(string messageToSend)
    {
        byte[] data = Encoding.ASCII.GetBytes(messageToSend);
        clientSocket.Send(data, data.Length, SocketFlags.None);
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

        clientSocket.Close();
        clientThread.Abort();
    }
}