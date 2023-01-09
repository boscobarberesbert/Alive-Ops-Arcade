using AliveOpsArcade.OdinSerializer.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkingManager : MonoBehaviour
{
    public static NetworkingManager Instance;
    public INetworking networking;

    // Reference to our player gameobject
    public GameObject myPlayerGO;

    // Map to relate networkID to its gameobject
    Dictionary<string, GameObject> playerGOMap = new Dictionary<string, GameObject>();

    delegate void OnClientAdded(string networkID, PlayerObject player);
    event OnClientAdded onClientAdded;

    // Condition to know if the LoadScene() method has been called
    bool isSceneLoading = false;

    [SerializeField] GameObject playerPrefab;
    public Vector3 startSpawnPosition;
    public float updateTime = 0.1f;

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

                SceneManager.LoadScene("Game");

                networking.triggerLoadScene = false;
            }
        }
        if (!isSceneLoading)
        {
            networking.OnUpdate();
            lock (networking.playerMapLock)
            {
                foreach (var player in networking.playerMap)
                {
                    HandlePlayerObject(player);
                }
                networking.UpdatePlayerState();
            }
        }


    }

    public void HandlePlayerObject(KeyValuePair<string, PlayerObject> player)
    {
        // TODO
        switch (player.Value.action)
        {
            case PlayerObject.Action.CREATE:
                {
                    if (onClientAdded != null)
                    {
                        onClientAdded(player.Key, player.Value);
                    }
                    break;
                }
            case PlayerObject.Action.UPDATE:
                {
                    if (playerGOMap.ContainsKey(player.Key) && player.Key != networking.myUserData.networkID)
                    {
                        // TODO: Perform interpolation
                        playerGOMap[player.Key].transform.position = player.Value.position;
                        playerGOMap[player.Key].transform.rotation = player.Value.rotation;
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

    public void Spawn(string networkID, PlayerObject player)
    {
        //Instantiate the game object at the required position
        GameObject playerGO = Instantiate(playerPrefab, player.position, player.rotation);
        //Set playerGO variables
        playerGO.GetComponent<PlayerID>().networkID = networkID;
        playerGO.name = networkID;
        //if the object created is mine, add it to myPlayerGO variable
        if (networkID == networking.myUserData.networkID)
        {
            myPlayerGO = playerGO;
        }
        else
        {
            //since the player is not ours we don't want to control it with our inputs
            playerGO.GetComponent<PlayerController>().enabled = false;
            playerGO.GetComponent<CharacterController>().enabled = false;
            playerGO.GetComponent<MouseAim>().enabled = false;
            // TODO: Instance Players without Player Tag
            playerGO.tag = "Untagged";
        }

        //Now we add it to the list of player GO if it is not already there (change scene case)
        if (!playerGOMap.ContainsKey(networkID))
        {
            playerGOMap.Add(networkID, playerGO);

        }
        ////Finally we broadcast the corresponding packet to the clients
        if (networking is NetworkingServer)
            (networking as NetworkingServer).NotifySpawn(networkID);


        networking.playerMap[networkID].action = PlayerObject.Action.NONE;
    }

    public void LoadScene(string sceneName)
    {
        // Set all player objects to be created with its respective positions
        int i = 0;
        foreach (var entry in networking.playerMap)
        {
            Vector3 spawnPosition = new Vector3(NetworkingManager.Instance.startSpawnPosition.x + i * 3, 1, 0);
            networking.playerMap[entry.Key].action = PlayerObject.Action.CREATE;
            ++i;
        }

        // Broadcast the corresponding message to the clients
        if (networking is NetworkingServer)
            (networking as NetworkingServer).NotifySceneChange(sceneName);

        networking.triggerLoadScene = true;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // We need to spawn the players ASAP as some scripts require that they exist at first
        lock (networking.playerMapLock)
        {


            foreach (var player in networking.playerMap)
            {
                //if (player.Value.action == PlayerObject.Action.CREATE)
                //{
                if (onClientAdded != null)
                {
                    onClientAdded(player.Key, player.Value);
                }
                //}
            }
        }

        isSceneLoading = false;
    }

    void OnDisable()
    {
        networking.OnDisconnect();
    }
}