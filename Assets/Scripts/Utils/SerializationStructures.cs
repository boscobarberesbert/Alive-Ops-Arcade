using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Packet
{
    public enum PacketType
    {
        DEFAULT,
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

public class PlayerData : Packet
{

}