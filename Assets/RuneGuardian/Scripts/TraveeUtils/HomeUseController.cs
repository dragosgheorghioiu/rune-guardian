using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGuardian
{
    public class HomeUseController : GameManagerBase
    {
        private RuneGuardianController _runeGuardianController;
        [SerializeField]
        private FinishExercisePanelsController _finishExercisePanelsController;

        private InputData _inputData;

        private void Start()
        {
            // Find RuneGuardianController in the scene
            if (_runeGuardianController == null)
            {
                _runeGuardianController = FindObjectOfType<RuneGuardianController>();
                if (_runeGuardianController == null)
                {
                    Debug.LogError("RuneGuardianController not found in the scene!");
                }
            }
            // Validate that FinishExercisePanelsController is assigned
            if (_finishExercisePanelsController == null)
            {
                Debug.LogError("FinishExercisePanelsController is not assigned in the Inspector!");
            }
        }

        public int WavesCompleted
        {
            get { return _runeGuardianController.WavesCompleted; }
        }

        public void Init(InputData inputData)
        {
            _inputData = inputData;

            _runeGuardianController.Init(_inputData, OnAllWavesCompleted, OnStartVibration);

            StartIteration();
        }

        public void UpdateGame(InputData inputData)
        {
            _inputData = inputData;

            _runeGuardianController.UpdateGame(_inputData);
        }

        protected void StartIteration()
        {
            _finishExercisePanelsController.Hide(FinishExercisePanelType.PANEL_TYPE_HOME_USE);

            _runeGuardianController.StartGame();

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

        protected void OnAllWavesCompleted()
        {
            SessionContainer.OnIterationCompleted();

            OnGameFinished();
        }

        protected void OnTimeFinished()
        {
            OnGameFinished();
        }

        private void OnGameFinished()
        {
            _finishExercisePanelsController.Show(FinishExercisePanelType.PANEL_TYPE_HOME_USE);

            _timerController.StopTimer();

            _outputDataController.PushGameSession(
                new GameSessionOutputData()
                {
                    StartTime = _timerController.StartDateTime,
                    FinishTime = _timerController.FinishDateTime.Value,
                    BodySide = _inputData.BodySide.ToString(),
                    WavesCompleted = _runeGuardianController.WavesCompleted
                }
            );
        }

        private SessionContainer SessionContainer
        {
            get
            {
                return GameObject.Find("SessionContainer").GetComponent<SessionContainer>();
            }
        }

        protected void OnStartVibration()
        {
            SendHDCMessage("START_VIBRATION");
        }
    }
}
