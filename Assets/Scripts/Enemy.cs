using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public enum EnemyState { Idle, Trace, Attack, RunAway}
    public EnemyState state = EnemyState.Idle;


    public float moveSpeed = 2f;
    public float traceRange = 15f;
    public float attackRange = 6f;
    public float attackCooldown = 1.5f;

    public Slider hpSlider;

    public GameObject projectilePrefab;
    public Transform firePoint;

    private Transform player;
    private float lastAttackTime;
    public int maxHP = 5;
    private int currentHP;
    public float runAwayFromPlayer = 5f;
    //public Collider playerIn;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        lastAttackTime = -attackCooldown;
        currentHP = maxHP;

        hpSlider.value = 1f;
    }


    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);

        if(currentHP <= maxHP * 0.2f && state != EnemyState.Idle )
        {
            state = EnemyState.RunAway;
        }
        

            switch (state)
            {
                case EnemyState.Idle:
                    if (dist < traceRange)
                        state = EnemyState.Trace;
                    break;
                case EnemyState.Trace:
                    if (dist < attackRange)
                        state = EnemyState.Attack;
                    else if (dist > traceRange)
                        state = EnemyState.Idle;
                    else
                        TracePlayer();
                    break;
                case EnemyState.Attack:
                    if (dist > attackRange)
                        state = EnemyState.Trace;
                    else
                        AttackPlayer();
                    break;
                case EnemyState.RunAway:
                if (dist > traceRange)
                    state = EnemyState.Idle;
                else
                    RunAway();
                    break;
            }
        
        
        //수정전 코드
        //Vector3 direction = (player.position - transform.position).normalized;
        //transform.position += direction * moveSpeed * Time.deltaTime;
        //transform.LookAt(player.position);
    }
    public void EnemyTakeDamage(int damage)
    {
        currentHP -= damage;

        hpSlider.value = (float)currentHP / maxHP;

        if (currentHP <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        Destroy(gameObject);
    }

    void TracePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.LookAt(player.position);
    }
    void AttackPlayer()
    {
        if(Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.deltaTime;
            ShootProjectile();
        }
    }
    void ShootProjectile()
    {
        if(projectilePrefab != null && firePoint != null)
        {
            transform.LookAt(player.position);
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
            if(ep != null)
            {
                Vector3 dir = (player.position - firePoint.position).normalized;
                ep.SetDirection(dir);
            }
        }
    }
    void RunAway()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position -= direction * moveSpeed * Time.deltaTime;
        
    }
}
