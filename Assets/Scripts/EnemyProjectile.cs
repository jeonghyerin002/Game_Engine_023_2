using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public int damage = 2;
    public float speed = 8f;
    public float lifeTime = 3f;

    private Vector3 moveDir;

    public void SetDirection(Vector3 dir)
    {
        moveDir = dir.normalized;
    }


    void Start()
    {
        Destroy(gameObject, lifeTime);
    }


    void Update()
    {
        transform.position += moveDir * speed * Time.deltaTime;
    }

    void OnTirggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null) pc.TakeDamage(damage);

            Destroy(gameObject);

        }
    }
}
