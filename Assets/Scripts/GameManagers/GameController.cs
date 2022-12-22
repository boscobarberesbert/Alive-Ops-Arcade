using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    NetworkingManager networkManager;

    // Lerp
    float timeElapsed = 0;
    float lerpDuration = 0.05f;
    float startValue = 0; // It should be the start position of the vector we want to interpolate.
    float endValue = 10; // It should be the end position of the vector we want to interpolate.
    float valueToLerp; // It should be the current value of the vector we want to interpolate.

    // Start is called before the first frame update
    void Start()
    {
        networkManager = GameObject.FindGameObjectWithTag("NetworkingManager").GetComponent<NetworkingManager>();

        StartCoroutine(InterpolatePositions());
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

    IEnumerator InterpolatePositions()
    {
        for (int i = 0; i < networkManager.playerMap.Count; ++i)
        {
            if (timeElapsed < lerpDuration)
            {
                valueToLerp = Mathf.Lerp(startValue, endValue, timeElapsed / lerpDuration);
                timeElapsed += Time.deltaTime;
            }
            else
            {
                valueToLerp = endValue;
                timeElapsed = 0;
            }
        }

        yield return null;
    }
}
