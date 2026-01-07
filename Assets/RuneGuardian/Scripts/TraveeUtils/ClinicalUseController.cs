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

            _eventsManager.EventName = "onNoGameIsPlaying";
        }

        public void UpdateGame(InputData inputData)
        {
            _inputData = inputData;

            Debug.Log("ClinicalUseController UpdateGame called");

            _runeGuardianController.UpdateGame(_inputData);
        }

        public void StartGame()
        {
            Debug.Log("ClinicalUseController StartGame called - hiding finish panel");

            _finishExercisePanelsController.Hide(FinishExercisePanelType.PANEL_TYPE_CLINICAL_USE);

            _runeGuardianController.StartGame();

            _timerController.StartTimer();

            _eventsManager.EventName = "onGameIsPlaying";

            Debug.Log("ClinicalUseController StartGame called");

            _outputDataController.SendNoteInformation(
                "Start sesiune joc."
                + " Dificultate: " + _inputData.GameType.ToString() + "."
                + " Număr inamici pe val: " + _inputData.EnemyCount + "."
                + " Număr valuri: " + _inputData.MaxWaves + "."
                + " Rază de regenerare vieață: " + (_inputData.EnableHealthRegen ? "Da" : "Nu") + "."
                + " Haptic: " + _inputData.Haptic + "."
            );
        }

        public void StopGame()
        {
            OnGameFinished();
        }

        protected void OnAllWavesCompleted()
        {
            OnGameFinished();
        }

        protected void OnTimeFinished()
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
                + "Valuri completate: " + _runeGuardianController.WavesCompleted + ". "
                + "Inamici învinși: " + _runeGuardianController.EnemiesDefeated + "."
            );

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

        protected void OnStartVibration()
        {
            SendHDCMessage("START_VIBRATION");
        }
    }
}
