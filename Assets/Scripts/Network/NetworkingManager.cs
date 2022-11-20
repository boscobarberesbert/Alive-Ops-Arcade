using System.Collections.Generic;
using System.IO;
using UnityEngine;



public class NetworkingManager : MonoBehaviour
{
    public static NetworkingManager Instance;
    public INetworking networking;

    public delegate void OnClientAdded();
    public event OnClientAdded onClientAdded;

    [SerializeField] GameObject playerPrefab;
    [SerializeField] Vector3 startSpawnPosition;
    List<GameObject> players = new List<GameObject>();


    private void Awake()
    {
        onClientAdded += Spawn;
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        //Creating network client | server
        networking = MainMenuInfo.isClient ? new NetworkClient() : new NetworkingServer();
        //Initializing user data
        networking.myUserData.username = MainMenuInfo.username;
        networking.myUserData.isClient = MainMenuInfo.isClient;
        networking.myUserData.connectToIP = MainMenuInfo.connectToIp;
        //starting networking
        networking.Start();
    }

    private void Update()
    {
        networking.OnUpdate();
        if (networking.triggerClientAdded)
        {
            if (onClientAdded != null)
            {
                onClientAdded();
            }
            networking.triggerClientAdded = false;
        }
    }
    private void OnDisable()
    {
        networking.OnDisconnect();
    }


    public void Spawn()
    {
        foreach (KeyValuePair<UserData, int> player in networking.lobbyState.players)
        {
            if (!GameObject.Find(player.Key.username))
            {
                Vector3 spawnPosition = new Vector3(startSpawnPosition.x + players.Count * 3, 0, 0);
                GameObject playerGO = Instantiate(playerPrefab, spawnPosition, new Quaternion(0, 0, 0, 0));
                playerGO.name = player.Key.username;
                playerGO.GetComponent<PlayerID>().playerId = player.Value;
                players.Add(playerGO);
            }

        }

    }


}