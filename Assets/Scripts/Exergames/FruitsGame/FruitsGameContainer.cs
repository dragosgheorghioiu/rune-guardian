using System.Collections.Generic;
using UnityEngine;

namespace FruitsGame
{
    [System.Serializable]
    public class HandMarkersContainer
    {
        public List<Transform> TipMarkers;
    }

    public class FruitsGameContainer : MonoBehaviour
    {
        [SerializeField]
        public HandMarkersContainer LeftHandMarkersContainer;
        [SerializeField]
        public HandMarkersContainer RightHandMarkersContainer;

        [SerializeField]
        public Transform LeftHandPalmTransform;
        [SerializeField]
        public Transform RightHandPalmTransform;

        [SerializeField]
        public float DetectionOffset;

        private static FruitsGameContainer _instance;

        public static FruitsGameContainer Instance
        {
            get
            {
                return _instance;
            }
        }

        private void Awake()
        {
            _instance = this;
        }
    }
}
