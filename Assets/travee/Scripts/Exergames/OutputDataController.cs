using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSessionOutputDataBase
{
    public DateTime StartTime;
    public DateTime FinishTime;
    public string BodySide;
}

/// <summary>
/// An example of output data.
/// </summary>
[Serializable]
public class OutputData
{
    /// <summary>
    /// Example int.
    /// </summary>
    public List<GameSessionOutputDataBase> GameSessions;

    public OutputData()
    {
        GameSessions = new List<GameSessionOutputDataBase>();
    }
}

public class OutputDataController
{
    private OutputData _outputData;

    public OutputData OutputData
    {
        get { return _outputData; }
    }

    public OutputDataController()
    {
        _outputData = new OutputData();
    }

    public void PushGameSession(GameSessionOutputDataBase gameSession)
    {
        _outputData.GameSessions.Add(gameSession);
    }

    public void SendNoteInformation(string noteText)
    {
        SessionContainer.SendMessage("OnNoteInformationSend", noteText);
    }

    private GameObject SessionContainer
    {
        get { return GameObject.Find("SessionContainer"); }
    }
}

