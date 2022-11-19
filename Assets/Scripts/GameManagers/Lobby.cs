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

    [SerializeField] GameObject clientPrefab;

    List<GameObject> players = new List<GameObject>();
    [SerializeField] List<Transform> spawnPoints = new List<Transform>();

    GameObject serverManager;
    // Start is called before the first frame update
    void Start()
    {


     
    }


    void SpawnClient()
    {
        players.Add(Instantiate(clientPrefab, spawnPoints[players.Count].position, spawnPoints[players.Count].rotation));
    }
}
