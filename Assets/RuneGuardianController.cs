using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RuneGuardian
{
    public class RuneGuardianController : MonoBehaviour
    {
        [SerializeField]
        private ProjectileShooter _projectileShooter;
        [SerializeField]
        private WaveController _waveController;
        [SerializeField]
        private GestureRecognizerExample _gestureRecognizer;

        private InputData _inputData;

        public int WavesCompleted
        {
            get { return _waveController.WavesCompleted; }
        }

        public int EnemiesDefeated
        {
            get { return _waveController.EnemiesDefeated; }
        }

        private void Start()
        {
            InitializeIfNeeded();
        }

        private void InitializeIfNeeded()
        {
            if (_waveController == null)
            {
                _waveController = new WaveController();
            }
            if (_projectileShooter == null)
            {
                _projectileShooter = new ProjectileShooter();
            }
        }

        public void Init(InputData inputData,
            UnityAction onAllWavesCompletedAction,
            UnityAction onStartVibrationsAction)
        {
            InitializeIfNeeded();

            _inputData = inputData;

            _waveController.Init(_inputData, onAllWavesCompletedAction, onStartVibrationsAction);
            
            if (_gestureRecognizer != null)
            {
                _gestureRecognizer.Init(_projectileShooter, _inputData);
            }
        }

        public void UpdateGame(InputData inputData)
        {
            InitializeIfNeeded();

            _inputData = inputData;

            _waveController.UpdateGame(_inputData);
        }

        public void StartGame()
        {
            _waveController.StartGame();
        }

        public void StopGame()
        {
            _waveController.StopGame();
        }

        public void DestroyEnemyInstances()
        {
            _waveController.DestroyEnemyInstances();
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