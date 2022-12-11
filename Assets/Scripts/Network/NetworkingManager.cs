using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        // Creating network (client or server) Interface Networking
        networking = MainMenuInfo.isClient ? new NetworkingClient() : new NetworkingServer();

        // Initializing user data
        networking.myNetworkUser = new NetworkUser(
            MainMenuInfo.connectToIp,
            MainMenuInfo.isClient,
            MainMenuInfo.username,
            System.Guid.NewGuid().ToString());

        Debug.Log("PLAYER DATA IS CREATED");
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

        if (networking.triggerClientAdded)
        {
            if (onClientAdded != null)
            {
                onClientAdded();
            }
            networking.triggerClientAdded = false;
        }

        if (networking.triggerLoadScene)
        {
            players.Clear();
            SceneManager.LoadScene("Game");
            networking.triggerLoadScene = false;
        }

        // TODO: Refactor
        //foreach (GameObject player in players)
        //{
        //    if (networking.myPlayerData.playerID == player.GetComponent<PlayerID>().playerId)
        //    {
        //        player.transform.position = networking.myPlayerData.position;
        //        player.transform.rotation = networking.myPlayerData.rotation;
        //    }
        //}
    }

    public void Spawn()
    {
        foreach (var player in networking.networkUserList)
        {
            if (!GameObject.Find(player.username))
            {
                Vector3 spawnPosition = new Vector3(startSpawnPosition.x + players.Count * 3, 1, 0);

                GameObject playerGO = Instantiate(playerPrefab, spawnPosition, new Quaternion(0, 0, 0, 0));
                playerGO.name = player.username;
                playerGO.GetComponent<PlayerID>().networkID = player.networkID;

                // Disable scripts as we are not going to be controlling the rest of players
                if (player.networkID != networking.myNetworkUser.networkID)
                {
                    playerGO.GetComponent<PlayerController>().enabled = false;
                    playerGO.GetComponent<CharacterController>().enabled = false;
                    playerGO.GetComponent<MouseAim>().enabled = false;
                    playerGO.tag = "Untagged";
                }
                players.Add(playerGO);
            }
        }
    }

    public void LoadScene(string sceneName)
    {
        (networking as NetworkingServer).LoadScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Spawn();
    }

    private void OnDisable()
    {
        networking.OnDisconnect();
    }
}