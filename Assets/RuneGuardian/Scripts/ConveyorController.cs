using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ConveyorController : MonoBehaviour
{
    [Header("Material / Shader param")]
    [SerializeField] private Renderer conveyorRenderer;
    [SerializeField] private string uvOffsetProperty = "UVOffset";
    [SerializeField] private float runningSpeed = -1f;

    private MaterialPropertyBlock mpb;
    private int uvOffsetId;

    private float uvOffset;
    private bool isRunning = false;

    private void Awake()
    {
        SpawnedToy.onToyDespawn += StopConveyor;
        if (conveyorRenderer == null)
            conveyorRenderer = GetComponentInChildren<Renderer>();

        mpb = new MaterialPropertyBlock();
        uvOffsetId = Shader.PropertyToID(uvOffsetProperty);

        ApplyOffset();
    }

    private void Update()
    {
        if (!isRunning) return;

        uvOffset += runningSpeed * Time.deltaTime;
        if (uvOffset > 10000f) uvOffset -= 10000f;

        ApplyOffset();
    }

    public void StartConveyor() => isRunning = true;
    public void StopConveyor() => isRunning = false;

    public void Reverse() => runningSpeed *= -1;

    public void SetRunningSpeed(float s) => runningSpeed = Mathf.Max(0f, s);

    private void ApplyOffset()
    {
        if (conveyorRenderer == null) return;
        Debug.Log(uvOffset);
        conveyorRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat(uvOffsetId, uvOffset);
        conveyorRenderer.SetPropertyBlock(mpb);
    }
}
