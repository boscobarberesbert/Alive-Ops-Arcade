using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    GameObject[] enemies;
    [SerializeField]
    GameObject enemyPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length < 4)
        {
            GameObject.Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}
