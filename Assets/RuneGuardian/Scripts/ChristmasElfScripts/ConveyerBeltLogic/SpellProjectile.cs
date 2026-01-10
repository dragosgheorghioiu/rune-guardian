using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    public ProjectileType type;

    private void OnCollisionEnter(Collision collision)
    {
        var obj = collision.collider.GetComponentInParent<SpawnedToy>();
        if (obj != null)
        {
            obj.TryHit(type);
            Destroy(gameObject);
        }
    }
}
