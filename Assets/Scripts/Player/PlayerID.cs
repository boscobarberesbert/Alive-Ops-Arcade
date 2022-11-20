using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AliveOpsArcade.OdinSerializer;

public class PlayerID : MonoBehaviour
{
    public int playerId = -1;
    
    private PlayerState currentState;

    public void Start()
    {
        currentState = new PlayerState();
        currentState.playerID = playerId;
    }

    public void Update()
    {
        currentState.position = transform.position;
        byte[] bytes = SerializationUtility.SerializeValue(currentState, DataFormat.JSON);

        //NetworkingManager.Instance.networking.SendPacket(bytes, NetworkingManager.Instance.networking.)
    }
}
