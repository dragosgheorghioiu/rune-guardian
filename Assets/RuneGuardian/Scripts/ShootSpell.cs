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

    [Header("Audio Feedback")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip rejectSound;

    [Header("Ambient Music")]
    [SerializeField] private List<AudioSource> ambientAudioSources;

    private bool isSpellInFlight = false;
    private AudioSource audioSource;
    private Coroutine cooldownCoroutine;

    public void OnEnable()
    {
        GestureRecognizerExample.OnValidGesture += FireProjectile;

        // Start playing ambient music
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("No effetcs AudioSource found on ShootSpell!");
        }
        
        // Play all ambient audio sources
        if (ambientAudioSources != null)
        {
            foreach (var ambientSource in ambientAudioSources)
            {
                if (ambientSource != null && ambientSource.clip != null)
                {
                    ambientSource.loop = true;
                    ambientSource.Play();
                }
            }
        }
    }

    public void OnDisable()
    {
        GestureRecognizerExample.OnValidGesture -= FireProjectile;

        // Stop ambient music
        if (ambientAudioSources != null)
        {
            foreach (var ambientSource in ambientAudioSources)
            {
                if (ambientSource != null && ambientSource.isPlaying)
                {
                    ambientSource.Stop();
                }
            }
        }
    }

    public void FireProjectile(int projectileIndex)
    {
        if (isSpellInFlight)
        {
            // Play reject sound when blocked
            if (audioSource != null && rejectSound != null)
            {
                audioSource.PlayOneShot(rejectSound);
            }
            return; // Block if spell already in flight
        }

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
        
        // Set up callback to reset cooldown when projectile is destroyed
        var projectile = go.GetComponent<SpellProjectile>();
        if (projectile != null)
        {
            projectile.onProjectileDestroyed = ResetCooldown;
        }

        // Play shoot sound
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        isSpellInFlight = true;
        cooldownCoroutine = StartCoroutine(ResetSpellCooldown(projectileLifetime));

        Destroy(go, projectileLifetime);
    }

    private IEnumerator ResetSpellCooldown(float delay)
    {
        yield return new WaitForSeconds(delay);
        isSpellInFlight = false;
    }
    
    private void ResetCooldown()
    {
        // Stop the cooldown coroutine if it's running
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }
        isSpellInFlight = false;
    }
}
