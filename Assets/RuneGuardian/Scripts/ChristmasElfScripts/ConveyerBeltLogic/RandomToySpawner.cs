using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomToySpawner : MonoBehaviour
{
    [Header("Points")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private Transform despawnPoint;

    [Header("Spawnable prefabs (3)")]
    [SerializeField] private SpawnedToy[] prefabs;

    [Header("Spawn rules")]
    [SerializeField] private bool autoSpawnIfNone = true;

    private SpawnedToy current;

    [SerializeField] private ConveyorController conveyor;

    private void Start()
    {
        SpawnRandom();
    }

    private void Update()
    {
        if (!autoSpawnIfNone) return;

        if (current == null)
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

        int idx = Random.Range(0, prefabs.Length);
        current = Instantiate(prefabs[idx], spawnPoint.position, spawnPoint.rotation);
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
