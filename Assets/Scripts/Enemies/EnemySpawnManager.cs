using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] int numberOfEnemiesToSpawn;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numberOfEnemiesToSpawn; i++)
        {
            GameObject enemy = GameObject.Instantiate(enemyPrefab, gameObject.transform.position, Quaternion.identity);
            enemy.GetComponent<Enemy>().spawnPos = gameObject.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        

    }
}
