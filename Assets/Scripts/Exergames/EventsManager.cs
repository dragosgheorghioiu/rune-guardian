using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventsManager
{
    private List<string> _eventNames;

    public EventsManager()
    {
        _eventNames = new List<string>();
    }

    public string EventName
    {
        get { return _eventNames[0]; }
        set
        {
            _eventNames.Clear();

            _eventNames.Add(value);

            EmitEventList();
        }
    }

    public void PushEvent(string eventName)
    {
        _eventNames.Add( eventName );
    }

    public void RemoveEvent(string eventName)
    {
        _eventNames.Remove(eventName);
    }

    private void EmitEventList()
    {
        SessionContainer.SendMessage("OnEventsUpdateEmit", _eventNames);
    }

    private GameObject SessionContainer
    {
        get { return GameObject.Find("SessionContainer"); }
    }
}
