using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkingManager : MonoBehaviour
{
    public static NetworkingManager Instance;
    public INetworking networking;

    // Map to relate networkID to its gameobject
    public Dictionary<string, PlayerObject> playerMap = new Dictionary<string, PlayerObject>();
    public Dictionary<string, GameObject> playerGOMap = new Dictionary<string, GameObject>();

    delegate void OnClientAdded(string networkID, PlayerObject player);
    event OnClientAdded onClientAdded;

    public Vector3 startSpawnPosition;
    [SerializeField] GameObject playerPrefab;

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
        networking.myUserData = new User(System.Guid.NewGuid().ToString(), MainMenuInfo.username, MainMenuInfo.connectToIp);
    }

    private void Start()
    {
        // Starting networking
        networking.Start();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        networking.OnUpdate();

        lock (networking.loadSceneLock)
        {
            if (networking.triggerLoadScene)
            {
                SceneManager.LoadScene("Game");
                networking.triggerLoadScene = false;
            }
        }

        lock (networking.playerMapLock)
        {
            playerMap = networking.playerMap;

            if (playerMap.Count != 0)
                HandlePlayerMap();
        }
    }

    void HandlePlayerMap()
    {
        // TODO
        foreach (var player in playerMap)
        {
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
    }

    public void Spawn(string networkID, PlayerObject player)
    {
        GameObject playerGO = Instantiate(playerPrefab, player.position, player.rotation);
        playerGO.GetComponent<PlayerID>().networkID = networkID;
        playerGO.name = networkID;

        // Disable scripts as we are not going to be controlling the rest of players
        if (networkID != networking.myUserData.networkID)
        {
            playerGO.GetComponent<PlayerController>().enabled = false;
            playerGO.GetComponent<CharacterController>().enabled = false;
            playerGO.GetComponent<MouseAim>().enabled = false;

            // TODO Players without Player Tag
            playerGO.tag = "Untagged";
        }

        // Set the spawned player to be action none
        lock (networking.playerMapLock)
        {
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
        //Spawn();
    }

    void OnDisable()
    {
        networking.OnDisconnect();
    }
}