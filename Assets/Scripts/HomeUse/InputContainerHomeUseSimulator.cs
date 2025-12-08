using UnityEngine;

public class InputContainerHomeUseSimulator : MonoBehaviour
{
#if (UNITY_EDITOR)
    [SerializeField]
    private GameObject eyePointerPrefab;

    private void Start()
    {
        var centerEye = GameObject.Find("CenterEyeAnchor");

        if (centerEye == null)
        {
            return;
        }

        if (eyePointerPrefab == null)
        {
            Debug.LogWarning("EyePointer prefab is not assigned in the Inspector. Skipping instantiation.");
            return;
        }

        var eyePointerGO = Instantiate(eyePointerPrefab, centerEye.transform);

        if (eyePointerGO.TryGetComponent<EyePointerController>(out var eyePointerController))
        {
            eyePointerController.Init();
        }
    }
#endif

}
