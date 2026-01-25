using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    public ProjectileType type;
    public System.Action onProjectileDestroyed;

    [Header("Audio Effects")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip despawnSound;

    private AudioSource audioSource;
    private bool hitCorrectSpell = false;

    private void Awake()
    {
        // Create or get AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.playOnAwake = false;
            audioSource.volume = 1.0f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Projectile of type {type} collided with {collision.collider.name}");
        var obj = collision.collider.GetComponentInParent<SpawnedToy>();
        if (obj != null)
        {
            // Check if it's the correct spell before hitting
            hitCorrectSpell = obj.IsCorrectSpell(type);
            obj.TryHit(type);
            if (hitCorrectSpell)
            {
                PlayHitEffects(collision.contacts[0].point);
                DestroyProjectile();
                return;
            }
        }

        // Despawn on any collision (unless correct spell hit above)
        PlayDespawnEffects(collision.contacts[0].point);
        DestroyProjectile();
    }

    private void OnDestroy()
    {
        // Notify ShootSpell that this projectile is destroyed
        onProjectileDestroyed?.Invoke();

        // If destroyed naturally (lifetime expired) and didn't hit correct spell, play despawn effects
        if (gameObject.scene.isLoaded && !hitCorrectSpell)
        {
            PlayDespawnEffects(transform.position);
        }
    }

    private void PlayHitEffects(Vector3 position)
    {
        // Play hit sound at position
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, position);
        }
    }

    private void PlayDespawnEffects(Vector3 position)
    {
        // Play despawn sound at position
        if (despawnSound != null)
        {
            AudioSource.PlayClipAtPoint(despawnSound, position);
        }
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
