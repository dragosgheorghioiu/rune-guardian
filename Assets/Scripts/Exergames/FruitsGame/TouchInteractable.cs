using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;

namespace FruitsGame
{
    public class TouchInteractable : MonoBehaviour
    {
        [SerializeField]
        public Collider InteractableCollider;

        private float _interactableColliderRadius;
        private bool _locked;
        private Transform _lockedTransform;
        private bool _needFall;
        private bool _placed;

        //[SerializeField]
        private float _fallAcceleration = 0.5f;
        private float _fallSpeed = 0.0f;
        private Vector3 _fallTargetPosition;

        private static bool _lockedFruit;

        private InputData _inputData;
        private BodySide _bodySide;
        private Transform _basketTransform;
        private FruitsGatherController _fruitsGatherController;
        private Transform _fallTargetTransform;

        protected UnityEvent _onStartVibration;

        // Start is called before the first frame update
        public void Init(InputData inputData, BodySide fruitBodySide,
            Transform basketTransform, FruitsGatherController fruitsGatherController,
            Transform fallTargetTransform, UnityAction onStartVibrationAction)
        {
            _inputData = inputData;
            _bodySide = fruitBodySide;
            _basketTransform = basketTransform;
            _fruitsGatherController = fruitsGatherController;
            _fallTargetTransform = fallTargetTransform;

            _interactableColliderRadius = InteractableCollider.bounds.extents.magnitude;

            //_initialBasketScale = _basketTransform.localScale;

            _lockedFruit = false;

            _locked = false;
            _needFall = false;
            _placed = false;

            _onStartVibration = new UnityEvent();
            if (onStartVibrationAction != null) {
                _onStartVibration.AddListener(onStartVibrationAction);
            }
        }

        // Update is called once per frame
        void Update()
        {
            //if (!_locked) {

            //    if (_lockedFruit) {
            //        return;
            //    }

            //    var markers = new List<Transform>();
            //    markers.AddRange(FruitsGameContainer.Instance.LeftHandMarkersContainer.TipMarkers);
            //    markers.AddRange(FruitsGameContainer.Instance.RightHandMarkersContainer.TipMarkers);

            //    foreach (var tipMarker in markers) {
            //        var position = InteractableCollider.ClosestPoint(tipMarker.position);

            //        if (position == tipMarker.position) {
            //            _locked = true;
            //            _lockedFruit = true;
            //            //_lockedTransform = tipMarker.transform;
            //            transform.SetParent(tipMarker.transform);

            //            break;
            //        }
            //    }

            //    return;
            //}

            if (!_locked) {
                return;
            }

            Vector3 basketPosition = _basketTransform.position;

            float moveSpeed = 0.5f;
            float rotationSpeed = 360.0f;

            if (!_needFall) {
                if (transform.localPosition.magnitude != 0) { 
                    var moveDirection = -transform.localPosition;
                    var moveSize = moveDirection.normalized * moveSpeed * Time.deltaTime;

                    if (moveSize.magnitude > moveDirection.magnitude) {
                        transform.localPosition = new Vector3(0, 0, 0);
                    }
                
                    if (moveSize.magnitude <= moveDirection.magnitude) {
                        transform.localPosition += moveDirection.normalized * moveSpeed * Time.deltaTime;
                    }
                }

                if (Quaternion.Angle (transform.localRotation, Quaternion.identity) > 1.0f) {
                    var rotation = Quaternion.RotateTowards(
                        transform.localRotation, Quaternion.identity,
                        rotationSpeed * Time.deltaTime
                    );

                    transform.localRotation = rotation;
                }

                if (_inputData.GameType != GameType.GAME_TYPE_BASKET) {
                    return;
                }

                //var projFruitPosition = new Vector3 (transform.position.x, 0, transform.position.z);
                //var projBasketPosition = new Vector3(basketPosition.x, 0, basketPosition.z);

                //Vector3 basketScaleV = FruitsGameContainer.Instance.BasketTransform.localScale;
                //float basketScale = basketScaleV.x / _initialBasketScale.x;

                //if (Vector3.Distance (projFruitPosition, projBasketPosition) <
                //    FruitsGameContainer.Instance.DetectionOffset * basketScale) {
                //    transform.SetParent(null);
                //    _needFall = true;

                //    _fallSpeed = _fallAcceleration;

                //    _lockedFruit = false;
                //}

                return;
            }

            if (!_placed) {

                _fallSpeed += Time.deltaTime * _fallAcceleration;
                var fallDirection = Vector3.Normalize(
                    _fallTargetPosition - transform.position
                );
                transform.position += fallDirection * _fallSpeed * Time.deltaTime;

                if (Vector3.Distance(_fallTargetPosition, transform.position) < 0.01f) {
                    //var a = transform.position;
                    transform.position = _fallTargetPosition;
                        //new Vector3(a.x, basketPosition.y - 0.1f, a.z);
                    _placed = true;

                    transform.SetParent(_basketTransform, true);

                    _fruitsGatherController.OnFruitLoaded();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_locked == true) {
                if (_inputData.GameType == GameType.GAME_TYPE_BASKET) {
                    if (other.gameObject.name == "Basket") {
                        transform.SetParent(null);
                        _needFall = true;

                        _fallSpeed = _fallAcceleration;

                        Vector2 fallTarget2D = Random.insideUnitCircle * 0.05f
                            * _inputData.BasketScale / 100.0f;
                        _fallTargetPosition = new Vector3(
                            _fallTargetTransform.position.x + fallTarget2D.x,
                            _fallTargetTransform.position.y,
                            _fallTargetTransform.position.z + fallTarget2D.y
                        );

                        _lockedFruit = false;
                    }
                }

                if (_inputData.GameType == GameType.GAME_TYPE_MOUTH) {
                    if (other.gameObject.name == "CenterEyeAnchor") {
                        gameObject.SetActive(false);

                        _needFall = true;
                        _placed = true;
                        _lockedFruit = false;

                        _fruitsGatherController.OnFruitLoaded();
                    }
                }
            }

            if (_locked == true || _lockedFruit == true) {
                return;
            }

            string correctHandGOName = _bodySide == BodySide.BODY_SIDE_LEFT ?
                    "OVRLeftHandVisual" : "OVRRightHandVisual";
            string parentName = other.gameObject.name.Contains("CapsuleCollider") ?
                other.transform.parent.parent.parent.name : "";

            if (parentName == correctHandGOName) {
                _locked = true;
                _lockedFruit = true;

                if (_bodySide == BodySide.BODY_SIDE_LEFT) {
                    transform.SetParent(FruitsGameContainer.Instance.LeftHandPalmTransform);
                }
                if (_bodySide == BodySide.BODY_SIDE_RIGHT) {
                    transform.SetParent(FruitsGameContainer.Instance.RightHandPalmTransform);
                }

                //
                if (_inputData.Haptic == true) {
                    _onStartVibration.Invoke();
                }
            }
        }
    }
}
