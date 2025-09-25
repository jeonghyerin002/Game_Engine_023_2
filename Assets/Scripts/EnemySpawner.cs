using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnerInterval = 3f;
    public float spawnerRange = 5f;
    private float timer = 0f;


    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= spawnerInterval)
        {
            Vector3 spawnPos = new Vector3(
                transform.position.x + Random.Range(-spawnerRange, spawnerRange),
                transform.position.y,
                transform.position.z + Random.Range(-spawnerRange, spawnerRange)
            );

            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            timer = 0f;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnerRange * 2, 0.1f, spawnerRange * 2));
    }
}
