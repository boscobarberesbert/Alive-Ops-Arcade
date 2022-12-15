using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkingManager : MonoBehaviour
{
    public static NetworkingManager Instance;
    public INetworking networking;

    // Map to relate networkID to its gameobject
    public Dictionary<string, GameObject> playerMap = new Dictionary<string, GameObject>();

    delegate void OnClientAdded();
    event OnClientAdded onClientAdded;

    [SerializeField] GameObject playerPrefab;
    [SerializeField] Vector3 startSpawnPosition;

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

        // Creating network (client or server) Interface Networking
        networking = MainMenuInfo.isClient ? new NetworkingClient() : new NetworkingServer();

        // Initializing user data
        networking.myUserData = new UserData(MainMenuInfo.username, MainMenuInfo.connectToIp);

        networking.myPlayer = new DynamicObject();
    }

    private void Start()
    {
        // Starting networking
        networking.Start();

        //SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        networking.OnUpdate();

        lock (networking.clientAddLock)
        {
            if (networking.triggerClientAdded)
            {
                if (onClientAdded != null)
                {
                    onClientAdded();
                }
                networking.triggerClientAdded = false;
            }
        }

        lock (networking.loadSceneLock)
        {
            if (networking.triggerLoadScene)
            {
                playerMap.Clear();
                SceneManager.LoadScene("Game");
                networking.triggerLoadScene = false;
            }
        }
    }

    public void Spawn()
    {
    //    lock (networking.userListLock)
    //    {
    //        worldPlayerList = networking.playerList;
    //    }

    //    foreach (var player in networking.playerList)
    //    {
    //        if (!playerMap.ContainsKey(player.networkID) && player.action == DynamicObject.Action.CREATE)
    //        {
    //            Vector3 spawnPosition = new Vector3(startSpawnPosition.x + playerMap.Count * 3, 1, 0);

    //            GameObject playerGO = Instantiate(playerPrefab, spawnPosition, new Quaternion(0, 0, 0, 0));
    //            playerGO.GetComponent<PlayerID>().networkID = player.networkID;
    //            playerGO.name = player.networkID;

    //            // Disable scripts as we are not going to be controlling the rest of players
    //            if (player.networkID != networking.myPlayer.networkID)
    //            {
    //                playerGO.GetComponent<PlayerController>().enabled = false;
    //                playerGO.GetComponent<CharacterController>().enabled = false;
    //                playerGO.GetComponent<MouseAim>().enabled = false;

    //                // TODO Instance Players without Player Tag
    //                playerGO.tag = "Untagged";
    //            }

    //            // Add the recently created playerGO to our map
    //            playerMap.Add(player.networkID, playerGO);

    //            // TODO: Update the action to be performed to the user's player object
    //            networking.networkUserList[i].player.action = DynamicObject.Action.UPDATE;
    //        }
    //    }
    }

    public void ProcessPacketQueue(ref Queue<ClientPacket> packetQueue)
    {
        foreach (var packet in packetQueue)
        {
            switch (packet.player.action)
            {
                case DynamicObject.Action.CREATE:
                    {

                        break;
                    }
                case DynamicObject.Action.UPDATE:
                    {

                        break;
                    }
                case DynamicObject.Action.DESTROY:
                    {

                        break;
                    }
                case DynamicObject.Action.NONE:
                    {

                        break;
                    }
            }
        }
    }

    public void LoadScene(string sceneName)
    {
        (networking as NetworkingServer).LoadScene();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Spawn();
    }

    void OnDisable()
    {
        networking.OnDisconnect();
    }
}