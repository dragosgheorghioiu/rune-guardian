using UnityEngine;

public class CloudToggle : MonoBehaviour
{
    private ParticleSystem ps;
    public SpawnedToy CurrentToy;
    
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
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