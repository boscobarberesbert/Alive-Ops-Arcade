using System.Collections.Generic;
using UnityEngine;

public enum PacketType
{
    DEFAULT,
    GAME_START,
    HELLO,
    WELCOME,
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
        player = new DynamicObject();
    }

    public NetworkUser(string connectIP, bool isClient, string username, string networkID)
    {
        this.connectToIP = connectIP;
        this.isClient = isClient;
        this.username = username;
        this.networkID = networkID;
        player = new DynamicObject();
    }

    // Player that corresponds to the user
    public DynamicObject player;
}

public class DynamicObject
{
    public enum Action
    {
        NONE,
        CREATE, // TODO: Maybe we don't need it?
        UPDATE,
        DESTROY
    }

    public DynamicObject()
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

    // List of enemies
    // TODO: initialize enemy list
    public List<DynamicObject> enemyList;

    public ServerPacket(PacketType type, List<NetworkUser> networkUserList)
    {
        this.type = type;
        this.networkUserList = networkUserList;
    }
}

public class ClientPacket
{
    public PacketType type = PacketType.DEFAULT;

    public NetworkUser networkUser;

    public ClientPacket(PacketType type, NetworkUser networkUser)
    {
        this.type = type;
        this.networkUser = networkUser;
    }
}