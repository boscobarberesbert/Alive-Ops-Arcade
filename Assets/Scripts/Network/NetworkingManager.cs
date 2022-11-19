using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NetworkingServer;

public class NetworkingManager : MonoBehaviour
{
    public static NetworkingManager Instance;
    INetworking networking;

    public event OnClientAdded onClientAdded;
    bool triggerOnClientAdded = false;

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
            NetworkingServer server = new NetworkingServer();
            server.onClientAdded += this.onClientAdded;
            networking = server;
        }

        networking.Start();        
    }

    private void Update()
    {
        networking.OnUpdate();
    }
}