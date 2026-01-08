using System.Collections;
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

    [Header("Spawnable prefabs (3)")]
    [SerializeField] private SpawnedToy[] prefabs;
    private List<int> validToys;

    private SpawnedToy current;

    [SerializeField] private ConveyorController conveyor;

    private void OnEnable()
    {
       RuneGuardianController.OnRuneGuardianInit += Init;
       RuneGuardianController.onRuneGuardianStart += SpawnRandom; 
       SpawnedToy.onToyDespawn += DelayedSpawnRandom; 
    }
    private void OnDisable()
    {
       RuneGuardianController.onRuneGuardianStart -= SpawnRandom; 
       RuneGuardianController.OnRuneGuardianInit -= Init;
       SpawnedToy.onToyDespawn -= DelayedSpawnRandom; 
    }

    public void Init(InputData inputData)
    {
        validToys = new List<int>();
        if (inputData.enabledDirtyObjects) validToys.Add(0);
        if (inputData.enabledDestroyedObjects) validToys.Add(1);
        if (inputData.enabledUncoloredObjects) validToys.Add(2);
    }

    public async void DelayedSpawnRandom()
    {
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
        current = Instantiate(prefabs[validToys[idx]], spawnPoint.position, spawnPoint.rotation);
        current.Init(targetPoint, despawnPoint);

        if (conveyor != null) conveyor.StartConveyor();

        current.OnArrivedTarget += () =>
        {
            if (conveyor != null) conveyor.StopConveyor();
        };

        current.OnStartedDespawn += () =>
        {
            if (conveyor != null) conveyor.StartConveyor();
        };
    }
}
