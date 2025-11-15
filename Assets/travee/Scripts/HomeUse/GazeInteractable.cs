using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//public abstract class GazeInteractableService
//{
//    public abstract void OnSelected();
//}

//public delegate void OnSelected();

public struct GazeInteractableEventParameters
{
    public RaycastHit hitInfo;
}

public class GazeInteractable : MonoBehaviour
{
    //protected GazeInteractableService _gazeInteractableService;

    //[SerializeField]
    //protected bool _isSelectable = true;
    [SerializeField]
    protected UnityEvent _onSelected;
    [SerializeField]
    protected UnityEvent _onHover;
    [SerializeField]
    protected UnityEvent _onUnhover;
    [SerializeField]
    protected UnityEvent<GazeInteractableEventParameters> _onOver;

    //public bool IsSelectable
    //{
    //    get { return _isSelectable; }
    //}

    //public GazeInteractable()
    //{
    //    _gazeInteractableService = null;
    //}

    //public void AttachGazeInteractableService(GazeInteractableService gazeInteractableService)
    //{
    //    _gazeInteractableService = gazeInteractableService;
    //}

    public void AttachOnSelectedAction(UnityAction action)
    {
        _onSelected.AddListener(action);
    }

    public void OnSelected()
    {
        //_gazeInteractableService.OnSelected();
        _onSelected.Invoke();
    }

    public void OnHover()
    {
        _onHover.Invoke();
    }

    public void OnUnhover()
    {
        _onUnhover.Invoke();
    }

    public void OnOver(GazeInteractableEventParameters eventParameters)
    {
        _onOver.Invoke(eventParameters);
    }
}
