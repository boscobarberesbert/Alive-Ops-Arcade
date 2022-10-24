using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TCPServer : MonoBehaviour
{
    Thread connectionThread;
    Thread serverThread;
    private object myLock;
    // Network
    private Socket serverSocket;

    ArrayList clientList;

    ArrayList copyClientList;

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
        myLock = new object();
        clientList = new ArrayList();

        InitializeSocket();
    }

    private void InitializeSocket()
    {
        Debug.Log("INITIALIZE THREAD");

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, channel1Port);
        serverSocket.Bind(ipep);

        connectionThread = new Thread(ServerConnectionListener);
        connectionThread.IsBackground = true;
        connectionThread.Start();

        serverThread = new Thread(ServerRoomBroadcast);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    private void ServerConnectionListener()
    {
        serverSocket.Listen(10);
        while (clientList.Count < 11)
        {
            Socket newClient = serverSocket.Accept();
            IPEndPoint clientIpep = (IPEndPoint)newClient.RemoteEndPoint;

            Debug.Log("Connected with " + clientIpep.Address.ToString() + " at port: " + clientIpep.Port);

            byte[] data = new byte[1024];
            int recv = newClient.Receive(data);
            Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
            chat.Add(new ChatMessage("client", Encoding.ASCII.GetString(data, 0, recv)));

            data = Encoding.ASCII.GetBytes("Welcome to the " + serverName);
            newClient.Send(data, data.Length, SocketFlags.None);
            lock (myLock)
            {
                clientList.Add(newClient);
            }
        }

    }
    private void ServerRoomBroadcast()
    {
        while (true)
        {
            lock (myLock)
            {
                copyClientList = new ArrayList(clientList);
            }
            if (copyClientList.Count == 0)
            {
                continue;
            }
            Socket.Select(copyClientList, null, null, 1000);
            foreach (Socket client in copyClientList)
            {
                byte[] data = new byte[1024];
                int recv = client.Receive(data);

                if (recv == 0)
                {
                    IPEndPoint iep = (IPEndPoint)client.RemoteEndPoint;
                    Debug.Log("Client {0} disconnected." + iep.ToString());
                    client.Close();

                    int clientCount;

                    lock (myLock)
                    {
                        clientList.Remove(client);
                        clientCount = clientList.Count;
                    }

                    if (clientCount == 0)
                    {
                        Debug.Log("Last client disconnected, bye");
                        return;
                    }
                }
                else
                {
                    Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
                    chat.Add(new ChatMessage("client", Encoding.ASCII.GetString(data, 0, recv)));
                    foreach(Socket client in copyClientList)
                    {
                        client.Send(data, recv, SocketFlags.None);
                    }
                }
            }
        }
    }

    private void SendChatMessage(string messageToSend)
    {
        lock (myLock)
        {
            copyClientList = new ArrayList(clientList);
        }

        Socket.Select(copyClientList, null, null, 1000);
        foreach (Socket client in copyClientList)
        {
            byte[] data = new byte[1024];
            data = Encoding.ASCII.GetBytes(messageToSend);
            client.Send(data, data.Length, SocketFlags.None);
            chat.Add(new ChatMessage("client", messageToSend));

        }
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

    private void OnDisable()
    {
        Debug.Log("Destroying Scene");

        serverSocket.Close();
        serverThread.Abort();
    }
}