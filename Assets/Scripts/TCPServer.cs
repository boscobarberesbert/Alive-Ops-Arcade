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
    private object clientListLock;
    private object chatLock;

    // Network
    private Socket serverSocket;

    ArrayList clientList;
    private int port = 9050;

    // Lobby & Chat
    public string serverName;

    string message = "";
    List<ChatMessage> chat;
    Vector2 scrollPosition;

    // Start is called before the first frame update
    void Start()
    {
        chat = new List<ChatMessage>();
        chatLock = new object();

        clientListLock = new object();
        clientList = new ArrayList();

        InitializeSocket();
    }

    private void InitializeSocket()
    {
        Debug.Log("INITIALIZE THREAD");

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
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
            lock (chatLock)
            {
                chat.Add(new ChatMessage("client", Encoding.ASCII.GetString(data, 0, recv), serverName));
            }

            data = Encoding.ASCII.GetBytes("Welcome to the " + serverName);
            newClient.Send(data, data.Length, SocketFlags.None);
            lock (clientListLock)
            {
                clientList.Add(newClient);
            }
        }
    }
   
    private void ServerRoomBroadcast()
    {
        ArrayList readableClients = new ArrayList();
        ArrayList writableClients = new ArrayList();
        while (true)
        {
            lock(clientListLock)
            {
                readableClients = new ArrayList(clientList);
                writableClients = new ArrayList(clientList);
            }
            
            if (readableClients.Count == 0)
            {
                continue;
            }
            Socket.Select(readableClients, null, null, 1000);
            foreach (Socket client in readableClients)
            {
                byte[] data = new byte[1024];
                int recv = client.Receive(data);

                if (recv == 0)
                {
                    IPEndPoint iep = (IPEndPoint)client.RemoteEndPoint;
                    Debug.Log("Client " + iep.ToString() + " disconnected.");
                    client.Close();

                    int clientCount;

                    lock (clientListLock)
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
                    lock (chatLock)
                    {
                        chat.Add(new ChatMessage("client", Encoding.ASCII.GetString(data, 0, recv), serverName));
                    }

                    if (writableClients.Count == 0)
                    {
                        Debug.Log("WRITABLE CLIENTS COUNT IS 0");
                        continue;
                    }

                    // Broadcast the message received to all clients available
                    Socket.Select(null, writableClients, null, 1000);
                    foreach (Socket clientToBroadcast in writableClients)
                    {
                        if (!(clientToBroadcast == client))
                            clientToBroadcast.Send(data, recv, SocketFlags.None);
                    }
                }
            }
        }
    }

    private void SendChatMessage(string messageToSend)
    {
        ArrayList copyClientList = new ArrayList(clientList);
        lock (clientListLock)
        {
            copyClientList = new ArrayList(clientList);
        }

        Socket.Select(null, copyClientList, null, 1000);
        foreach (Socket client in copyClientList)
        {
            byte[] data = new byte[1024];
            data = Encoding.ASCII.GetBytes(messageToSend);
            client.Send(data, data.Length, SocketFlags.None);
        }
        lock (chat)
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
            foreach (var chatEntry in chat)
            {
                if (chatEntry.senderType.Contains("server"))
                    style.alignment = TextAnchor.MiddleRight;
                else
                    style.alignment = TextAnchor.MiddleLeft;

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
        serverThread.Abort();
    }
}