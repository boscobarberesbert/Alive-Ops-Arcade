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

public class UserData
{
    public int clientID = -1;
    public string username = "";
    public string connectToIP = "";

    public UserData(string username, string connectToIP)
    {
        clientID = -1;
        this.username = username;
        this.connectToIP = connectToIP;
    }
}

public class DynamicObject
{
    public enum Action
    {
        NONE,
        CREATE,
        UPDATE,
        DESTROY
    }

    public string networkID = "";

    // World State
    public Action action = Action.NONE;
    public Vector3 position;
    public Quaternion rotation;

    public DynamicObject()
    {
        networkID = "";
        action = Action.NONE;
        position = new Vector3(0f, 0f, 0f);
        rotation = new Quaternion(0f, 0f, 0f, 0f);
    }

    public DynamicObject(string networkID)
    {
        this.networkID = networkID;
        action = Action.NONE;
        position = new Vector3(0f, 0f, 0f);
        rotation = new Quaternion(0f, 0f, 0f, 0f);
    }
}

public class Packet
{
    public PacketType type = PacketType.DEFAULT;
}

public class ServerPacket : Packet
{
    // List of players (including server)
    public List<DynamicObject> playerList;
    
    // List of enemies
    // TODO: initialize enemy list
    public List<DynamicObject> enemyList;

    public ServerPacket(PacketType type, List<DynamicObject> playerList)
    {
        this.type = type;
        this.playerList = playerList;
    }
}

public class ClientPacket : Packet
{
    public DynamicObject player;

    public ClientPacket(PacketType type, DynamicObject player)
    {
        this.type = type;
        this.player = player;
    }
}

public class HelloPacket : Packet
{
    public UserData clientData;

    public HelloPacket(UserData clientData)
    {
        type = PacketType.HELLO;
        this.clientData = clientData;
    }
}

public class WelcomePacket : Packet
{
    public int clientIDAssigned = -1;
    public string networkIDAssigned = "";

    // List of players (including server)
    public List<DynamicObject> playerList;

    // List of enemies
    // TODO: initialize enemy list
    public List<DynamicObject> enemyList;

    public WelcomePacket(int clientIDAssigned, string networkIDAssigned)
    {
        type = PacketType.WELCOME;
        this.clientIDAssigned = clientIDAssigned;
        this.networkIDAssigned = networkIDAssigned;
    }
}