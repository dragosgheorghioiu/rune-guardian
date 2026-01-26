using System.Collections.Generic;
using RuneGuardian;
using UnityEngine;
using System.Threading.Tasks;

public class RandomToySpawner : MonoBehaviour
{
    [Header("Points")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private Transform despawnPoint;
    [SerializeField] private Transform portalPoint;
    [SerializeField] private List<ParticleSystem> CloudParticles;

    private static int numberOfSpawnedToys = 0;
    private static int maxToyNumber = 0;
    private static Quaternion extraToyRotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);

    [Header("Spawnable prefabs (3)")]
    [SerializeField] private SpawnedToy[] prefabs;
    private List<int> validToys;

    private SpawnedToy current;

    [SerializeField] private ConveyorController conveyor;

    private void OnEnable()
    {
        RuneGuardianController.OnRuneGuardianInit += Init;
    }
    private void OnDisable()
    {
        RuneGuardianController.OnRuneGuardianInit -= Init;
    }

    public void Init(InputData inputData)
    {
        validToys = new List<int>();
        if (inputData.enabledDirtyObjects) validToys.Add(0);
        if (inputData.enabledDestroyedObjects) validToys.Add(1);
        if (inputData.enabledUncoloredObjects) validToys.Add(2);
        maxToyNumber = inputData.numberOfToys;
    }

    public async void DelayedSpawnRandom()
    {
        if (numberOfSpawnedToys >= maxToyNumber)
        {
            Debug.Log("Game end");
            conveyor.StopConveyor();
            return;
        }
        await Task.Delay(1000);
        SpawnRandom();
    }

    public void SpawnRandom()
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("No prefabs set in RandomToySpawner");
            return;
        }
        if (spawnPoint == null || targetPoint == null || despawnPoint == null)
        {
            Debug.LogWarning("No target/spawn/despawn");
            return;
        }

        int idx = Random.Range(0, validToys.Count);
        current = Instantiate(prefabs[validToys[idx]], spawnPoint.position, spawnPoint.rotation * extraToyRotation);
        current.Init(targetPoint, despawnPoint, portalPoint);
        ++numberOfSpawnedToys;

        conveyor?.Reverse();
        conveyor?.StartConveyor();

        current.OnArrivedTarget += () =>
        {
            conveyor?.StopConveyor();
        };

        current.OnStartedDespawn += () =>
        {
            conveyor?.StartConveyor();
        };
        
        current.onToyHit += () =>
        {
            conveyor?.Reverse();
            foreach (var Cloud in CloudParticles)
            {
                Cloud.Play();
            }
        };

        current.onToyRepaired += () =>
        {
            foreach (var Cloud in CloudParticles)
            {
                Cloud.Stop();
            }
        };
    }
}
