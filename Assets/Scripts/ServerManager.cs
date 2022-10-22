using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ServerManager : MonoBehaviour
{
    public enum Protocol
    {
        UDP,
        TDP
    }

    public Protocol m_Protocol = Protocol.UDP;

    Thread serverThread;

    private Socket socketUDP;
    //private Socket socketTDP;

    EndPoint endPoint;

    private int receivePort = 9050;
    private int sendPort = 9051;

    public string serverName;
    string message = "";
    List<string> chatList;

    // Start is called before the first frame update
    void Start()
    {
        InitializeSocket();
    }

    private void OnDestroy()
    {
        Debug.Log("Destroying Scene");

        socketUDP.Close();
        serverThread.Abort();
        //clientThread.Abort();
    }

    void OnGUI()
    {
        Rect rectObj = new Rect(40, 380, 200, 400);
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        GUI.Box(rectObj, "Chat", style);

        foreach (var chat in chatList)
        {
            GUI.Box(rectObj, chat, style);
        }

        message = GUI.TextField(new Rect(40, 420, 140, 20), message);
        if (GUI.Button(new Rect(190, 420, 40, 20), "send"))
        {
            SendChatMessage(message + "\n");
            message = "";
        }
    }

    private void InitializeSocket()
    {
        Debug.Log("INITIALIZE THREAD");

        socketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, receivePort);
        socketUDP.Bind(ipep);

        IPEndPoint sendIpep = new IPEndPoint(IPAddress.Any, sendPort);
        endPoint = (EndPoint)(sendIpep);

        serverThread = new Thread(ServerSetupUDP);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    public void ServerSetupUDP()
    {
        Debug.Log("Server initialized listening....");

        byte[] data = new byte[1024];
        int recv = socketUDP.ReceiveFrom(data, ref endPoint);
        Debug.Log(Encoding.ASCII.GetString(data, 0, recv));

        data = Encoding.ASCII.GetBytes("Welcome to the " + serverName);
        socketUDP.SendTo(data, data.Length, SocketFlags.None, endPoint);

        while (true)
        {
            data = new byte[1024];
            recv = socketUDP.ReceiveFrom(data, ref endPoint);
            Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
            chatList.Add(Encoding.ASCII.GetString(data, 0, recv));
        }
    }

    private void SendChatMessage(string messageToSend)
    {
        byte[] data = Encoding.ASCII.GetBytes(messageToSend);
        socketUDP.SendTo(data, data.Length, SocketFlags.None, endPoint);
    }
}