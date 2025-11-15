using Oculus.Platform.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FruitsGame
{
    public class FruitsDistributionController : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> _fruitPrefabs;
        [SerializeField]
        private float _distributionWidth;
        [SerializeField]
        private float _distributionHeight;
        [SerializeField]
        private Transform _fruitsContainer;
        [SerializeField]
        private Transform _basketTransform;
        [SerializeField]
        private FruitsGatherController _fruitsGatherController;
        [SerializeField]
        private Transform _fallTargetTransform;

        private List<GameObject> _fruitInstances;

        protected UnityAction _onStartVibration;

        FruitsDistributionController()
        {
            _fruitInstances = new List<GameObject>();
        }


        public void Init(UnityAction onStartVibration)
        {
            _onStartVibration = onStartVibration;
        }

        public void DistributeFruits (InputData inputData)
        {
            for (int i= 0; i < inputData.NrFructe; i++) {
                DistributeFruit(inputData);
            }
        }

        public void DistributeFruit(InputData inputData)
        {
            var fruitPrefab = GetRandomFruitPrefab();

            BodySide fruitBodySide;

            var generationSpaceFruitPosition =
                GetRandonFruitPosition(inputData, fruitPrefab, out fruitBodySide);
            var fruitPosition = generationSpaceFruitPosition
                + new Vector3(0, _basketTransform.position.y, 0);

            var fruitRotation = Random.rotation;
            var fruitScale = Random.Range(0.8f, 1.2f) * 0.25f;

            var instantiatedFruitGO = GameObject.Instantiate(fruitPrefab);
            instantiatedFruitGO.transform.position = fruitPosition;
            instantiatedFruitGO.transform.rotation = fruitRotation;
            instantiatedFruitGO.transform.localScale = new Vector3(fruitScale, fruitScale, fruitScale);

            instantiatedFruitGO.transform.SetParent(_fruitsContainer, true);

            instantiatedFruitGO.GetComponent<TouchInteractable>().Init(
                inputData, fruitBodySide, _basketTransform,
                _fruitsGatherController, _fallTargetTransform,
                _onStartVibration
            );

            _fruitInstances.Add(instantiatedFruitGO);
        }

        public void DestroyFruitInstances()
        {
            foreach(var fruitInstance in _fruitInstances) {
                Destroy(fruitInstance);
            }

            _fruitInstances.Clear();
        }

        private GameObject GetRandomFruitPrefab ()
        {
            int fruitPrefabIndex = Random.Range(0, _fruitPrefabs.Count);

            return _fruitPrefabs[fruitPrefabIndex];
        }

        private Vector3 GetRandonFruitPosition (InputData inputData,
            GameObject fruitPrefab, out BodySide fruitBodySide)
        {
            var touchInteractable = fruitPrefab.GetComponent<TouchInteractable>();
            var fruitPrefabBBoxRadius = touchInteractable.InteractableCollider.bounds.extents.magnitude;

            while (true) {

                var fruitPrefabPosition = ComputeRandomFruitPosition(inputData, out fruitBodySide);

                bool validPosition = true;

                foreach (var instantiatedFruitPrefab in _fruitInstances) {
                    var instantiatedTouchInteractable = instantiatedFruitPrefab.GetComponent<TouchInteractable>();
                    var iFruitPrefabBBoxRadius = instantiatedTouchInteractable.InteractableCollider.bounds.extents.magnitude;

                    var iFruitPrefabPosition = instantiatedFruitPrefab.transform.position;

                    if (Vector3.Distance(fruitPrefabPosition, iFruitPrefabPosition) <
                        fruitPrefabBBoxRadius + iFruitPrefabBBoxRadius) {

                        validPosition = false;
                        break;
                    }
                }

                if (validPosition) {
                    return fruitPrefabPosition;
                }
            }

            return new Vector3(); //...
        }

        private Vector3 ComputeRandomFruitPosition (InputData inputData,
            out BodySide fruitBodySide)
        {
            //var z = Random.Range(-0.25f, 0.25f);
            var depth = Random.Range(
                inputData.MinDistancePlacement / 100.0f,
                inputData.MaxDistancePlacement / 100.0f
            );

            var abductionAngle = Random.Range(inputData.MinAngleAbduction, inputData.MaxAngleAbduction);

            fruitBodySide = BodySide.BODY_SIDE_LEFT;
            if (inputData.BodySide == BodySide.BODY_SIDE_LEFT) {
                abductionAngle += 90;
            }

            if (inputData.BodySide == BodySide.BODY_SIDE_RIGHT) {
                abductionAngle = 90 - abductionAngle;

                fruitBodySide = BodySide.BODY_SIDE_RIGHT;
            }

            if (inputData.BodySide == BodySide.BODY_SIDE_BOTH) {
                int random = Random.Range(0, 2);

                if (random == 0) {
                    abductionAngle += 90;

                    fruitBodySide = BodySide.BODY_SIDE_LEFT;
                }

                if (random == 1) {
                    abductionAngle = 90 - abductionAngle;

                    fruitBodySide = BodySide.BODY_SIDE_RIGHT;
                }
            }

            var dir = Quaternion.Euler(0, abductionAngle, 0) * new Vector3(1, 0, 0);
            dir = (new Vector3 (dir.x, 0, -dir.z).normalized) * depth;

            var distributionMinHeightDegrees = Mathf.Tan(Mathf.Deg2Rad * (inputData.MinAngleFlexion - 90));
            var distributionMaxHeightDegrees = Mathf.Tan(Mathf.Deg2Rad * (inputData.MaxAngleFlexion - 90));

            var distributionMinHeight = 0.5f + distributionMinHeightDegrees * 0.5f;
            var distributionMaxHeight = 0.5f + distributionMaxHeightDegrees * 0.5f;
            var y = Random.Range(distributionMinHeight, distributionMaxHeight);

            return new Vector3(dir.x, y, dir.z);
        }
    }
}
