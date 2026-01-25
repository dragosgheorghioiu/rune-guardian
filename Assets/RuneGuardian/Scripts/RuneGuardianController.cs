using System;
using UnityEngine;

namespace RuneGuardian
{
    public class RuneGuardianController
    {
        public static Action onRuneGuardianStart;
        public static Action<InputData> OnRuneGuardianInit;
        private InputData _inputData;

        public RuneGuardianController(InputData inputData)
        {
            _inputData = inputData;

            // Get OVRCameraRig from scene root and set transform
            OVRCameraRig ovrCameraRig = UnityEngine.Object.FindObjectOfType<OVRCameraRig>();
            if (ovrCameraRig != null)
            {
                ovrCameraRig.transform.position = inputData.gameMode == GameMode.CONVEYOR_BELT
                ? new Vector3(0.7f, 1.0f, -1.0f)
                : new Vector3(0.0f, 1.0f, -6.0f);
            }

            OnRuneGuardianInit?.Invoke(inputData);
        }

        public void UpdateGame(InputData inputData)
        {
            _inputData = inputData;

            Debug.Log(inputData);
        }

        public void StartGame()
        {
        }

        public void StopGame()
        {
        }

        public void DestroyEnemyInstances()
        {
        }
    }
}