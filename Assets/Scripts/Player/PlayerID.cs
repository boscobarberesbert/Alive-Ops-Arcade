using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AliveOpsArcade.OdinSerializer;

public class PlayerID : MonoBehaviour
{
    public string networkID = "";
    
    private PlayerData currentState;

    public void Start()
    {
        //currentState = new PlayerData();
        //currentState.playerID = playerId;
    }

    public void Update()
    {
        //currentState.position = transform.position;
        //currentState.rotation = transform.rotation;
        //byte[] bytes = SerializationUtility.SerializeValue(currentState, DataFormat.JSON);
        //if(NetworkingManager.Instance.networking is NetworkingClient)
        //{
        //   (NetworkingManager.Instance.networking as NetworkingClient).SendPacketToServer(bytes);
        //}
        //else
        //{
        //    (NetworkingManager.Instance.networking as NetworkingServer).BroadcastPacket(bytes,false);
        //}
    }
}
