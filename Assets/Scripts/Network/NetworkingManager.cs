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
        networking.OnUpdate();
        foreach(var player in networking.playerMap)
        {
            HandlePlayerObject(player);
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
        if(networkID == networking.myUserData.networkID)
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

        //Now we add it to the list of player GO
        playerGOMap.Add(networkID, playerGO);
        //Finally we broadcast the corresponding packet to the clients
        if (networking is NetworkingServer)
            (networking as NetworkingServer).NotifySpawn(networkID);


        networking.playerMap[networkID].action = PlayerObject.Action.NONE;
    }

    public void LoadScene(string sceneName)
    {
       
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
      
    }

    void OnDisable()
    {
        networking.OnDisconnect();
    }
}