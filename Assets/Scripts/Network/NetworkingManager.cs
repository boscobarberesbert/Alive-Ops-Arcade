using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkingManager : MonoBehaviour
{
    public static NetworkingManager Instance;
    public INetworking networking;

    // Map to relate networkID to its gameobject
    public Dictionary<string, GameObject> playerGOMap = new Dictionary<string, GameObject>();

    // Reference to our player gameobject
    public GameObject myPlayerGO;

    // Map to relate networkID to its playerobject that is received through the network
    public Dictionary<string, PlayerObject> playerMap = new Dictionary<string, PlayerObject>();

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
        if (NetworkingManager.Instance.myPlayerGO)
            networking.UpdatePlayerState();

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

        // Check if we called or not the LoadScene() method to avoid spawns before loading
        if (!isSceneLoading)
        {
            playerMap.Clear();

            // Copy the dictionary from INetworking
            lock (networking.playerMapLock)
            {
                foreach (var entry in networking.playerMap)
                {
                    PlayerObject newObj = new PlayerObject(entry.Value.action, entry.Value.position, entry.Value.rotation);
                    playerMap.Add(entry.Key, newObj);
                }
            }

            foreach (var player in playerMap)
                HandlePlayerObject(player);
        }

        networking.OnUpdate();
    }

    void HandlePlayerObject(KeyValuePair<string, PlayerObject> player)
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
        GameObject playerGO = Instantiate(playerPrefab, player.position, player.rotation);
        playerGO.GetComponent<PlayerID>().networkID = networkID;
        playerGO.name = networkID;


        if (networkID == networking.myUserData.networkID)
            myPlayerGO = playerGO;
        else
        {
            // Disable scripts as we are don't want to be controlling the rest of players
            playerGO.GetComponent<PlayerController>().enabled = false;
            playerGO.GetComponent<CharacterController>().enabled = false;
            playerGO.GetComponent<MouseAim>().enabled = false;

            // TODO: Instance Players without Player Tag
            playerGO.tag = "Untagged";
        }

        playerGOMap.Add(networkID, playerGO);

        lock (networking.playerMapLock)
        {
            // Set the spawned player to be action none as it has already spawned
            if (networking.playerMap.ContainsKey(networkID))
            {
                networking.playerMap[networkID].action = PlayerObject.Action.NONE;
            }
        }
    }

    public void LoadScene(string sceneName)
    {
        (networking as NetworkingServer).LoadScene();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // We need to spawn the players ASAP as some scripts require that they exist at first
        foreach (var player in playerMap)
        {
            if (player.Value.action == PlayerObject.Action.CREATE)
            {
                if (onClientAdded != null)
                {
                    onClientAdded(player.Key, player.Value);
                }
            }
        }
        isSceneLoading = false;
    }

    void OnDisable()
    {
        networking.OnDisconnect();
    }
}