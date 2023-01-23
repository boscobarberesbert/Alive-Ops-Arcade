using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    public Vector3[] spawnPoints;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] int numberOfEnemiesToSpawn;

    void Start()
    {
        for (int i = 0; i < numberOfEnemiesToSpawn; i++)
        {
            GameObject enemy = GameObject.Instantiate(enemyPrefab, gameObject.transform.position, Quaternion.identity);
            enemy.GetComponent<Enemy>().spawnPos = gameObject.transform.position;
        }
    }
}