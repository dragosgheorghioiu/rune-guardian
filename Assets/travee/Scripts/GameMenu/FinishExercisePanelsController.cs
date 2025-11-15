using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum FinishExercisePanelType
{
    //PANEL_TYPE_NONE,
    PANEL_TYPE_CLINICAL_USE,
    PANEL_TYPE_HOME_USE,
    //PANEL_TYPE_ALL
}

public class FinishExercisePanelsController : MonoBehaviour
{
    [SerializeField]
    protected GameObject _finishExercisePanelClinicalUse;
    [SerializeField]
    protected GameObject _finishExercisePanelHomeUse;

    [SerializeField]
    protected TextMeshProUGUI _finishExercisePanelClinicalUseTextRenderer;
    [SerializeField]
    protected TextMeshProUGUI _finishExercisePanelHomeUseTextRenderer;

    [SerializeField]
    protected List<Camera> _cameras;

    protected void Awake()
    {
        _finishExercisePanelClinicalUse.SetActive(false);
        _finishExercisePanelHomeUse.SetActive(false);
    }

    public void Show(FinishExercisePanelType panelType)
    {
        if (panelType == FinishExercisePanelType.PANEL_TYPE_CLINICAL_USE) {
            ShowFinishExercisePanelClinicalUse();
        }

        if (panelType == FinishExercisePanelType.PANEL_TYPE_HOME_USE) {
            ShowFinishExercisePanelHomeUse();
        }
    }

    public void Hide(FinishExercisePanelType panelType)
    {
        if (panelType == FinishExercisePanelType.PANEL_TYPE_CLINICAL_USE) {
            _finishExercisePanelClinicalUse.SetActive(false);
        }

        if (panelType == FinishExercisePanelType.PANEL_TYPE_HOME_USE) {
            _finishExercisePanelHomeUse.SetActive(false);
        }
    }

    public void SetText(FinishExercisePanelType panelType, string text)
    {
        if (panelType == FinishExercisePanelType.PANEL_TYPE_CLINICAL_USE) {
            _finishExercisePanelClinicalUseTextRenderer.text = text;
        }

        if (panelType == FinishExercisePanelType.PANEL_TYPE_HOME_USE) {
            _finishExercisePanelHomeUseTextRenderer.text = text;
        }
    }

    protected void ShowFinishExercisePanelClinicalUse()
    {
        _finishExercisePanelClinicalUse.SetActive(true);

        foreach (Camera camera in _cameras) {
            camera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
        }
    }

    protected void ShowFinishExercisePanelHomeUse()
    {
        _finishExercisePanelHomeUse.SetActive(true);

        foreach (Camera camera in _cameras) {
            camera.cullingMask |= (1 << LayerMask.NameToLayer("UI"));
        }
    }
}
