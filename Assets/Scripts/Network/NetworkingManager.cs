using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Networking.Types;
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
    // Map to relate networkID to its enemyObject
    public Dictionary<string, GameObject> enemyGOMap = new Dictionary<string, GameObject>();
    // Map to relate networkID to its bulletsObject
    public Dictionary<string, GameObject> bulletGOMap = new Dictionary<string, GameObject>();

    // Condition to know if the LoadScene() method has been called
    bool isSceneLoading = false;

    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject bulletPrefab;
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
                enemyGOMap.Clear();
                bulletGOMap.Clear();
                if (networking is NetworkingServer)
                {
                    SceneManager.LoadSceneAsync("Game");

                }
                else
                {
                    SceneManager.LoadSceneAsync("GameClient");

                }

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
                                foreach (var enemy in (packet as ServerPacket).enemiesMap)
                                {
                                    HandleEnemyObject(enemy.Key, enemy.Value);
                                }
                                foreach (var bullet in (packet as ServerPacket).bulletsMap)
                                {
                                    HandleBulletObject(bullet.Key, bullet.Value);
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
                                    foreach (var enemy in (packet as ServerPacket).enemiesMap)
                                    {
                                        HandleEnemyObject(enemy.Key, enemy.Value);
                                    }
                                    foreach (var bullet in (packet as ServerPacket).bulletsMap)
                                    {
                                        HandleBulletObject(bullet.Key, bullet.Value);
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
                        playerGOMap[key].transform.position = InterpolatePosition(playerGOMap[key].transform.position, player.position);
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
    public void HandleEnemyObject(string key, GenericObject enemy)
    {
        // TODO
        switch (enemy.action)
        {
            case GenericObject.Action.CREATE:
                {
                    SpawnEnemy(enemy);
                    break;
                }
            case GenericObject.Action.UPDATE:
                {
                    if (enemyGOMap.ContainsKey(key))
                    {
                        // TODO: Perform interpolation
                        enemyGOMap[key].transform.position = InterpolatePosition(enemyGOMap[key].transform.position, enemy.position);
                        enemyGOMap[key].transform.rotation = enemy.rotation;
                    }
                    break;
                }
        }
    }
    public void HandleBulletObject(string key, GenericObject bullet)
    {
        // TODO
        switch (bullet.action)
        {
            case GenericObject.Action.CREATE:
                {
                    SpawnBullet(bullet);
                    break;
                }
            case GenericObject.Action.UPDATE:
                {
                    if (bulletGOMap.ContainsKey(key))
                    {
                        // TODO: Perform interpolation
                        bulletGOMap[key].transform.position = InterpolatePosition(bulletGOMap[key].transform.position, bullet.position);
                        bulletGOMap[key].transform.rotation = bullet.rotation;
                    }
                    break;
                }
           
        }
    }

    Vector3 InterpolatePosition(Vector3 startPos, Vector3 endPos)
    {
        float duration = 0.09f;
        float t = Time.deltaTime / duration;
        return Vector3.Lerp(startPos, endPos, t);
    }
    public void SpawnEnemy(GenericObject enemyObject)
    {
        GameObject enemyGO = Instantiate(enemyPrefab, enemyObject.position, new Quaternion(0, 0, 0, 0));
        // Set playerGO variables
        enemyGO.GetComponent<NetworkObject>().networkID = enemyObject.networkID;
        enemyGO.name = enemyObject.networkID;

        enemyGO.GetComponent<EnemyFollow>().enabled = false;
        enemyGO.GetComponent<NavMeshAgent>().enabled = false;
        enemyGO.GetComponent<Enemy>().enabled = false;

        // Now we add it to the list of player GO if it is not already there (change scene case)
        if (!enemyGOMap.ContainsKey(enemyObject.networkID))
        {
            enemyGOMap.Add(enemyObject.networkID, enemyGO);
        }
    }

    public void SpawnBullet(GenericObject bulletObject)
    {
        GameObject bulletGO = Instantiate(bulletPrefab, bulletObject.position, bulletObject.rotation);
        // Set playerGO variables
        bulletGO.GetComponent<NetworkObject>().networkID = bulletObject.networkID;
        bulletGO.name = bulletObject.networkID;
        bulletGO.GetComponent<Bullet>().Setup();

        // Now we add it to the list of player GO if it is not already there (change scene case)
        if (!bulletGOMap.ContainsKey(bulletObject.networkID))
        {
            bulletGOMap.Add(bulletObject.networkID, bulletGO);
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

    public void OnShoot()
    {
        Debug.Log("SHoot");
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