using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FruitsGame
{
    public class ClinicalUseController : GameManagerBase
    {
        [SerializeField]
        private FruitsController _fruitsController;
        [SerializeField]
        private BasketController _basketController;
        [SerializeField]
        private TimerController _timeController;
        [SerializeField]
        private ScoreController _scoreController;
        [SerializeField]
        private FinishExercisePanelsController _finishExercisePanelsController;

        private InputData _inputData;

        public bool Pause
        {
            get { return _timeController.Pause; }
            set {
                _timeController.Pause = value;
            }
        }

        public int Score
        {
            get { return _scoreController.Score; }
        }

        public void Init(InputData inputData)
        {
            _inputData = inputData;

            _fruitsController.Init(_inputData, OnAllFruitsGathered, OnStartVibration);
            _basketController.Init(_inputData);
            _timeController.Init(_inputData, OnTimeFinished);
            _scoreController.Init();

            _eventsManager.EventName = "onNoGameIsPlaying";
        }

        public void UpdateGame(InputData inputData)
        {
            _inputData = inputData;

            _fruitsController.UpdateGame(_inputData);
            _basketController.UpdateBasket(_inputData);

            _timeController.UpdateTime(_inputData);
        }

        public void StartGame()
        {
            _finishExercisePanelsController.Hide(FinishExercisePanelType.PANEL_TYPE_CLINICAL_USE);

            _fruitsController.StartGame();
            _basketController.StartGame();

            _scoreController.StartGame();
            _timeController.StartTime();
            _timerController.StartTimer();

            _eventsManager.EventName = "onGameIsPlaying";

            _outputDataController.SendNoteInformation(
                "Start sesiune joc."
                + " Număr total fructe: " + _inputData.NrTotalFructe + "."
                + " Număr fructe vizibile: " + _inputData.NrFructe + "."
                + " Tip joc: " + _inputData.GameType.ToString() + "."
            );
        }

        public void StopGame()
        {
            _timeController.StopTime();

            OnGameFinished();
        }

        protected void OnAllFruitsGathered()
        {
            _timeController.StopTime();

            OnGameFinished();
        }

        protected void OnTimeFinished ()
        {
            OnGameFinished();
        }

        private void OnGameFinished()
        {
            _finishExercisePanelsController.Show(FinishExercisePanelType.PANEL_TYPE_CLINICAL_USE);

            _timerController.StopTimer();

            _eventsManager.EventName = "onNoGameIsPlaying";

            _outputDataController.SendNoteInformation(
                "Stop sesiune joc. "
                + "Timp execuție: " + _timerController.GameDuration.ToString("m' min 's' sec'") + ". "
                + "Număr fructe culese: " + _fruitsController.LoadedFruitCount + "."
            );

            _outputDataController.PushGameSession(
                new GameSessionOutputData() {
                    StartTime = _timerController.StartDateTime,
                    FinishTime = _timerController.FinishDateTime.Value,
                    BodySide = _inputData.BodySide.ToString(),
                    LoadedFruitCount = _fruitsController.LoadedFruitCount
                }
            );
        }

        protected void OnStartVibration()
        {
            SendHDCMessage("START_VIBRATION");
        }
    }
}
