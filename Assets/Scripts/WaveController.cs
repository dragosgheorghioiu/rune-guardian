using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RuneGuardian
{
    public class WaveController : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> _enemyPrefabs;
        
        private InputData _inputData;
        private int _currentWave = 0;
        private int _enemiesDefeated = 0;
        private int _wavesCompleted = 0;
        private List<GameObject> _activeEnemies;
        private float _enemySpawnTimer = 0f;
        private int _enemiesSpawnedInWave = 0;
        private bool _gameActive = false;

        private UnityAction _onAllWavesCompletedAction;
        private UnityAction _onStartVibrationsAction;

        public int WavesCompleted
        {
            get { return _wavesCompleted; }
        }

        public int EnemiesDefeated
        {
            get { return _enemiesDefeated; }
        }

        public int CurrentWave
        {
            get { return _currentWave; }
        }

        public void Init(InputData inputData,
            UnityAction onAllWavesCompletedAction,
            UnityAction onStartVibrationsAction)
        {
            _inputData = inputData;
            _onAllWavesCompletedAction = onAllWavesCompletedAction;
            _onStartVibrationsAction = onStartVibrationsAction;

            _activeEnemies = new List<GameObject>();
            _currentWave = 0;
            _enemiesDefeated = 0;
            _wavesCompleted = 0;
        }

        private void Update()
        {
            if (!_gameActive || _inputData == null)
                return;
            
            Debug.Log("WaveController Game Active");

            // Update enemy spawning every frame
            UpdateEnemySpawning();

            // Check if wave is complete
            if (_enemiesSpawnedInWave >= _inputData.EnemyCount && _activeEnemies.Count == 0)
            {
                CompleteWave();
            }
        }

        public void UpdateGame(InputData inputData)
        {
            _inputData = inputData;

            if (!_gameActive)
                return;

            // Update enemy spawning
            UpdateEnemySpawning();

            // Check if wave is complete
            if (_enemiesSpawnedInWave >= _inputData.EnemyCount && _activeEnemies.Count == 0)
            {
                CompleteWave();
            }
        }

        public void StartGame()
        {
            _gameActive = true;
            _currentWave = 0;
            _enemiesDefeated = 0;
            _wavesCompleted = 0;
            _enemySpawnTimer = 0f;
            _enemiesSpawnedInWave = 0;

            Debug.Log("WaveController StartGame called");

            StartWave();
        }

        public void StopGame()
        {
            _gameActive = false;
            DestroyEnemyInstances();
        }

        public void DestroyEnemyInstances()
        {
            foreach (GameObject enemy in _activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            _activeEnemies.Clear();
        }

        private void StartWave()
        {
            _currentWave++;
            _enemiesSpawnedInWave = 0;
            _enemySpawnTimer = 0f;

            if (_onStartVibrationsAction != null)
            {
                _onStartVibrationsAction.Invoke();
            }
        }

        private void UpdateEnemySpawning()
        {
            // Only spawn if we haven't reached the enemy count for this wave
            if (_enemiesSpawnedInWave >= _inputData.EnemyCount)
                return;

            _enemySpawnTimer += Time.deltaTime;

            // Spawn delay is converted from milliseconds to seconds
            float spawnDelay = _inputData.EnemySpawnRate / 1000f;

            if (_enemySpawnTimer >= spawnDelay)
            {
                SpawnEnemy();
                _enemySpawnTimer = 0f;
                _enemiesSpawnedInWave++;
            }
        }

        private void SpawnEnemy()
        {
            if (_enemyPrefabs == null || _enemyPrefabs.Count == 0)
            {
                Debug.LogWarning("No enemy prefabs assigned to WaveController");
                return;
            }

            // Select a random enemy prefab
            int randomIndex = Random.Range(0, _enemyPrefabs.Count);
            GameObject enemyPrefab = _enemyPrefabs[randomIndex];

            // Instantiate the enemy
            GameObject enemy = Instantiate(enemyPrefab, transform);

            Debug.LogWarning("Spawned enemy: " + enemy.name);

            // Configure enemy based on input data
            // This assumes enemies have a script with these properties
            var enemyComponent = enemy.GetComponent<IEnemy>();
            if (enemyComponent != null)
            {
                enemyComponent.SetHealth(_inputData.EnemyHealth);
                enemyComponent.SetSpeed(_inputData.EnemySpeed);
                enemyComponent.SetDifficulty(_inputData.GameType);
            }

            // Find and assign target to Golem
            var golemComponent = enemy.GetComponent<Golem>();
            if (golemComponent != null)
            {
                GameObject target = GameObject.Find("Target");
                if (target != null)
                {
                    golemComponent.targetPoint = target.transform;
                    Debug.Log("Assigned Target to Golem");
                }
                else
                {
                    Debug.LogWarning("Target GameObject not found in scene!");
                }
            }

            _activeEnemies.Add(enemy);
        }

        private void CompleteWave()
        {
            _wavesCompleted++;

            // Check if all waves are complete
            if (_wavesCompleted >= _inputData.MaxWaves)
            {
                AllWavesCompleted();
            }
            else
            {
                // Start next wave
                StartWave();
            }
        }

        private void AllWavesCompleted()
        {
            _gameActive = false;

            if (_onAllWavesCompletedAction != null)
            {
                _onAllWavesCompletedAction.Invoke();
            }
        }

        public void OnEnemyDefeated(GameObject enemy)
        {
            _enemiesDefeated++;
            _activeEnemies.Remove(enemy);
            Destroy(enemy);
        }
    }

    public interface IEnemy
    {
        void SetHealth(int health);
        void SetSpeed(int speed);
        void SetDifficulty(RuneDifficulty difficulty);
    }
}
