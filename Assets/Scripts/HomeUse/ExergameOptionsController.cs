using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExergameOptionsController : MonoBehaviour
{
    [SerializeField]
    protected LayoutElement _scrollViewLayout;
    [SerializeField]
    private GameObject _contentContainer;
    [SerializeField]
    private GameObject _exergameOptionPrefab;

    protected OnExergameSelected _onExergameSelected;

    protected List<ExergameOptionController> _exergameOptionsControllers;

    public ExergameOptionsController()
    {
        _exergameOptionsControllers = new List<ExergameOptionController>();
    }

    public void Init(List<PrescribedExercise> prescribedExercises,
        OnExergameSelected onExergameSelected)
    {
        _onExergameSelected = onExergameSelected;

        CreateExergameOptions(prescribedExercises);
        SetScrollViewHeight(prescribedExercises);
    }

    public void UpdateOptions()
    {
        foreach(var optionController in _exergameOptionsControllers) {
            optionController.UpdateOption();
        }
    }

    protected void CreateExergameOptions(List<PrescribedExercise> prescribedExercises)
    {
        ClearExergameOptions();

        foreach (var prescribedExercise in prescribedExercises) {
            var exergameOptionGO = Instantiate(
                _exergameOptionPrefab, _contentContainer.transform
            );

            var exergameOptionController = exergameOptionGO.GetComponent<ExergameOptionController>();

            exergameOptionController.Init(prescribedExercise, _onExergameSelected);

            _exergameOptionsControllers.Add(exergameOptionController);
        }
    }

    protected void SetScrollViewHeight(List<PrescribedExercise> prescribedExercises)
    {
        var elementLayout = _exergameOptionPrefab.GetComponent<LayoutElement>();
        float elementHeight = elementLayout.preferredHeight;

        float scrollViewHeight = elementHeight * prescribedExercises.Count + 90;
        _scrollViewLayout.preferredHeight = scrollViewHeight;
    }

    protected void ClearExergameOptions()
    {
        foreach (Transform transform in _contentContainer.transform) {
            Destroy(transform.gameObject);
        }
    }
}
