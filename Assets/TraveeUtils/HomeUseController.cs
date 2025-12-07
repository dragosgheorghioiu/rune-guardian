using FruitsGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGuardian
{
    public class HomeUseController : GameManagerBase
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
            Debug.Log("BodySide: " + _inputData.BodySide);
            Debug.Log("GameType: " + _inputData.GameType);
            Debug.Log("EnemyCount: " + _inputData.EnemyCount);
            Debug.Log("Haptic: " + _inputData.Haptic);
            Debug.Log("RoundDuration: " + _inputData.RoundDuration);

            StartIteration();
        }

        protected void StartIteration()
        {
            _finishGamePanel.SetActive(false);

            // Game logic
            Debug.Log("StartIteration - BodySide: " + _inputData.BodySide);
            Debug.Log("StartIteration - EnemyCount: " + _inputData.EnemyCount);
            Debug.Log("StartIteration - Haptic: " + _inputData.Haptic);

            _timerController.StartTimer();
        }

        protected void StopGame()
        {
            OnGameFinished();

            ExitGame();
        }

        protected void ExitGame()
        {
            SessionContainer.OnStop();
        }

        protected void OnTimeFinished ()
        {
            OnGameFinished();
        }

        protected void OnGameFinished()
        {
            // Game logic
            // ...

            _finishGamePanel.SetActive(true);

            _timerController.StopTimer();
            Debug.Log("OnGameFinished - BodySide: " + _inputData.BodySide);
            Debug.Log("OnGameFinished - EnemyCount: " + _inputData.EnemyCount);

            _outputDataController.SendNoteInformation(
                "Stop sesiune joc. "
                + "BodySide: " + _inputData.BodySide + ". "
                + "GameType: " + _inputData.GameType + ". "
                + "EnemyCount: " + _inputData.EnemyCount + ". "
                + "RoundDuration: " + _inputData.RoundDuration + "s."
            );

            _outputDataController.PushGameSession(
                new GameSessionOutputData() {
                    StartTime = _timerController.StartDateTime,
                    FinishTime = _timerController.FinishDateTime.Value,
                    BodySide = _inputData.BodySide.ToString(),
                }
            );
        }

        /// <summary>
        /// Helper method!
        /// </summary>
        private SessionContainer SessionContainer
        {
            get {
                return GameObject.Find("SessionContainer").GetComponent<SessionContainer>();
            }
        }
    }

}
