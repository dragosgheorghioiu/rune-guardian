using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HDCCommunicationController
{
    public void SendMessage(string message)
    {
        SessionContainerGameObject.SendMessage("OnHDCMessageSend", message);
    }

    private GameObject SessionContainerGameObject
    {
        get {
            return GameObject.Find("SessionContainer");
        }
    }
}
