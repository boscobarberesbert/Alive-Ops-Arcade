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
    [SerializeField] Transform[] positions;
    GameObject serverManager;
    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerData.client)
        {
            //If there is not a host create one
            Instantiate(serverPrefab, positions[0].transform.position, positions[0].transform.rotation);
           serverManager =  Instantiate(serverManagerPrefab);
            serverManager.GetComponent<UDPServer>().onClientAdded += SpawnClient;

        }
        else
        {
            //If there is a host create a client for each client connected to the host
            Instantiate(clientPrefab, positions[1].transform.position, positions[1].transform.rotation);
            Instantiate(clientManagerPrefab);

        }
    }

    void SpawnClient()
    {
        Instantiate(clientPrefab, positions[1].transform.position, positions[1].transform.rotation);
    }
}
