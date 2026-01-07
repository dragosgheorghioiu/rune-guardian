using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootSpell : MonoBehaviour
{
    [SerializeField] private Transform from;
    [SerializeField] private Transform to;

    [SerializeField] private GameObject cubeProjectile;
    [SerializeField] private GameObject sphereProjectile;
    [SerializeField] private GameObject capsuleProjectile;

    [SerializeField] private float speed = 12f;
    [SerializeField] private float spawnOffset = 0.2f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) { Debug.Log("H"); Fire(cubeProjectile); }
        if (Input.GetKeyDown(KeyCode.J)) { Debug.Log("J"); Fire(sphereProjectile); }
        if (Input.GetKeyDown(KeyCode.K)) { Debug.Log("K"); Fire(capsuleProjectile); }
    }

    private void Fire(GameObject prefab)
    {
        if (prefab == null || from == null || to == null) return;

        Vector3 dir = (to.position - from.position).normalized;
        Vector3 pos = from.position + dir * spawnOffset;

        var go = Instantiate(prefab, pos, Quaternion.LookRotation(dir));

        var rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        rb.velocity = dir * speed;
    }
}
