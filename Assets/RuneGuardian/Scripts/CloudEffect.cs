using UnityEngine;

public class CloudToggle : MonoBehaviour
{
    private ParticleSystem ps;
    
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void OnEnable()
    {
        SpawnedToy.onToyHit += StartParticles;
        SpawnedToy.onToyRepaired += StopParticles;
    }
    void OnDisable()
    {
        SpawnedToy.onToyHit -= StartParticles;
        SpawnedToy.onToyRepaired -= StopParticles;
    }
    
    public void StartParticles()
    {
        ps.Play();
    }
    
    public void StopParticles()
    {
        ps.Stop();
    }
}