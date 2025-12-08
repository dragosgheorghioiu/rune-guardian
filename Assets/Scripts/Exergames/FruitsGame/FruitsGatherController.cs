using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FruitsGame
{
    public class FruitsGatherController : MonoBehaviour
    {
        [SerializeField]
        private FruitsDistributionController _fruitsDistributionController;
        [SerializeField]
        private ScoreController _scoreController;
        [SerializeField]
        private AudioSource _fruitLoadSound;

        protected InputData _inputData;
        protected UnityEvent _onAllFruitsGathered;

        private int _loadedFruitsCount;

        public int LoadedFruitCount
        {
            get { return _loadedFruitsCount; }
        }

        public void Init(InputData inputData,
            UnityAction onAllFruitsGatheredAction,
            UnityAction onStartVibration)
        {
            _inputData = inputData;

            _onAllFruitsGathered = new UnityEvent();
            _onAllFruitsGathered.AddListener (onAllFruitsGatheredAction);

            _fruitsDistributionController.Init(onStartVibration);
        }
        
        public void UpdateGame(InputData inputData)
        {
            _inputData = inputData;
        }

        public void StartGame()
        {
            _loadedFruitsCount = 0;
        }

        public void OnFruitLoaded()
        {
            if (_loadedFruitsCount + _inputData.NrFructe < _inputData.NrTotalFructe) {
                _fruitsDistributionController.DistributeFruit(_inputData);
            }

            _loadedFruitsCount++;

            _scoreController.AddScore(100);

            _fruitLoadSound.Play();

            if (_loadedFruitsCount == _inputData.NrTotalFructe) {
                _onAllFruitsGathered.Invoke();
            }
        }
    }
}
