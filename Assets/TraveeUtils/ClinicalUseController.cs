using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGuardian
{
    /// <summary>
    /// Data type used for the object assigned with controlling game
    /// behavior in a clinical context.
    /// </summary>
    public class ClinicalUseController : GameManagerBase
    {
        /// <summary>
        /// A GameObject that contains a panel with a game-over message.
        /// </summary>
        [SerializeField]
        private GameObject _finishGamePanel;

        /// <summary>
        /// Object that contains the input parameter values for the game.
        /// </summary>
        private InputData _inputData;

        public void Init(InputData inputData)
        {
            _inputData = inputData;

            // Game logic
            // Game logic: log only valid InputData fields
            Debug.Log("BodySide: " + _inputData.BodySide);
            Debug.Log("GameType: " + _inputData.GameType);
            Debug.Log("EnemyCount: " + _inputData.EnemyCount);
            Debug.Log("Haptic: " + _inputData.Haptic);
            Debug.Log("RoundDuration: " + _inputData.RoundDuration);

            _eventsManager.EventName = "onNoGameIsPlaying";
        }

        public void UpdateGame(InputData inputData)
        {
            _inputData = inputData;

            // Game logic
            // Game logic: log only valid InputData fields
            Debug.Log("BodySide: " + _inputData.BodySide);
            Debug.Log("GameType: " + _inputData.GameType);
            Debug.Log("EnemyCount: " + _inputData.EnemyCount);
            Debug.Log("Haptic: " + _inputData.Haptic);
            Debug.Log("RoundDuration: " + _inputData.RoundDuration);
        }

        public void StartGame()
        {
            // Game logic
            // Game logic: log only valid InputData fields
            Debug.Log("BodySide: " + _inputData.BodySide);
            Debug.Log("GameType: " + _inputData.GameType);
            Debug.Log("EnemyCount: " + _inputData.EnemyCount);
            Debug.Log("Haptic: " + _inputData.Haptic);
            Debug.Log("MaxWaves: " + _inputData.MaxWaves);
            Debug.Log("SpellDamage: " + _inputData.SpellDamage);

            _timerController.StartTimer();

            _eventsManager.EventName = "onGameIsPlaying";

            _outputDataController.SendNoteInformation(
                "Start sesiune joc. "
                + "BodySide: " + _inputData.BodySide + ". "
                + "GameType: " + _inputData.GameType + ". "
                + "EnemyCount: " + _inputData.EnemyCount + ". "
                + "SpellDamage: " + _inputData.SpellDamage + ". "
                + "RoundDuration: " + _inputData.RoundDuration + "s. "
                + "Haptic: " + _inputData.Haptic + "."
            );
        }

        public void StopGame()
        {
            OnGameFinished();
        }

        protected void OnTimeFinished()
        {
            OnGameFinished();
        }

        private void OnGameFinished()
        {
            // Game logic
            // ...

            _finishGamePanel.SetActive(true);

            _timerController.StopTimer();

            _eventsManager.EventName = "onNoGameIsPlaying";

            _outputDataController.SendNoteInformation(
                "Stop sesiune joc. "
                      );

            _outputDataController.PushGameSession(
                new GameSessionOutputData()
                {
                    StartTime = _timerController.StartDateTime,
                    FinishTime = _timerController.FinishDateTime.Value,
                    BodySide = _inputData.BodySide.ToString(),
                }
            );
        }
    }
}
