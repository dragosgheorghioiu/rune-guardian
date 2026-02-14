using RuneGuardian;
using UnityEngine;

/// <summary>
/// Manages transitions between different game modes and handles the activation of mode-specific environment objects.
/// </summary>
public class GameModeManager : MonoBehaviour
{
    [SerializeField] private GameObject conveyorMode;
    [SerializeField] private GameObject sphereMode;
    [SerializeField] private GameObject gridMode;

    /// <summary>
    /// Gets the current active game mode.
    /// </summary>
    public static GameMode CurrentMode { get; private set; } = GameMode.CONVEYOR_BELT;

    /// <summary>
    /// Switches the game environment to the specified mode.
    /// </summary>
    /// <param name="mode">The target game mode to activate.</param>
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
                }
                break;
            case GameMode.GRID:
                {
                    gridMode.SetActive(true);
                }
                break;
            case GameMode.SPHERE:
                {
                    sphereMode.SetActive(true);
                }
                break;
        }
    }
}
