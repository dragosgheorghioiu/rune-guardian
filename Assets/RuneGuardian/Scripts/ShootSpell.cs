using System.Collections;
using System.Collections.Generic;
using RuneGuardian;
using UnityEngine;

public class ShootSpell : MonoBehaviour
{
    [SerializeField] private Transform to;

    [SerializeField] private List<GameObject> projectileTypes;

    [SerializeField] private float speed = 12f;
    [SerializeField] private float spawnOffset = 0.5f;
    [SerializeField] private float projectileLifetime = 3f;

    [Header("Audio Feedback")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip rejectSound;

    [Header("Ambient Music")]
    [SerializeField] private List<AudioSource> ambientAudioSources;

    [Header("Grid Mode")]
    [SerializeField] public GestureRecognizerExample gestureRecognizer;

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
            Debug.LogWarning("No effects AudioSource found on ShootSpell!");
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

    public void FireProjectile(int projectileIndex, Vector3 spawnPosition)
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

        Fire(projectileTypes[projectileIndex % projectileTypes.Count], spawnPosition);
    }

    private void Fire(GameObject prefab, Vector3 spawnPosition)
    {
        if (prefab == null) return;

        Vector3 dir;
        Vector3 pos;
        if (gestureRecognizer != null && gestureRecognizer.IsGridModeActive())
        {
            // Shoot in the direction the camera is looking, but spawn from last drawing point
            var cam = gestureRecognizer.mainCamera;
            dir = cam.transform.forward.normalized;
            pos = spawnPosition + dir * spawnOffset;
        }
        else if (to != null)
        {
            dir = (to.position - spawnPosition).normalized;
            pos = spawnPosition + dir * spawnOffset;
        }
        else
        {
            Debug.LogWarning("No target for projectile!");
            return;
        }

        var go = Instantiate(prefab, pos, Quaternion.LookRotation(dir));
        
        // Set projectile to Projectile layer to avoid colliding with VR objects
        int projectileLayer = LayerMask.NameToLayer("Projectile");
        if (projectileLayer != -1)
        {
            go.layer = projectileLayer;
            // Ignore collisions between Projectile and VR layers
            int vrLayer = LayerMask.NameToLayer("VR");
            if (vrLayer != -1)
            {
                Physics.IgnoreLayerCollision(projectileLayer, vrLayer, true);
            }
        }

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
