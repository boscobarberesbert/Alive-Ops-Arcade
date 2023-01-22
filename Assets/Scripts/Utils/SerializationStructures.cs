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

//data structure that stores all the client networking information passed throught the network
public class User
{
    public string networkID;
    public string username;
    public string connectToIP;

    public User()
    {
        networkID = "";
        username = "";
        connectToIP = "";
    }

    public User(string networkID, string username, string connectToIP)
    {
        this.networkID = networkID;
        this.username = username;
        this.connectToIP = connectToIP;
    }
}

//data structure that stores all the player game object information passed through the network
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
    public Action action;
    public Vector3 position;
    public bool isRunning;
    public Quaternion rotation;

    public PlayerObject()
    {
        action = Action.NONE;
        position = new Vector3(0f, 0f, 0f);
        rotation = new Quaternion(0f, 0f, 0f, 0f);
        isRunning = false;
    }

    public PlayerObject(Action action, Vector3 position, Quaternion rotation, bool isRunning)
    {
        this.action = action;
        this.position = position;
        this.rotation = rotation;
        this.isRunning = isRunning;
    }
}

//data structure that stores all the player game object information passed through the network
public class GenericObject
{
    public enum Action
    {
        NONE,
        CREATE,
        UPDATE,
        DESTROY
    }

    // World State
    public string networkID;
    public Action action;
    public Vector3 position;
    public Quaternion rotation;

    public GenericObject()
    {
        networkID = "";
        action = Action.NONE;
        position = new Vector3(0f, 0f, 0f);
        rotation = new Quaternion(0f, 0f, 0f, 0f);
    }

    public GenericObject(string networkID, Action action, Vector3 position, Quaternion rotation)
    {
        this.networkID = networkID;
        this.action = action;
        this.position = position;
        this.rotation = rotation;
    }
}


//Definition of a packet sent through the internet
public class Packet
{
    public PacketType type;

    public Packet()
    {
        type = PacketType.DEFAULT;
    }
    public Packet(PacketType type)
    {
        this.type = type;
    }
}

//Type of packet sent by the server
public class ServerPacket : Packet
{
    // List of players (including server)
    public Dictionary<string, PlayerObject> playerMap;

    //  List of enemies and bullets
    public Dictionary<string, GenericObject> enemiesMap;
    public Dictionary<string, GenericObject> bulletsMap;
    public ServerPacket()
    {
        playerMap = new Dictionary<string, PlayerObject>();
        enemiesMap = new Dictionary<string, GenericObject>();
        bulletsMap = new Dictionary<string, GenericObject>();
    }

    public ServerPacket(PacketType type, Dictionary<string, PlayerObject> playerMap,Dictionary<string,GenericObject> enemiesMap, Dictionary<string, GenericObject> bulletsMap)
    {
        this.type = type;
        this.playerMap = playerMap;
        this.enemiesMap = enemiesMap;
        this.bulletsMap = bulletsMap;
    }
}
//type of packet sent by the client
public class ClientPacket : Packet
{
    public PlayerObject playerObject;
    public string networkID;

    public ClientPacket()
    {
        playerObject = new PlayerObject();
        networkID = "";
    }

    public ClientPacket(PacketType type, string networkID, PlayerObject playerObject)
    {
        this.type = type;
        this.networkID = networkID;
        this.playerObject = playerObject;
    }
}

//Hello packet that sends the user information when a client is added
public class HelloPacket : Packet
{
    public User clientData;

    public HelloPacket()
    {
        clientData = new User();
    }

    public HelloPacket(User clientData)
    {
        type = PacketType.HELLO;
        this.clientData = clientData;
    }
}