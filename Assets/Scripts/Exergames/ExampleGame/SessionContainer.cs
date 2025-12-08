using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExampleGame
{
    /// <summary>
    /// An example of input data.
    /// Only int, bool and list data types are allowed at this time.
    /// </summary>
    public class InputData
    {
        /// <summary>
        /// Example int.
        /// </summary>
        public int ExampleInt;

        /// <summary>
        /// Example bool.
        /// </summary>
        public bool ExampleBool;

        /// <summary>
        /// Example list.
        /// This attribute only stores the index of the selected option from the list.
        /// </summary>
        public int ExampleList;

        /// <summary>
        /// Default attribute. It is optional.
        /// </summary>
        public int duration;

        /// <summary>
        /// Default attribute. Represents a list of options for selecting the hand
        /// with which the exercise is performed.
        /// </summary>
        public BodySide BodySide;
    }

    /// <summary>
    /// An example of output data.
    /// Only int and bool data types are allowed at this time.
    /// </summary>
    [Serializable]
    public class GameSessionOutputData : GameSessionOutputDataBase
    {
        public int ExampleInt;
        public bool ExampleBool;
        public List<string> ExampleList;
    }

    /// <summary>
    /// An example of output data.
    /// </summary>
    public class OutputData
    {
        /// <summary>
        /// Example int.
        /// </summary>
        public int ExampleInt;
    }

    public class SessionContainer : SessionContainerBase
    {
        /// <summary>
        /// Object that contains the input parameter values for the game.
        /// </summary>
        [SerializeField]
        protected InputData _inputData;

        /// <summary>
        /// Object assigned with controlling game behavior in a clinical context.
        /// </summary>
        [SerializeField]
        protected ClinicalUseController _clinicalUseController;

        /// <summary>
        /// Object assigned with controlling game behavior in a home use context.
        /// </summary>
        [SerializeField]
        protected HomeUseController _homeUseController;

        /// <summary>
        /// Internal object used to simplify the management of the context
        /// for which the game is intended.
        /// </summary>
        protected GameManagerBase _mainController;

        /// <summary>
        /// Event called when the main VR application notifies the exergame to start.
        /// </summary>
        /// <param name="json">The input data of the exergame in JSON format.</param>
        public override void OnInit(string json)
        {
            _inputData = JsonUtility.FromJson<InputData>(json);

            _clinicalUseController.Init(_inputData);

            _mainController = _clinicalUseController;
        }

        /// <summary>
        /// Event called when the main VR application notifies the exergame to start
        /// in a home use context.
        /// </summary>
        /// <param name="json">The input data of the exergame in JSON format.</param>
        public override void OnInitHomeUse(string json)
        {
            _inputData = JsonUtility.FromJson<InputData>(json);

            _homeUseController.Init(_inputData);

            _mainController = _homeUseController;
        }

        /// <summary>
        /// Event called when the main VR application notifies the exergame to update.
        /// </summary>
        /// <param name="json">The input data of the exergame in JSON format.</param>
        public override void OnUpdate(string json)
        {
            _inputData = JsonUtility.FromJson<InputData>(json);

            _clinicalUseController.UpdateGame(_inputData);
        }

        /// <summary>
        /// Event called when the main VR application notifies the exergame to pause.
        /// </summary>
        public override void OnPause(bool pause)
        {
            // Optional
            // Game logic
        }

        /// <summary>
        /// Event called when the main VR application notifies the
        /// exergame to start a custom event.
        /// </summary>
        /// <param name="eventName">The name of the custom event.</param>
        public override void OnCustomEvent(string eventName)
        {
            if (eventName == "startgame") {
                _clinicalUseController.StartGame();
            }

            if (eventName == "stopgame") {
                _clinicalUseController.StopGame();
            }
        }

        /// <summary>
        /// Event called when the main VR application notifies the exergame to stop.
        /// </summary>
        public override void OnStop()
        {
            if (_onStop == null) {
                return;
            }

            var outputData = _mainController.OutputData;

            string output = JsonConvert.SerializeObject(outputData);

            _onStop(output);
        }
    }
}

