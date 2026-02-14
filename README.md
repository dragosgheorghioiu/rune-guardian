# Rune Guardian (name to be replaced)

Rune Guardian is a Virtual Reality (VR) and Mixed Reality (MR) experience where players act as a Christmas elf fixing broken or dirty toys by drawing magical runes in the air.

The basic gameplay loop involves toys appearing in the environment, driven by a conveyor belt, each with a specific defect (Dirty, Destroyed, or Uncolored).
The player must identify the defect and draw the corresponding rune to repair the toy.

## Game Modes

The game features three distinct modes, each offering a different way to interact with the environment:

1. **Sphere (Magic Orbs)**: The player has 8 magic orbs floating in front of them.
    When a toy appears, the player must touch the orbs in the correct sequence to cast the spell and repair the toy.
    The next orb in the sequence will turn orange, while orbs already touched will turn green.
2. **Conveyor Belt (Normal)**: Toys appear one at a time on a single conveyor belt.
    The player must cast the spell by drawing the correct rune in the air as the toy moves along the belt.
3. **Grid (Challenge)**: This mode is similar to the Conveyor Belt mode, but it features 3 conveyor belts.
    Toys can appear on any of the belts, and the player must look at the toy when casting the spell to ensure it hits it.

## Logic Structure

The project follows a modular structure to facilitate easy expansion and integration with the Travee framework.

### Core Systems

- **[SessionContainer.cs](Assets/RuneGuardian/Scripts/TraveeUtils/SessionContainer.cs)**: The main entry point. It handles the game lifecycle (Start/Stop) and holds the `InputData` configuration.
- **[RuneGuardianController.cs](Assets/RuneGuardian/Scripts/RuneGuardianController.cs)**: Orchestrates the high-level game flow and triggers events for initialization and start.
- **[GameModeManager.cs](Assets/RuneGuardian/Scripts/GameModeManager.cs)**: Manages the activation and deactivation of environment objects based on the selected `GameMode`.

### Spawning & Toys

- **[ToySpawnerManager.cs](Assets/RuneGuardian/Scripts/ToySpawnerManager.cs)**: Coordinates multiple spawners and responds to toy despawn events to maintain game flow.
- **[RandomToySpawner.cs](Assets/RuneGuardian/Scripts/RandomToySpawner.cs)**: Handles the instantiation of toys. It assigns a "damaged" state (Dirty, Destroyed, or Uncolored) and attaches the corresponding visual rune symbol.
- **[SpawnedToy.cs](Assets/RuneGuardian/Scripts/SpawnedToy.cs)**: Controls the individual toy logic, including movement, state transitions (Moving, Waiting, Repaired), and hit detection.

### Magic & Gestures

- **[GestureRecognizerExample.cs](Assets/RuneGuardian/Scripts/GestureRecognizerExample.cs)**: Captures hand or controller input and uses the **$1 Unistroke** algorithm to recognize drawn shapes.
- **[Unistroke.cs](Assets/RuneGuardian/Scripts/Unistroke.cs)**: A lightweight gesture recognition engine that compares the player's drawing against a set of templates.
- **[ShootSpell.cs](Assets/RuneGuardian/Scripts/ShootSpell.cs)**: Listens for valid gestures and fires the appropriate `SpellProjectile` towards the target.
- **[SpellProjectile.cs](Assets/RuneGuardian/Scripts/SpellProjectile.cs)**: Handles the physics and collision of the magical spell, triggering the repair logic on toys.

## How to Contribute

### Adding New Toys

1. Create a prefab for the toy with at least two variants (Damaged and Repaired).
2. Attach the `SpawnedToy` script and configure the `normalModel` and `hitModel` references.
3. Add the prefab to the `prefabs` list in the `RandomToySpawner` instances in the scene.

### Adding New Runes

1. Open `GestureRecognizerExample.cs`.
2. Ensure there is a template in `Unistroke` for the new shape.
3. Update the `ProjectileType` enum and the mapping in `GestureRecognizerExample` to handle the new shape.

### Project Structure

- `Assets/RuneGuardian/Scripts`: Core gameplay logic.
- `Assets/RuneGuardian/Prefabs`: Reusable game objects and toys.
- `Assets/RuneGuardian/MagicBall`: Specific assets for the Sphere mode.
- `Assets/Scripts`: Common utilities and framework components.

## Sample Input Data

```json
{
  "inputParameters": [
    {
      "name": "gameMode",
      "text": [
        {
          "language": "en",
          "value": "Game Mode"
        }
      ],
      "type": "list",
      "value": [
        {
          "text": [
            {
              "language": "en",
              "value": "Sphere"
            }
          ],
          "value": "0"
        },
        {
          "text": [
            {
              "language": "en",
              "value": "Conveyor"
            }
          ],
          "value": "1"
        },
        {
          "text": [
            {
              "language": "en",
              "value": "Grid"
            }
          ],
          "value": "2"
        }
      ],
      "defaultValue": "0",
      "categoryName": "Game"
    },
    {
      "name": "ambientMusic",
      "text": "Enable Ambient Music",
      "type": "bool",
      "defaultValue": true,
      "categoryName": "Core"
    },
    {
      "name": "useRightHand",
      "text": "Use Right Hand",
      "type": "bool",
      "defaultValue": true,
      "categoryName": "Core"
    },
    {
      "name": "useToggleMode",
      "text": "Use Toggle Mode",
      "type": "bool",
      "defaultValue": false,
      "categoryName": "Core"
    },
    {
      "name": "pinchFingerIndex",
      "text": "Pinch Finger",
      "type": "list",
      "value": [
        {
          "text": "Index",
          "value": "0"
        },
        {
          "text": "Middle",
          "value": "1"
        },
        {
          "text": "Ring",
          "value": "2"
        },
        {
          "text": "Pinky",
          "value": "3"
        }
      ],
      "defaultValue": "0",
      "categoryName": "Core"
    },
    {
      "name": "enabledDirtyObjects",
      "text": "Enable Teddy Bears",
      "type": "bool",
      "defaultValue": true,
      "categoryName": "Round"
    },
    {
      "name": "enabledDestroyedObjects",
      "text": "Enable Robots",
      "type": "bool",
      "defaultValue": true,
      "categoryName": "Round"
    },
    {
      "name": "enabledUncoloredObjects",
      "text": "Enable Tanks",
      "type": "bool",
      "defaultValue": true,
      "categoryName": "Round"
    },
    {
      "name": "dirtyObjectsDrawing",
      "text": "Teddy Bear Spell",
      "type": "list",
      "defaultValue": 0,
      "categoryName": "Round",
      "value": [
        {
          "text": "Circle",
          "value": "0"
        },
        {
          "text": "Square",
          "value": "1"
        },
        {
          "text": "Triangle",
          "value": "2"
        },
        {
          "text": "OpenSquareBracket",
          "value": "3"
        },
        {
          "text": "Star",
          "value": "4"
        },
        {
          "text": "Zigzag",
          "value": "5"
        },
        {
          "text": "X",
          "value": "6"
        },
        {
          "text": "V",
          "value": "7"
        }
      ]
    },
    {
      "name": "destroyedObjectsDrawing",
      "text": "Robot Spell",
      "type": "list",
      "defaultValue": 1,
      "categoryName": "Round",
      "value": [
        {
          "text": "Circle",
          "value": "0"
        },
        {
          "text": "Square",
          "value": "1"
        },
        {
          "text": "Triangle",
          "value": "2"
        },
        {
          "text": "OpenSquareBracket",
          "value": "3"
        },
        {
          "text": "Star",
          "value": "4"
        },
        {
          "text": "Zigzag",
          "value": "5"
        },
        {
          "text": "X",
          "value": "6"
        },
        {
          "text": "V",
          "value": "7"
        }
      ]
    },
    {
      "name": "uncoloredObjectsDrawing",
      "text": "Tank Spell",
      "type": "list",
      "defaultValue": 2,
      "categoryName": "Round",
      "value": [
        {
          "text": "Circle",
          "value": "0"
        },
        {
          "text": "Square",
          "value": "1"
        },
        {
          "text": "Triangle",
          "value": "2"
        },
        {
          "text": "OpenSquareBracket",
          "value": "3"
        },
        {
          "text": "Star",
          "value": "4"
        },
        {
          "text": "Zigzag",
          "value": "5"
        },
        {
          "text": "X",
          "value": "6"
        },
        {
          "text": "V",
          "value": "7"
        }
      ]
    },
    {
      "name": "gestureMinScore",
      "text": "Gesture Min Score",
      "type": "int",
      "defaultValue": 60,
      "minValue": 30,
      "maxValue": 95,
      "categoryName": "Game"
    },
    {
      "name": "numberOfToys",
      "text": "Number Of Toys",
      "type": "int",
      "defaultValue": 10,
      "minValue": 1,
      "maxValue": 50,
      "categoryName": "Game"
    },
    {
      "name": "randomizeShapes",
      "text": "Randomize Shapes Per Toy",
      "type": "bool",
      "defaultValue": false,
      "categoryName": "Game"
    }
  ],
  "inputParameterCategories": [
    {
      "name": "Core",
      "text": [
        {
          "language": "en",
          "value": "Core"
        }
      ]
    },
    {
      "name": "Game",
      "text": [
        {
          "language": "en",
          "value": "Game"
        }
      ]
    },
    {
      "name": "Round",
      "text": [
        {
          "language": "en",
          "value": "Round"
        }
      ]
    }
  ],
  "buttons": [
    {
      "name": "startgame",
      "text": "Start Game",
      "type": "start",
      "observedEvents": [
        {
          "name": "show",
          "activeEventNames": [
            "onNoGameIsPlaying"
          ]
        }
      ]
    },
    {
      "name": "stopgame",
      "text": "Stop Game",
      "type": "stop",
      "observedEvents": [
        {
          "name": "show",
          "activeEventNames": [
            "onGameIsPlaying"
          ]
        }
      ]
    }
  ]
}
```
