using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public int gunIndex = 0;

    public GameObject[] projectilePrefab;
    public Transform firePoint;
    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }


    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            gunIndex += 1;
        }



    }

    void Shoot()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPoint;
        targetPoint = ray.GetPoint(50f);
        Vector3 direction = (targetPoint - firePoint.position).normalized;

        int gunCountIndex = gunIndex % 2;
        GameObject proj = Instantiate(projectilePrefab[gunCountIndex], firePoint.position, Quaternion.LookRotation(direction));
    }
    void ChangeAttack()
    {
       
    }
}
