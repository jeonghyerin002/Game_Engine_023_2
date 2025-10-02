using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    public float speed = 20f;           // 이동속도  
    public float lifeTime = 2f;         //생존 시간
    public int attack = 5;

    void Start()
    {
        //일정 시간 후 자동 삭제 (메모리 관리)
        Destroy(gameObject, lifeTime);
    }


    void Update()
    {
        //로컬의 forward 방향(앞)으로 이동
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemyHP = other.GetComponent<Enemy>();
            enemyHP.EnemyTakeDamage(attack);
          
            //Projectile 제거
            Destroy(gameObject);
        }
    }


}
