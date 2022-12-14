using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkingManager : MonoBehaviour
{
    public static NetworkingManager Instance;
    public INetworking networking;

    // Dynamic objects to be updated
    public List<NetworkUser> userList = new List<NetworkUser>();
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
        networking.myNetworkUser = new NetworkUser(
            MainMenuInfo.connectToIp,
            MainMenuInfo.isClient,
            MainMenuInfo.username,
            System.Guid.NewGuid().ToString());
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

        lock (networking._clientAddLock)
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

        lock (networking._loadSceneLock)
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
        lock(networking._userListLock)
        {
            userList = networking.networkUserList;
        }

        foreach (var user in userList)
        {
            if (!playerMap.ContainsKey(user.networkID) && user.player.action == DynamicObject.Action.CREATE)
            {
                Vector3 spawnPosition = new Vector3(startSpawnPosition.x + playerMap.Count * 3, 1, 0);

                GameObject playerGO = Instantiate(playerPrefab, spawnPosition, new Quaternion(0, 0, 0, 0));
                playerGO.name = user.username;
                playerGO.GetComponent<PlayerID>().networkID = user.networkID;

                // Disable scripts as we are not going to be controlling the rest of players
                if (user.networkID != networking.myNetworkUser.networkID)
                {
                    playerGO.GetComponent<PlayerController>().enabled = false;
                    playerGO.GetComponent<CharacterController>().enabled = false;
                    playerGO.GetComponent<MouseAim>().enabled = false;

                    // TODO Instance Players without Player Tag
                    //playerGO.tag = "Untagged";
                }

                // Add the recently created playerGO to our map
                playerMap.Add(user.networkID, playerGO);

                // TODO: Update the action to be performed to the user's player object
                //networking.networkUserList[i].player.action = DynamicObject.Action.UPDATE;
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