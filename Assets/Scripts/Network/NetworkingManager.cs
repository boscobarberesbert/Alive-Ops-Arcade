using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkingManager : MonoBehaviour
{
    public static NetworkingManager Instance;
    public INetworking networking;

    // Reference to our player gameobject
    [NonSerialized] public GameObject myPlayerGO;
    Transform initialSpawnPoint;
    [NonSerialized] public Vector3 initialSpawnPosition;
    // Map to relate networkID to its gameobject
    public Dictionary<string, GameObject> playerGOMap = new Dictionary<string, GameObject>();

    // Condition to know if the LoadScene() method has been called
    bool isSceneLoading = false;

    [SerializeField] GameObject playerPrefab;
    public float updateTime = 0.1f;

    private void Awake()
    {
        initialSpawnPoint = GameObject.FindGameObjectWithTag("Spawn Point").transform;
        initialSpawnPosition = initialSpawnPoint.position;
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Creating network (client or server) Interface Networking
        networking = MainMenuInfo.isClient ? new NetworkingClient() : new NetworkingServer();

        // Initializing user and player data
        networking.myUserData = new User(System.Guid.NewGuid().ToString(), MainMenuInfo.username, MainMenuInfo.connectToIp);
    }

    void Start()
    {
        // Starting networking
        networking.Start();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Update()
    {
        lock (networking.loadSceneLock)
        {
            if (networking.triggerLoadScene)
            {
                isSceneLoading = true;

                playerGOMap.Clear();

                SceneManager.LoadSceneAsync("Game");

                networking.triggerLoadScene = false;
            }
        }

        if (!isSceneLoading)
        {
            lock (networking.playerMapLock)
            {

                foreach (var packet in networking.packetQueue.ToList())
                {
                    switch (packet.type)
                    {
                        case PacketType.HELLO:
                            {
                                Spawn((packet as HelloPacket).clientData.networkID);
                                break;
                            }
                        case PacketType.WELCOME:
                            {
                                foreach (var player in (packet as ServerPacket).playerMap)
                                {
                                    HandlePlayerObject(player.Key, player.Value);
                                }
                                break;
                            }
                        case PacketType.GAME_START:
                            {
                                lock (networking.loadSceneLock)
                                {
                                    networking.triggerLoadScene = true;
                                }
                                break;
                            }
                        case PacketType.WORLD_STATE:
                            {
                                if (networking is NetworkingServer)
                                {
                                    HandlePlayerObject((packet as ClientPacket).networkID, (packet as ClientPacket).playerObject);
                                }
                                else
                                {
                                    foreach (var player in (packet as ServerPacket).playerMap)
                                    {
                                        HandlePlayerObject(player.Key, player.Value);
                                    }
                                }

                                break;
                            }
                        case PacketType.PING:
                            {
                                break;
                            }
                        case PacketType.DEFAULT:
                            {
                                break;
                            }
                    }
                    networking.packetQueue.Dequeue();
                }
            }

            networking.OnUpdate();
        }
    }

    public void HandlePlayerObject(string key, PlayerObject player)
    {
        // TODO
        switch (player.action)
        {
            case PlayerObject.Action.CREATE:
                {
                    Spawn(key);
                    break;
                }
            case PlayerObject.Action.UPDATE:
                {
                    if (playerGOMap.ContainsKey(key) && key != networking.myUserData.networkID)
                    {
                        // TODO: Perform interpolation
                        playerGOMap[key].transform.position = player.position;
                        playerGOMap[key].transform.rotation = player.rotation;
                        playerGOMap[key].GetComponent<PlayerController>().SetAnimatorRunning(player.isRunning);
                    }
                    break;
                }
            case PlayerObject.Action.DESTROY:
                {

                    break;
                }
            case PlayerObject.Action.NONE:
                {

                    break;
                }
        }
    }

    public void Spawn(string networkID)
    {
        Vector3 spawnPos = new Vector3(NetworkingManager.Instance.initialSpawnPosition.x + playerGOMap.Count * 3, NetworkingManager.Instance.initialSpawnPosition.y, NetworkingManager.Instance.initialSpawnPosition.z);

        // Instantiate the game object at the required position
        GameObject playerGO = Instantiate(playerPrefab, spawnPos, new Quaternion(0, 0, 0, 0));

        // Set playerGO variables
        playerGO.GetComponent<NetworkObject>().networkID = networkID;
        playerGO.name = networkID;

        // If the object created is mine, add it to myPlayerGO variable
        if (networkID == networking.myUserData.networkID)
        {
            myPlayerGO = playerGO;
        }
        else
        {
            // Since the player is not ours we don't want to control it with our inputs
            playerGO.GetComponent<PlayerController>().enabled = false;
            playerGO.GetComponent<CharacterController>().enabled = false;
            playerGO.GetComponent<MouseAim>().enabled = false;

            // TODO: Instance Players without Player Tag
            playerGO.transform.GetChild(0).tag = "Untagged";
        }

        // Now we add it to the list of player GO if it is not already there (change scene case)
        if (!playerGOMap.ContainsKey(networkID))
        {
            playerGOMap.Add(networkID, playerGO);
        }

        // Finally we broadcast the corresponding packet to the clients
        if (networking is NetworkingServer)
            (networking as NetworkingServer).NotifySpawn(networkID);

    }

  

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        initialSpawnPoint = GameObject.FindGameObjectWithTag("Spawn Point").transform;
        initialSpawnPosition = initialSpawnPoint.position;
        networking.OnSceneLoaded();
        isSceneLoading = false;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        networking.OnDisconnect();
    }
}