using System.Collections.Generic;
using UnityEngine;

public enum PacketType
{
    DEFAULT,
    HELLO,
    WELCOME,
    PING,
    GAME_START,
    WORLD_STATE
}

public class User
{
    public string networkID = "";
    public string username = "";
    public string connectToIP = "";

    public User(string networkID, string username, string connectToIP)
    {
        this.networkID = networkID;
        this.username = username;
        this.connectToIP = connectToIP;
    }
}

public class PlayerObject
{
    public enum Action
    {
        NONE,
        CREATE,
        UPDATE,
        DESTROY
    }

    // World State
    public Action action = Action.NONE;
    public Vector3 position;
    public Quaternion rotation;

    public PlayerObject()
    {
        action = Action.NONE;
        position = new Vector3(0f, 0f, 0f);
        rotation = new Quaternion(0f, 0f, 0f, 0f);
    }

    public PlayerObject(Action action, Vector3 position, Quaternion rotation)
    {
        this.action = action;
        this.position = position;
        this.rotation = rotation;
    }
}

public class Packet
{
    public PacketType type = PacketType.DEFAULT;
}

public class ServerPacket : Packet
{
    // List of players (including server)
    public Dictionary<string, PlayerObject> playerMap;
    
    // TODO: List of enemies

    public ServerPacket(PacketType type, Dictionary<string, PlayerObject> playerMap)
    {
        this.type = type;
        this.playerMap = playerMap;
    }
}

public class ClientPacket : Packet
{
    public PlayerObject playerObject;
    public string networkID = "";

    public ClientPacket(PacketType type, string networkID, PlayerObject playerObject)
    {
        this.type = type;
        this.networkID = networkID;
        this.playerObject = playerObject;
    }
}

public class HelloPacket : Packet
{
    public User clientData;

    public HelloPacket(User clientData)
    {
        type = PacketType.HELLO;
        this.clientData = clientData;
    }
}