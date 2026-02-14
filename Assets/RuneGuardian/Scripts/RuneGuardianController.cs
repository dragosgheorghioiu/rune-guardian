using System;
using UnityEngine;

namespace RuneGuardian
{
    /// <summary>
    /// The primary controller for the Rune Guardian game logic. Orchestrates game state and initialization.
    /// </summary>
    public class RuneGuardianController
    {
        /// <summary>
        /// Event triggered when the game is ready to start after initialization.
        /// </summary>
        public static Action onRuneGuardianStart;

        /// <summary>
        /// Event triggered when the game is being initialized with input data.
        /// </summary>
        public static Action<InputData> OnRuneGuardianInit;

        private InputData _inputData;

        /// <summary>
        /// Initializes a new instance of the RuneGuardianController.
        /// </summary>
        /// <param name="inputData">Configuration data for the session.</param>
        public RuneGuardianController(InputData inputData)
        {
            _inputData = inputData;

            OnRuneGuardianInit?.Invoke(inputData);
        }

        public void UpdateGame(InputData inputData)
        {
            _inputData = inputData;
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