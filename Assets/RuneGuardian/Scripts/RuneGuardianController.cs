using System;
using UnityEngine;
using UnityEngine.Events;

namespace RuneGuardian
{
    public class RuneGuardianController
    {
        public static Action onRuneGuardianStart;
        public static Action<InputData> OnRuneGuardianInit;
        private InputData _inputData;

        public RuneGuardianController(InputData inputData)
        {
            _inputData = inputData;
            OnRuneGuardianInit?.Invoke(inputData);
        }

        public void UpdateGame(InputData inputData)
        {
            Debug.Log("RuneGuardianController UpdateGame called");

            _inputData = inputData;

            Debug.Log(inputData);
        }

        public void StartGame()
        {
            Debug.Log("RuneGuardianController StartGame called");
        }

        public void StopGame()
        {
        }

        public void DestroyEnemyInstances()
        {
        }
    }
}