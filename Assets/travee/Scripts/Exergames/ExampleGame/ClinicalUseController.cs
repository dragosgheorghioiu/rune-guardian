using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExampleGame
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
            Debug.Log(_inputData.BodySide);
            Debug.Log(_inputData.ExampleInt);
            Debug.Log(_inputData.ExampleBool);
            Debug.Log(_inputData.ExampleList);

            _eventsManager.EventName = "onNoGameIsPlaying";
        }

        public void UpdateGame(InputData inputData)
        {
            _inputData = inputData;

            // Game logic
            Debug.Log(_inputData.BodySide);
            Debug.Log(_inputData.ExampleInt);
            Debug.Log(_inputData.ExampleBool);
            Debug.Log(_inputData.ExampleList);
        }

        public void StartGame()
        {
            // Game logic
            Debug.Log(_inputData.BodySide);
            Debug.Log(_inputData.ExampleInt);
            Debug.Log(_inputData.ExampleBool);
            Debug.Log(_inputData.ExampleList);

            _timerController.StartTimer();

            _eventsManager.EventName = "onGameIsPlaying";

            _outputDataController.SendNoteInformation(
                "Start sesiune joc."
                + " Body side: " + _inputData.BodySide + "."
                + " ExampleInt: " + _inputData.ExampleInt + "."
                + " ExampleBool: " + _inputData.ExampleBool + "."
                + " ExampleList: " + _inputData.ExampleList + "."
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
                + "Timp execuție: " + _timerController.GameDuration.ToString("m' min 's' sec'") + ". "
                + "ExampleInt: " + 100 + "." /* Anything relevant to the outcome of the game session. */
                + "ExampleBool: " + true + "." /* Anything relevant to the outcome of the game session. */
                + "ExampleList: " + (new List<string>() { "Bine", "Foarte bine", "Excelent" }).ToString() + "." /* Anything relevant to the outcome of the game session. */
            );

            _outputDataController.PushGameSession(
                new GameSessionOutputData()
                {
                    StartTime = _timerController.StartDateTime,
                    FinishTime = _timerController.FinishDateTime.Value,
                    BodySide = _inputData.BodySide.ToString(),
                    ExampleInt = 100 /* Anything relevant to the outcome of the game session. */,
                    ExampleBool = true /* Anything relevant to the outcome of the game session. */,
                    ExampleList = new List<string>() { "Bine", "Foarte bine", "Excelent" } /* Anything relevant to the outcome of the game session. */
                }
            );
        }
    }
}
