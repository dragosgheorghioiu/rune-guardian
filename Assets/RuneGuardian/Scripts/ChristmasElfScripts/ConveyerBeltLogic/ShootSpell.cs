using System.Collections;
using System.Collections.Generic;
using RuneGuardian;
using UnityEngine;

public class ShootSpell : MonoBehaviour
{
    [SerializeField] private Transform from;
    [SerializeField] private Transform to;
    
    [SerializeField] private List<GameObject> projectileTypes;

    [SerializeField] private float speed = 12f;
    [SerializeField] private float spawnOffset = 0.2f;
    [SerializeField] private float projectileLifetime = 3f;

    private bool isSpellInFlight = false;

    public void OnEnable()
    {
        GestureRecognizerExample.OnValidGesture += FireProjectile;
    }

    public void OnDisable()
    {
        GestureRecognizerExample.OnValidGesture -= FireProjectile;

    }

    public void FireProjectile(int projectileIndex)
    {
        if (isSpellInFlight) return; // Block if spell already in flight
        
        Fire(projectileTypes[projectileIndex % projectileTypes.Count]);
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
        
        isSpellInFlight = true;
        StartCoroutine(ResetSpellCooldown(projectileLifetime));
        
        Destroy(go, projectileLifetime);
    }

    private IEnumerator ResetSpellCooldown(float delay)
    {
        yield return new WaitForSeconds(delay);
        isSpellInFlight = false;
    }
}
