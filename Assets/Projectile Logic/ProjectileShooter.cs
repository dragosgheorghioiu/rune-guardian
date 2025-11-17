using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileShooter : MonoBehaviour
{
    public Transform firePoint;
    public GameObject projectileTypeA;
    public GameObject projectileTypeB;

    public void Shoot(GameObject projPrefab)
    {
        Instantiate(projPrefab, firePoint.position, firePoint.rotation);
    }
}
