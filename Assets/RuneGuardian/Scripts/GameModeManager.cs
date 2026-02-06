using RuneGuardian;
using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    [SerializeField] private GameObject conveyorMode;
    [SerializeField] private GameObject sphereMode;
    [SerializeField] private GameObject gridMode;

    public static GameMode CurrentMode { get; private set; } = GameMode.CONVEYOR_BELT;

    public void PickMode(GameMode mode)
    {
        conveyorMode.SetActive(false);
        gridMode.SetActive(false);
        sphereMode.SetActive(false);

        CurrentMode = mode;
        switch (CurrentMode)
        {
            case GameMode.CONVEYOR_BELT:
            {
                conveyorMode.SetActive(true);
            } break;
            case GameMode.GRID:
            {
                gridMode.SetActive(true);
            } break;
            case GameMode.SPHERE:
            {
                sphereMode.SetActive(true);
            } break;
        }
    }
}
