using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Threading;
using System;

public class Lobby : MonoBehaviour
{
    [SerializeField] GameObject serverPrefab;
    [SerializeField] GameObject serverManagerPrefab;
    [SerializeField] GameObject clientPrefab;
    [SerializeField] GameObject clientManagerPrefab;
  List<GameObject> players = new List<GameObject>();
    [SerializeField]  List<Transform> spawnPoints = new List<Transform>();

    GameObject serverManager;
    // Start is called before the first frame update
    void Start()
    {

        if (!PlayerData.client)
        {
            //If there is not a host create one
            players.Add(Instantiate(serverPrefab));
            serverManager =  Instantiate(serverManagerPrefab);
            serverManager.GetComponent<UDPServer>().onClientAdded += SpawnClient;

        }
        else
        {
            //If there is a host create a client for each client connected to the host
            players.Add(Instantiate(clientPrefab));
            Instantiate(clientManagerPrefab);

        }
    }


    void SpawnClient()
    {
        players.Add(Instantiate(clientPrefab, spawnPoints[players.Count].position, spawnPoints[players.Count].rotation));
    }
}
