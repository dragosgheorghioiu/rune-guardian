using UnityEngine;
using UnityEngine.Events;

namespace RuneGuardian
{
    public class RuneGuardianController : MonoBehaviour
    {
        private ProjectileShooter _projectileShooter;
        private WaveController _waveController;
        private GestureRecognizerExample _gestureRecognizer;

        private InputData _inputData;

        public int WavesCompleted
        {
            get { return _waveController != null ? _waveController.WavesCompleted : 0; }
        }

        public int EnemiesDefeated
        {
            get { return _waveController != null ? _waveController.EnemiesDefeated : 0; }
        }

        private void Awake()
        {
            FindComponents();
        }

        private void FindComponents()
        {
            // Get components from the same GameObject first, then search the scene
            if (_waveController == null)
            {
                _waveController = GetComponent<WaveController>();
                if (_waveController == null)
                {
                    _waveController = FindObjectOfType<WaveController>();
                    if (_waveController == null)
                    {
                        Debug.LogError("WaveController not found in the scene!");
                    }
                }
            }
            if (_projectileShooter == null)
            {
                _projectileShooter = GetComponent<ProjectileShooter>();
                if (_projectileShooter == null)
                {
                    _projectileShooter = FindObjectOfType<ProjectileShooter>();
                    if (_projectileShooter == null)
                    {
                        Debug.LogError("ProjectileShooter not found in the scene!");
                    }
                }
            }
            if (_gestureRecognizer == null)
            {
                _gestureRecognizer = GetComponent<GestureRecognizerExample>();
                if (_gestureRecognizer == null)
                {
                    _gestureRecognizer = FindObjectOfType<GestureRecognizerExample>();
                }
            }
        }

        public void Init(InputData inputData,
            UnityAction onAllWavesCompletedAction,
            UnityAction onStartVibrationsAction)
        {
            _inputData = inputData;

            if (_waveController != null)
            {
                _waveController.Init(_inputData, onAllWavesCompletedAction, onStartVibrationsAction);
            }
            
            if (_gestureRecognizer != null)
            {
                _gestureRecognizer.Init(_projectileShooter, _inputData);
            }
        }

        public void UpdateGame(InputData inputData)
        {
            Debug.Log("RuneGuardianController UpdateGame called");

            _inputData = inputData;

            if (_waveController != null)
            {
                _waveController.UpdateGame(_inputData);
            }
        }

        public void StartGame()
        {
            if (_waveController != null)
            {
                Debug.Log("RuneGuardianController StartGame called");
                _waveController.StartGame();
            }
        }

        public void StopGame()
        {
            if (_waveController != null)
            {
                _waveController.StopGame();
            }
        }

        public void DestroyEnemyInstances()
        {
            if (_waveController != null)
            {
                _waveController.DestroyEnemyInstances();
            }
        }

        public void ApplyHapticFeedback()
        {
            if (_inputData.Haptic)
            {
                // Implement haptic feedback based on InputData settings
            }
        }
    }
}