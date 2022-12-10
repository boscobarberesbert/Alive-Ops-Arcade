using System.Collections.Generic;
using UnityEngine;

public enum PacketType
{
    DEFAULT,
    GAME_START,
    CLIENT_JOIN,
    WORLD_STATE,
    PING
}

public class NetworkUser
{
    // Network information
    public string connectToIP = "";
    public bool isClient = true;
    public string username = "";
    public string networkID = "";

    public NetworkUser()
    {
        connectToIP = "";
        isClient = true;
        username = "";
        networkID = "";
        playerData = new PlayerData();
    }

    public NetworkUser(string connectIP, bool isClient, string username, string networkID)
    {
        this.connectToIP = connectIP;
        this.isClient = isClient;
        this.username = username;
        this.networkID = networkID;
        playerData = new PlayerData();
    }

    // Player that corresponds to the user
    public PlayerData playerData;
}

public class PlayerData
{
    public enum Action
    {
        NONE,
        CREATE,
        UPDATE,
        DESTROY
    }

    public PlayerData()
    {
        action = Action.NONE;
        position = new Vector3(0f, 0f, 0f);
        rotation = new Quaternion(0f, 0f, 0f, 0f);
    }

    // World State
    public Action action = Action.NONE;
    public Vector3 position;
    public Quaternion rotation;
}

public class ServerPacket
{
    public PacketType type = PacketType.DEFAULT;

    // List of players (including server)
    public List<NetworkUser> networkUserList;

    public ServerPacket(List<NetworkUser> networkUserList, PacketType type)
    {
        this.type = type;
        this.networkUserList = networkUserList;
    }
}

public class ClientPacket
{
    public PacketType type = PacketType.DEFAULT;

    public NetworkUser networkUser;

    public ClientPacket(NetworkUser networkUser, PacketType type)
    {
        this.type = type;
        this.networkUser = networkUser;
    }
}