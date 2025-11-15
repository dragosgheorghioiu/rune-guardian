using FruitsGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExampleGame
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
            Debug.Log(_inputData.BodySide);
            Debug.Log(_inputData.ExampleInt);
            Debug.Log(_inputData.ExampleBool);
            Debug.Log(_inputData.ExampleList);

            StartIteration();
        }

        protected void StartIteration()
        {
            _finishGamePanel.SetActive(false);

            // Game logic
            Debug.Log(_inputData.BodySide);
            Debug.Log(_inputData.ExampleInt);
            Debug.Log(_inputData.ExampleBool);
            Debug.Log(_inputData.ExampleList);

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

            _outputDataController.PushGameSession(
                new GameSessionOutputData() {
                    StartTime = _timerController.StartDateTime,
                    FinishTime = _timerController.FinishDateTime.Value,
                    BodySide = _inputData.BodySide.ToString(),
                    ExampleInt = 100 /* Anything relevant to the outcome of the game session. */,
                    ExampleBool = true /* Anything relevant to the outcome of the game session. */,
                    ExampleList = new List<string>() { "Bine", "Foarte bine", "Excelent" } /* Anything relevant to the outcome of the game session. */
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
