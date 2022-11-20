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
        currentState.rotation = transform.rotation;
        byte[] bytes = SerializationUtility.SerializeValue(currentState, DataFormat.JSON);
        if(NetworkingManager.Instance.networking is NetworkClient)
        {
           (NetworkingManager.Instance.networking as NetworkClient).SendPacketToServer(bytes);
        }
        else
        {
            (NetworkingManager.Instance.networking as NetworkingServer).BroadcastPacketFromServer(bytes);
        }
    }
}
