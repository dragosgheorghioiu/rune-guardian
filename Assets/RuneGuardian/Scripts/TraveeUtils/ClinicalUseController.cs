using System;
using UnityEngine;

namespace RuneGuardian
{
    /// <summary>
    /// Data type used for the object assigned with controlling game
    /// behavior in a clinical context.
    /// </summary>
    public class ClinicalUseController : GameManagerBase
    {

        private RuneGuardianController _runeGuardianController;
        [SerializeField]
        private FinishExercisePanelsController _finishExercisePanelsController;

        /// <summary>
        /// Object that contains the input parameter values for the game.
        /// </summary>
        private InputData _inputData;

        public void StartGame()
        {
            RuneGuardianController.onRuneGuardianStart?.Invoke();
        }

        public void Init(InputData inputData)
        {
            _eventsManager.EventName = "onNoGameIsPlaying";

            _inputData = inputData;
            _runeGuardianController = new RuneGuardianController(_inputData);
        }

        public void UpdateGame(InputData inputData)
        {
            _inputData = inputData;
        }

        public void StopGame()
        {
            OnGameFinished();
        }

        private void OnGameFinished()
        {
            _finishExercisePanelsController.Show(FinishExercisePanelType.PANEL_TYPE_CLINICAL_USE);

            _timerController.StopTimer();

            _eventsManager.EventName = "onNoGameIsPlaying";

            // TODO(dregos): should rewrite this message too
            // _outputDataController.SendNoteInformation(
            //     "Stop sesiune joc. "
            //     + "Inamici învinși: " + _runeGuardianController.EnemiesDefeated + "."
            // );

            _outputDataController.PushGameSession(
                new GameSessionOutputData()
                {
                    StartTime = _timerController.StartDateTime,
                    FinishTime = _timerController.FinishDateTime.Value,
                    // BodySide = _inputData.BodySide.ToString(),
                }
            );
        }

        protected void OnStartVibration()
        {
            SendHDCMessage("START_VIBRATION");
        }
    }
}
