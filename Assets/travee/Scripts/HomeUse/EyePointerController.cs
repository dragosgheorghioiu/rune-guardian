using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class EyePointerController : MonoBehaviour
{
    protected bool _initialized;

    protected Material _material;

    protected Transform _parent;
    protected GameObject _lastGazeInteractableGO;

    protected float _gazeProgressElapsedTime;
    [SerializeField]
    protected float _gazeProgressTotalTime;
    protected GameObject _alreadyInteractedGO;
    [SerializeField]
    protected float _gazeProgressWaitTime;

    public EyePointerController()
    {
        _initialized = false;
    }

    public void Init()
    {
        _material = GetComponent<MeshRenderer>().material;

        _parent = transform.parent;
        _lastGazeInteractableGO = null;

        _alreadyInteractedGO = null;

        _initialized = true;
    }

    private void Update()
    {
        if (_initialized == false) {
            return;
        }

        Ray ray = new Ray(_parent.position, _parent.forward);

        List<RaycastHit> hitsInfo = Physics.RaycastAll(ray).ToList();

        if (hitsInfo.Count == 0) {
            ResetTimers();

            return;
        }

        bool anyObject = hitsInfo.Any(
            hi => hi.collider.gameObject.GetComponent<GazeInteractable>() != null
        );
        if (anyObject == false) {
            ResetTimers();

            return;
        }

        var hitInfo = hitsInfo.FirstOrDefault(
            hi => hi.collider.gameObject.TryGetComponent(out GazeInteractable component)
        );
        var go = hitInfo.collider.gameObject;

        var gazeInteractable = go.GetComponent<GazeInteractable>();

        if (gazeInteractable == null) {
            ResetTimers();

            return;
        }

        OnOver(go, hitInfo);

        if (go != _lastGazeInteractableGO) {
            OnUnhover(_lastGazeInteractableGO);
            OnHover(go);

            _gazeProgressElapsedTime = _gazeProgressWaitTime +
                _gazeProgressTotalTime;
        }

        //if (go == _lastGazeInteractableGO && gazeInteractable.IsSelectable) {
        if (go == _lastGazeInteractableGO) {
            _gazeProgressElapsedTime -= Time.deltaTime;

            float progressValue = _gazeProgressElapsedTime > _gazeProgressTotalTime ?
                0 : 360.0f * (1.0f - (_gazeProgressElapsedTime / _gazeProgressTotalTime));
            SetProgressValue (progressValue);

            if (_gazeProgressElapsedTime < 0 &&
                go != _alreadyInteractedGO) {
                gazeInteractable.OnUnhover();

                _alreadyInteractedGO = go;

                OnSelected(go);
            }
        }

        _lastGazeInteractableGO = go;
    }

    protected void ResetTimers()
    {
        OnUnhover(_lastGazeInteractableGO);

        _lastGazeInteractableGO = null;
        _alreadyInteractedGO = null;

        SetProgressValue(0);
    }

    protected void OnSelected(GameObject gazeInteractableGO)
    {
        OnHover(gazeInteractableGO);

        _gazeProgressElapsedTime = _gazeProgressWaitTime +
            _gazeProgressTotalTime;

        var gazeInteractable = gazeInteractableGO.GetComponent<GazeInteractable>();
        gazeInteractable.OnSelected();

        _lastGazeInteractableGO = null;
        _alreadyInteractedGO = null;

        SetProgressValue(0);
    }

    protected void OnHover(GameObject hoverGO)
    {
        if (hoverGO != null) {
            var gazeInteractable = hoverGO.GetComponent<GazeInteractable>();

            gazeInteractable.OnHover();
        }
    }

    protected void OnUnhover(GameObject unhoverGO)
    {
        if (unhoverGO != null) {
            var gazeInteractable = unhoverGO.GetComponent<GazeInteractable>();

            gazeInteractable.OnUnhover();
        }
    }

    protected void OnOver(GameObject overGO, RaycastHit hitInfo)
    {
        if (overGO != null) {
            var gazeInteractable = overGO.GetComponent<GazeInteractable>();

            var eventParameters = new GazeInteractableEventParameters();
            eventParameters.hitInfo = hitInfo;

            gazeInteractable.OnOver(eventParameters);
        }
    }

    protected void SetProgressValue(float value)
    {
        _material.SetFloat("_Angle", value);
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawLine(_parent.position, _parent.position + _parent.forward * 100);
    }
}
