using System.Collections;
using System.Collections.Generic;
using RuneGuardian;
using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    [SerializeField] private GameObject conveyorMode;
    [SerializeField] private GameObject gridMode;

    public void PickMode(GameMode mode)
    {
        if (mode == GameMode.CONVEYOR_BELT)
        {
            conveyorMode.SetActive(true);
            gridMode.SetActive(false);
        } 
        else
        {
            conveyorMode.SetActive(false);
            gridMode.SetActive(true);
        }
    }
}
