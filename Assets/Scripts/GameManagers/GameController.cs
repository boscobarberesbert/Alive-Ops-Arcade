using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    NetworkingManager networkManager;

    // Dynamic objects to be updated
    List<GameObject> playerList;
    List<GameObject> enemyList;

    // Start is called before the first frame update
    void Start()
    {
        networkManager = GameObject.FindGameObjectWithTag("NetworkingManager").GetComponent<NetworkingManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
