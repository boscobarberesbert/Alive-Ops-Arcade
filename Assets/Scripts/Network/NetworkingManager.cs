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

    delegate void OnClientAdded();
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

        lock (networking.playerMapLock)
        {
            playerMap = networking.playerMap;

            if (playerMap.Count != 0)
                UpdatePlayers();
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

    void UpdatePlayers()
    {

    }

    public void Spawn()
    {
        foreach (var player in playerMap)
        {
            if (player.Value.action == PlayerObject.Action.CREATE)
            {
                GameObject playerGO = Instantiate(playerPrefab, player.Value.position, player.Value.rotation);
                playerGO.GetComponent<PlayerID>().networkID = player.Key;
                playerGO.name = player.Key;

                // Disable scripts as we are not going to be controlling the rest of players
                if (player.Key != networking.myUserData.networkID)
                {
                    playerGO.GetComponent<PlayerController>().enabled = false;
                    playerGO.GetComponent<CharacterController>().enabled = false;
                    playerGO.GetComponent<MouseAim>().enabled = false;

                    // TODO Instance Players without Player Tag
                    playerGO.tag = "Untagged";
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