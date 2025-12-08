using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FruitsGame
{
    public class BasketController : MonoBehaviour
    {
        [SerializeField]
        private Transform _basketTransform;

        private InputData _inputData;

        private Vector3 _initialBasketPosition;
        private Vector3 _initialBasketScale;

        public void Init(InputData inputData)
        {
            _inputData = inputData;

            _initialBasketPosition = _basketTransform.position;
            _initialBasketScale = _basketTransform.localScale;

            UpdateBasketData(_inputData);
        }

        public void UpdateBasket(InputData inputData)
        {
            _inputData = inputData;

            UpdateBasketData(_inputData);
        }

        public void StartGame()
        {
            if (_inputData.GameType == GameType.GAME_TYPE_BASKET) {
                _basketTransform.gameObject.SetActive(true);
            }

            if (_inputData.GameType != GameType.GAME_TYPE_BASKET) {
                _basketTransform.gameObject.SetActive(false);
            }
        }

        //public Vector3 GetBasketPosition()
        //{
        //    return _basketTransform.position;
        //}

        private void UpdateBasketData(InputData inputData)
        {
            SetBasketScale(inputData);
            SetBasketHeight(inputData);
        }

        private void SetBasketScale(InputData inputData)
        {
            List<Transform> children = new List<Transform>();

            foreach(Transform child in _basketTransform) {
                if (child.gameObject.name != "Cylinder.001" &&
                    child.gameObject.name != "Basket") {
                    children.Add(child);
                }
            }

            foreach(Transform child in children) {
                child.SetParent(null);
            }

            float basketScale = inputData.BasketScale / 100.0f;
            _basketTransform.localScale = new Vector3(
                _initialBasketScale.x * basketScale,
                _initialBasketScale.y * basketScale,
                _initialBasketScale.z
            );

            foreach(Transform child in children) {
                child.SetParent(_basketTransform, true);
            }
        }

        private void SetBasketHeight(InputData inputData)
        {
            float basketOffsetY = inputData.BasketOffsetY / 100.0f;
            _basketTransform.position = _initialBasketPosition +
                new Vector3(0, basketOffsetY, 0);
        }
    }
}
