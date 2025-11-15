using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public enum ExergameUseType
{
    ClinicalUse = 0,
    HomeUse = 1
}

[ExecuteInEditMode]
public class InputDataContainer : MonoBehaviour
{
    [SerializeField]
    [TextArea]
    private string InputData;

    [SerializeField]
    [TextArea]
    private string Parameters;

    [SerializeField]
    private Language Language;

    [SerializeField]
    protected ExergameUseType UseType;

#if (UNITY_EDITOR)
    private void Start()
    {
        if (UseType == ExergameUseType.HomeUse) {
            var check = GetComponent<InputContainerHomeUseSimulator>();

            if (check == null) {
                this.gameObject.AddComponent<InputContainerHomeUseSimulator>();
            }
        }
    }
#endif

}
