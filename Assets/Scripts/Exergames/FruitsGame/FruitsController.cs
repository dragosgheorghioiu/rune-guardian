using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FruitsGame
{
    public class FruitsController : MonoBehaviour
    {
        [SerializeField]
        private FruitsDistributionController _fruitsDistributionController;
        [SerializeField]
        private FruitsGatherController _fruitsGatherController;

        private InputData _inputData;

        public int LoadedFruitCount
        {
            get { return _fruitsGatherController.LoadedFruitCount; }
        }

        public void Init(InputData inputData,
            UnityAction onAllFruitsGatheredAction,
            UnityAction onStartVibrations)
        {
            _inputData = inputData;

            _fruitsGatherController.Init(_inputData, onAllFruitsGatheredAction, onStartVibrations);
        }

        public void UpdateGame(InputData inputData)
        {
            _inputData = inputData;

            _fruitsGatherController.UpdateGame(_inputData);
        }

        public void StartGame()
        {
            _fruitsGatherController.StartGame();

            _fruitsDistributionController.DestroyFruitInstances();

            _fruitsDistributionController.DistributeFruits(_inputData);
        }

        public void DestroyFruitInstances()
        {
            _fruitsDistributionController.DestroyFruitInstances();
        }
    }
}
