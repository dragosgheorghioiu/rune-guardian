using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public delegate void OnExergameSelected(PrescribedExercise prescribedExercise);

public class ExergameOptionController : MonoBehaviour
{
    [SerializeField]
    protected TextMeshProUGUI _textRenderer;
    [SerializeField]
    protected TextMeshProUGUI _iterationsTextRenderer;
    [SerializeField]
    protected Image _panel;
    [SerializeField]
    protected Color _initialColor;
    [SerializeField]
    protected Color _finalColor;

    protected PrescribedExercise _prescribedExercise;
    protected OnExergameSelected _onExergameSelected;

    public void Init(PrescribedExercise prescribedExercise,
        OnExergameSelected onExergameSelected)
    {
        _prescribedExercise = prescribedExercise;
        _onExergameSelected = onExergameSelected;

        _textRenderer.text = prescribedExercise.Activity.Name + " - "
            + prescribedExercise.BodySide;

        UpdateOption();
    }

    public void UpdateOption()
    {
        string prescribedExerciseId = "PE" + _prescribedExercise.Id;
        int currentIterationCount = PlayerPrefs.GetInt(prescribedExerciseId, 0);

        _iterationsTextRenderer.text = currentIterationCount
            + " / " + _prescribedExercise.IterationCount;

        float t = (float)currentIterationCount / _prescribedExercise.IterationCount;
        _panel.color = Color.Lerp(_initialColor, _finalColor, t);
    }

    public void OnSelected()
    {
        _onExergameSelected(_prescribedExercise);
    }
}
