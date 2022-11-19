using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkingManager : MonoBehaviour
{
    public static NetworkingManager Instance;
    INetworking networking;
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (PlayerData.isClient)
        {
            networking = new NetworkClient();
        }
        else
        {
            networking = new NetworkingServer();
        }

        networking.Start();
    }
}
