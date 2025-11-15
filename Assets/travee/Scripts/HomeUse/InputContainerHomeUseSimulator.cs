using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InputContainerHomeUseSimulator : MonoBehaviour
{
#if (UNITY_EDITOR)
    private void Start()
    {
        GameObject eyePointerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/EyePointer.prefab", typeof(Object));

        var centerEye = GameObject.Find("CenterEyeAnchor");

        if (centerEye == null)
        {
            return;
        }

        var eyePointerGO = Instantiate(eyePointerPrefab, centerEye.transform);

        var eyePointerController = eyePointerGO.GetComponent<EyePointerController>();

        eyePointerController.Init();
    }
#endif

}
