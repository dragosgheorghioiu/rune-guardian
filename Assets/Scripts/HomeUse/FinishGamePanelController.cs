using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishGamePanelController : MonoBehaviour
{
    public void OnContinueOptionSelected()
    {
        GameObject.Find("SessionContainer").SendMessage("StartIteration");
    }

    public void OnExitOptionSelected()
    {
        GameObject.Find("SessionContainer").SendMessage("ExitGame");
    }
}
