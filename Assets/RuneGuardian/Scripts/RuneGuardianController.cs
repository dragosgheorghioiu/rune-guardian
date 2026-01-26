using System;
using UnityEngine;

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
            _inputData = inputData;

            Debug.Log(inputData);
        }

        public void StartGame()
        {
        }

        public void StopGame()
        {
        }

        public void DestroyEnemyInstances()
        {
        }
    }
}