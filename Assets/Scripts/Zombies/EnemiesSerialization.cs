using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

class EnemiesState
{
    public int enemyCount;
    public List<int> enemiesIDs;
    public List<Vector3> enemiesPositions;
}

public class EnemiesSerialization : MonoBehaviour
{
    GameObject[] enemies;
    List<int> enemiesIDs = new List<int>();
    List<Vector3> enemiesPositions = new List<Vector3>();
    MemoryStream stream;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Fill the array with the enemies (GameObjects with tag "Enemy").
        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        enemiesIDs.Clear();
        enemiesPositions.Clear();

        // Traverse the array to save each and every enemy position.
        for (int i = 0; i < enemies.Length; ++i)
        {
            enemiesIDs.Add(enemies[i].GetInstanceID());
            enemiesPositions.Add(enemies[i].transform.position);
        }

        EnemiesState enemiesStateToSerialize = new EnemiesState();
        enemiesStateToSerialize.enemyCount = enemiesPositions.Count;
        enemiesStateToSerialize.enemiesIDs = enemiesIDs;
        enemiesStateToSerialize.enemiesPositions = enemiesPositions;

        SerializeJson(enemiesStateToSerialize);
        EnemiesState enemiesStateDeserialized = DeserializeJson();

        enemiesIDs.Clear();
        enemiesPositions.Clear();

        for (int i = 0; i < enemies.Length; ++i)
        {
            enemies[i].transform.position = enemiesStateDeserialized.enemiesPositions[i];
        }
    }

    void SerializeJson(EnemiesState enemiesState)
    {
        string json = JsonUtility.ToJson(enemiesState);
        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(json);
    }

    EnemiesState DeserializeJson()
    {
        var t = new EnemiesState();
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        string json = reader.ReadString();
        Debug.Log(json);
        t = JsonUtility.FromJson<EnemiesState>(json);
        Debug.Log(t.enemyCount.ToString());

        return t;
    }
}
