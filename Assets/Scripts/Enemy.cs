using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    private Transform player;
    public float HP = 5f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }


    void Update()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.LookAt(player.position);
    }
    public void EnemyTakeDamage(float damage)
    {
        HP -= damage;

        if(HP <= 0)
        {
            Destroy(gameObject);
        }
    }

}
