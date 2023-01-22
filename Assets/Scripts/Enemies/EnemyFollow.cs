using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    [HideInInspector]public NavMeshAgent enemyAgent;
    GameObject closestPlayer;
    GameObject[] players;

    // Start is called before the first frame update
    void Start()
    {
        enemyAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Fill the array with the active players (GameObjects with tag "Player").
        players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0)
        {
            return;
        }
        // Setting the first player in the array as the closest one (as a starting point).
        closestPlayer = players[0];
        float closestPlayerDistance = Vector3.Distance(closestPlayer.transform.position, this.transform.position);
        float currentPlayerDistance;

        // Traverse the array in order to see which one is closer to the enemy.
        for (int i = 0; i < players.Length; ++i)
        {
            // In case the current distance is lower than the previously saved one, we update it.
            currentPlayerDistance = Vector3.Distance(players[i].transform.position, this.transform.position);
            if (currentPlayerDistance < closestPlayerDistance)
            {
                closestPlayer = players[i];
                closestPlayerDistance = currentPlayerDistance;
            }
        }

        enemyAgent.SetDestination(closestPlayer.transform.position);
    }
}
