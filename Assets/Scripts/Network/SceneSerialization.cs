using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataToSend
{
    public enum GameObjectType
    {
        PLAYER,
        ENEMIES
    }

    public GameObjectType type;
    public MemoryStream stream;
}

public class PlayerSerialize : DataToSend
{
    public PlayerSerialize()
    {
        type = GameObjectType.PLAYER;
    }

    void SerializeJson(PlayerState playerState)
    {
        string json = JsonUtility.ToJson(playerState);
        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(json);
    }

    PlayerState DeserializeJson()
    {
        PlayerState playerState = new PlayerState();

        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        string json = reader.ReadString();
        Debug.Log(json);
        playerState = JsonUtility.FromJson<PlayerState>(json);
        Debug.Log(playerState.position);

        return playerState;
    }
}

public class EnemySerialize : DataToSend
{
    public EnemySerialize()
    {
        type = GameObjectType.ENEMIES;
    }

    void SerializeJson(EnemiesState enemiesState)
    {
        string json = JsonUtility.ToJson(enemiesState);
        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(json);
    }

    EnemiesState DeserializeJson()
    {
        var enemiesState = new EnemiesState();
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        string json = reader.ReadString();
        Debug.Log(json);
        enemiesState = JsonUtility.FromJson<EnemiesState>(json);
        Debug.Log(enemiesState.enemyCount.ToString());

        return enemiesState;
    }
}
