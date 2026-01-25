using RuneGuardian;
using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    [SerializeField] private GameObject conveyorMode;
    [SerializeField] private GameObject gridMode;

    public static GameMode CurrentMode { get; private set; } = GameMode.CONVEYOR_BELT;

    public void PickMode(GameMode mode)
    {
        CurrentMode = mode;
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
