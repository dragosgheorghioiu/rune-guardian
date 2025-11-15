using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerController
{
    private DateTime _startDateTime;
    private DateTime? _finishDateTime;

    public DateTime StartDateTime
    {
        get { return _startDateTime; }
    }

    public DateTime? FinishDateTime
    {
        get { return _finishDateTime; }
    }

    //public bool Pause
    //{
    //    get { return _pause; }
    //    set { _pause = value; }
    //}

    public TimeSpan GameDuration
    {
        get {
            var finishDateTime = _finishDateTime;

            if (finishDateTime == null) {
                finishDateTime = DateTime.Now;
            }

            return (finishDateTime.Value - _startDateTime);
        }
    }

    public void StartTimer()
    {
        _startDateTime = DateTime.Now;
        _finishDateTime = null;
    }

    public void StopTimer()
    {
        _finishDateTime = DateTime.Now;
    }
}
