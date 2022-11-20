using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Playables;

public class UserData
{
    public string connectToIP = "";
    public bool isClient = true;
    public string username = "";

    void SerializeJson()
    {
        string json = JsonUtility.ToJson(this);
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(json);
    }

    void DeserializeJson()
    {
        MemoryStream stream = new MemoryStream();
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        string json = reader.ReadString();
        Debug.Log(json);
        UserData newData = JsonUtility.FromJson<UserData>(json);
        this.connectToIP = newData.connectToIP;
        this.isClient = newData.isClient;
        this.username = newData.username;
    }
}

public class NetworkingManager : MonoBehaviour
{
    public static NetworkingManager Instance;
    public INetworking networking;

    public delegate void OnClientAdded();
    public event OnClientAdded onClientAdded;

    [SerializeField] GameObject playerPrefab;
    [SerializeField] Vector3 startSpawnPosition;
    List<GameObject> players = new List<GameObject>();

    UserData myUserData = new UserData();

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        myUserData.username = MainMenuInfo.username;
        myUserData.isClient = MainMenuInfo.isClient;
        myUserData.connectToIP = MainMenuInfo.connectToIp;

        if (myUserData.isClient)
        {

            NetworkClient client = new NetworkClient();
            client.myUserData = myUserData;
            networking = client;
            
        }
        else
        {
            NetworkingServer server = new NetworkingServer();
            networking = server;
        }

        networking.Start();        
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
    }
    private void OnDisable()
    {
        networking.OnDisconnect();
    }


    public void Spawn()
    {
        //Vector3 spawnPosition = new Vector3(startSpawnPosition.x + players.Count * 3, 0, 0);
        //GameObject myPlayer = Instantiate(playerPrefab, spawnPosition, new Quaternion(0,0,0,0));
        //myPlayer.name = myUserData.username;
        //players.Add(myPlayer);
        //SpawnPlayer(myPlayer.GetComponent<PlayerSerialization>().GetPlayerState().playerID);
    }


}