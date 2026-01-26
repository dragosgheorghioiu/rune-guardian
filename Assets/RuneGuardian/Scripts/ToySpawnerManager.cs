using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RuneGuardian;

public class ToySpawnerManager : MonoBehaviour
{
    [SerializeField] private List<RandomToySpawner> toySpawners;

    private void OnEnable()
    {
        RuneGuardianController.onRuneGuardianStart += SpawnRandom;
        SpawnedToy.onToyDespawn += DelayedSpawnRandom;
    }
    private void OnDisable()
    {
        SpawnedToy.onToyDespawn -= DelayedSpawnRandom;
        RuneGuardianController.onRuneGuardianStart -= SpawnRandom;
    }

    public void SpawnRandom()
    {
        toySpawners[Random.Range(0, toySpawners.Count)].SpawnRandom();
    }
    public void DelayedSpawnRandom()
    {
        toySpawners[Random.Range(0, toySpawners.Count)].DelayedSpawnRandom();
    }

}
