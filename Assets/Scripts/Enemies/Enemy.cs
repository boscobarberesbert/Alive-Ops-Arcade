using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 spawnPos;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Respawn()
    {
        gameObject.GetComponent<EnemyFollow>().enemyAgent.Warp(spawnPos);
    }
}
