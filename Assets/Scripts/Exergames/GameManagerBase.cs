using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public enum GameUseState
{
    Clinical_Use = 0,
    Home_Use = 1
}

public abstract class GameManagerBase : MonoBehaviour
{
    /// <summary>
    /// Manager assigned with transmitting constraints to the control 
    /// panel operated by the therapist.
    /// </summary>
    protected EventsManager _eventsManager;

    /// <summary>
    /// Manager assigned with transmitting output information for each game
    /// session completed in the home use context.
    /// </summary>
    protected OutputDataController _outputDataController;

    /// <summary>
    /// Manager assigned with calculating the duration of a game session.
    /// </summary>
    protected TimerController _timerController;

    /// <summary>
    /// Manager assigned with communication with the HDC.
    /// </summary>
    protected HDCCommunicationController _hdcCommunicationController;

    //public TimeController TimeController
    //{
    //    get { return _timeController; }
    //}

    public GameManagerBase()
    {
        _eventsManager = new EventsManager();
        _outputDataController = new OutputDataController();
        _timerController = new TimerController();
        _hdcCommunicationController = new HDCCommunicationController();
    }

    public OutputData OutputData
    {
        get { return _outputDataController.OutputData; }
    }

    public void SendHDCMessage(string message)
    {
        _hdcCommunicationController.SendMessage(message);
    }
}
