using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Packet
{
    public enum PacketType
    {
        DEFAULT,
        LOBBY_STATE,
        CLIENT_NEW,
        CLIENT_UPDATE
    }
    public PacketType type = PacketType.DEFAULT;
}

public class UserData : Packet
{
    public string connectToIP = "";
    public bool isClient = true;
    public string username = "";

    public UserData()
    {
        this.type = PacketType.CLIENT_NEW;
        connectToIP = "";
        isClient = true;
        username = "";
    }

    public UserData(string connectIP, bool isClient, string username)
    {
        this.type = PacketType.CLIENT_NEW;
        this.connectToIP = connectIP;
        this.isClient = isClient;
        this.username = username;
    }

    public byte[] SerializeJson()
    {
        string json = JsonUtility.ToJson(this);

        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(json);

        return stream.ToArray();
    }
}

public class LobbyState : Packet
{
    // Dictionary to link a user with a game object (including server)
    public Dictionary<UserData, int> players;

    public LobbyState()
    {
        this.type = PacketType.LOBBY_STATE;
        players = new Dictionary<UserData, int>();
    }
}

public class PlayerState : Packet
{
    public int playerID;
    public Vector3 position;

    public PlayerState()
    {
        this.type = PacketType.CLIENT_UPDATE;
        position = new Vector3(0f, 0f, 0f);
    }
}