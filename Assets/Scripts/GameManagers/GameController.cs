using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    NetworkingManager networkManager;

    // Dynamic objects to be updated
    List<GameObject> playerList;
    List<GameObject> enemyList;

    // Start is called before the first frame update
    void Start()
    {
        networkManager = GameObject.FindGameObjectWithTag("NetworkingManager").GetComponent<NetworkingManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (user.player.action == DynamicObject.Action.CREATE)
        //{
        //    // TODO: create objects if needed
        //}
        //else if (user.player.action == DynamicObject.Action.UPDATE)
        //{
        //    // TODO: update objects from our world
        //    //else if (packet.type == PacketType.WORLD_STATE)
        //    //{
        //    //    myPlayerData = SerializationUtility.DeserializeValue<PlayerData>(inputPacket, DataFormat.JSON);
        //    //}
        //}
        //else if (user.player.action == DynamicObject.Action.DESTROY)
        //{
        //    // TODO: destroy objects from our world
        //}
        //else
        //{
        //    Debug.Log("[WARNING] Player Action is NONE.");
        //}
    }
}
