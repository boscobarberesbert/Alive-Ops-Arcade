using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Vector3 spawnPos;

    public void Respawn()
    {
        gameObject.GetComponent<EnemyFollow>().enemyAgent.Warp(spawnPos);
    }
}