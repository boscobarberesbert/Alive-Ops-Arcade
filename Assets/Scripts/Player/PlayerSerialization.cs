using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// The player data class to be sent through the network
// It is only storing the position for now, but it is needed for scalability
class PlayerState
{
    public int playerID;
    public Vector3 position;
}

public class PlayerSerialization : MonoBehaviour
{
    GameObject player;
    MemoryStream stream;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        // We fill the updated data about the player and fill our class
        PlayerState playerState = new PlayerState
        {
            position = player.transform.position
        };

        Debug.Log(playerState.position);
        //SerializeJson(playerState);
    }

    void SerializeJson(PlayerState playerState)
    {
        string json = JsonUtility.ToJson(playerState);
        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(json);
    }

    void DeserializeJson()
    {
        PlayerState playerState = new PlayerState();

        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        string json = reader.ReadString();
        Debug.Log(json);
        playerState = JsonUtility.FromJson<PlayerState>(json);
        Debug.Log(playerState.position);
    }
}
