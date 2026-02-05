using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Events;

namespace RuneGuardian
{
    public class GestureRecognizerExample : MonoBehaviour
    {
        public static event Action<int, Vector3> OnValidGesture;

        // Maps gesture shape names to projectile type indices
        private static Dictionary<string, int> shapeToProjectileMap = new Dictionary<string, int>();

        void OnEnable()
        {
            RuneGuardianController.OnRuneGuardianInit += Init;
        }

        void OnDisable()
        {
            RuneGuardianController.OnRuneGuardianInit -= Init;
        }

        [Header("Gesture Recognition")]
        private Unistroke recognizer;
        private List<Vector2> currentGesture;
        private bool isDrawing = false;

        [Header("Visual Feedback")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] public Camera mainCamera;
        [SerializeField] private Color drawColor = Color.white;
        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private ParticleSystem drawingParticlesPrefab;

        [Header("Audio Feedback")]
        [SerializeField] private AudioClip drawLoopSound;

        [Header("VR Controller Settings")]
        [SerializeField] private Transform rightControllerTransform;
        [SerializeField] private Transform leftControllerTransform;
        [SerializeField] private bool useRightHand;

        [Header("Hand Tracking (Pinch)")]
        [SerializeField] private OVRHand rightOVRHand;
        [SerializeField] private OVRHand leftOVRHand;
        [SerializeField] private Transform rightIndexTip;
        [SerializeField] private Transform leftIndexTip;

        [Header("Recognition Settings")]
        [SerializeField] private int minPoints = 10;
        [SerializeField] private float minScore = 0.5f;
        [SerializeField] private float minPointDistance = 0.01f; // Minimum distance between points

        [Header("Keyboard Fallback")]
        [SerializeField] private Transform keyboardSpawnPosition;
        [SerializeField] private bool enableKeyboardFallback = true;
        [SerializeField] private KeyCode shootTypeAKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode shootTypeBKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode shootTypeTriangleKey = KeyCode.Alpha3;

        [Header("Drawing Mode")]
        private bool useToggleMode = false;

        private GameObject lineObject;
        private ParticleSystem activeParticleSystem;
        private Transform activeControllerTransform;
        private InputDevice activeDevice;
        private XRNode activeNode;
        private Vector3 lastRecordedPosition;
        private AudioSource audioSource;
        private OVRHand.HandFinger pinchFinger = OVRHand.HandFinger.Index;

        // Toggle mode tracking
        private bool wasControllerGripPressed = false;
        private bool wasAnyPinching = false;
        private Transform activeDrawingTransform = null; // Track the active drawing transform in toggle mode

        /// <summary>
        /// Represents a shape template with its name and corresponding add function
        /// </summary>
        [System.Serializable]
        public class ShapeTemplate
        {
            public string Name;
            public UnityEvent AddTemplateAction;
            public GameObject BulletinDrawing;

            public ShapeTemplate(string name, UnityEvent addTemplateAction, GameObject bulletinDrawing)
            {
                Name = name;
                AddTemplateAction = addTemplateAction;
                BulletinDrawing = bulletinDrawing;
            }
        }

        public List<ShapeTemplate> availableShapes;

        [SerializeField] private BulletinBoard bulletinBoard;
        [SerializeField] private List<GameObject> spellTypeBulletinBoardDrawing;
        private static Vector3 DefaultPositionOffset = new Vector3(-0.4f, 2.3f, 0.127f);

        void Init(InputData inputData)
        {
            if (inputData != null)
            {
                minScore = inputData.gestureMinScore / 100.0f;
            }

            recognizer = new Unistroke();

            // Reset game statistics when initializing
            GameStats.Reset();

            // Clear and rebuild the shape-to-projectile mapping
            shapeToProjectileMap.Clear();

            int objectTypeCount = 0;
            if (inputData.enabledDirtyObjects)
            {
                ShapeTemplate validShape = availableShapes[inputData.dirtyObjectsDrawing];
                bulletinBoard.SpawnShapeWithSpell(spellTypeBulletinBoardDrawing[0], validShape, objectTypeCount);
                // Map this shape name to projectile type 0 (dirty)
                shapeToProjectileMap[validShape.Name] = 0;
                Debug.Log($"Mapped gesture '{validShape.Name}' -> Dirty Objects (projectile 0)");
                ++objectTypeCount;
            }
            if (inputData.enabledDestroyedObjects)
            {
                ShapeTemplate validShape = availableShapes[inputData.destroyedObjectsDrawing];
                bulletinBoard.SpawnShapeWithSpell(spellTypeBulletinBoardDrawing[1], validShape, objectTypeCount);
                // Map this shape name to projectile type 1 (destroyed)
                shapeToProjectileMap[validShape.Name] = 1;
                Debug.Log($"Mapped gesture '{validShape.Name}' -> Destroyed Objects (projectile 1)");
                ++objectTypeCount;
            }
            if (inputData.enabledUncoloredObjects)
            {
                ShapeTemplate validShape = availableShapes[inputData.uncoloredObjectsDrawing];
                bulletinBoard.SpawnShapeWithSpell(spellTypeBulletinBoardDrawing[2], validShape, objectTypeCount);
                // Map this shape name to projectile type 2 (uncolored)
                shapeToProjectileMap[validShape.Name] = 2;
                Debug.Log($"Mapped gesture '{validShape.Name}' -> Uncolored Objects (projectile 2)");
                ++objectTypeCount;
            }
            if (bulletinBoard == null)
            {
                Debug.LogError("bulletinBoard is not assigned!");
                return;
            }

            mainCamera ??= Camera.main;

            if (rightControllerTransform == null || leftControllerTransform == null)
            {
                FindVRControllers();
            }

            useRightHand = inputData.useRightHand;
            useToggleMode = inputData.useToggleMode;
            activeNode = useRightHand ? XRNode.RightHand : XRNode.LeftHand;
            pinchFinger = inputData.pinchFingerIndex switch
            {
                PinchFinger.INDEX => OVRHand.HandFinger.Index,
                PinchFinger.MIDDLE => OVRHand.HandFinger.Middle,
                PinchFinger.PINKY => OVRHand.HandFinger.Pinky,
                PinchFinger.RING => OVRHand.HandFinger.Ring,
                _ => OVRHand.HandFinger.Index
            };

            activeControllerTransform = useRightHand ? rightControllerTransform : leftControllerTransform;
            UpdateActiveDevice();

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("No AudioSource found on GestureRecognizerExample!");
            }

            Debug.Log("Gesture Recognizer ready! Use VR controller trigger to draw.");
            string drawModeText = useToggleMode ? "TOGGLE mode (tap to start/stop)" : "CONTINUOUS mode (hold to draw)";
            Debug.Log($"Drawing mode: {drawModeText}");
            if (enableKeyboardFallback)
            {
                Debug.Log($"Keyboard fallback enabled: {shootTypeAKey} = Square/TypeA, {shootTypeBKey} = Circle/TypeB, {shootTypeTriangleKey} = Triangle");
            }
        }

        void FindVRControllers()
        {
            var allTransforms = FindObjectsOfType<Transform>();
            foreach (var t in allTransforms)
            {
                string lowerName = t.name.ToLower();
                if (rightControllerTransform == null && lowerName.Contains("right") && lowerName.Contains("controller"))
                    rightControllerTransform = t;
                if (leftControllerTransform == null && lowerName.Contains("left") && lowerName.Contains("controller"))
                    leftControllerTransform = t;
            }

            if (rightControllerTransform == null || leftControllerTransform == null)
            {
                Debug.LogWarning("VR Controllers not found! Please assign them manually.");
            }
        }

        void UpdateActiveDevice()
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(activeNode, devices);
            if (devices.Count > 0)
            {
                activeDevice = devices[0];
                Debug.Log($"Active device: {activeDevice.name}");
            }
        }

        void Update()
        {
            // Keyboard fallback controls
            if (enableKeyboardFallback)
            {

                if (Input.GetKeyDown(shootTypeAKey))
                {
                    OnValidGesture?.Invoke(0, keyboardSpawnPosition.position);
                }
                else if (Input.GetKeyDown(shootTypeBKey))
                {
                    OnValidGesture?.Invoke(1, keyboardSpawnPosition.position);
                }
                else if (Input.GetKeyDown(shootTypeTriangleKey))
                {
                    OnValidGesture?.Invoke(2, keyboardSpawnPosition.position);
                }
            }

            // Hand tracking pinch detection (both hands)
            bool rightTracked = rightOVRHand != null && rightOVRHand.IsTracked && useRightHand;
            bool leftTracked = leftOVRHand != null && leftOVRHand.IsTracked && !useRightHand;
            bool handTracked = rightTracked || leftTracked;
            bool rightPinching = rightTracked && rightOVRHand.GetFingerIsPinching(pinchFinger);
            bool leftPinching = leftTracked && leftOVRHand.GetFingerIsPinching(pinchFinger);
            bool anyPinching = rightPinching || leftPinching;

            // Get the appropriate finger tip transform based on which hand is active
            Transform fingerTipTransform = null;
            if (rightTracked && rightIndexTip != null)
                fingerTipTransform = rightIndexTip;
            else if (leftTracked && leftIndexTip != null)
                fingerTipTransform = leftIndexTip;

            // For pinch detection, we still need to know if currently pinching
            Transform drawingTransform = null;
            if (rightPinching && rightIndexTip != null)
                drawingTransform = rightIndexTip;
            else if (leftPinching && leftIndexTip != null)
                drawingTransform = leftIndexTip;

            // Check if controller grip is pressed
            bool controllerGripPressed = false;
            bool controllerTracked = false;
            if (!activeDevice.isValid)
            {
                UpdateActiveDevice();
            }
            if (activeDevice.isValid && activeControllerTransform != null)
            {
                if (activeDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gripButton))
                {
                    controllerGripPressed = gripButton;
                }
                if (activeDevice.TryGetFeatureValue(CommonUsages.isTracked, out bool isTracked))
                {
                    controllerTracked = isTracked;
                }
            }

            // If controller grip is pressed and controller is tracked, prioritize controller drawing and ignore hand tracking completely
            if (controllerTracked && activeControllerTransform != null)
            {
                if (useToggleMode)
                {
                    // Toggle mode: detect edge (transition) from not pressed to pressed
                    if (controllerGripPressed && !wasControllerGripPressed)
                    {
                        if (!isDrawing)
                        {
                            activeDrawingTransform = activeControllerTransform;
                            StartDrawing(activeControllerTransform);
                        }
                        else
                        {
                            StopDrawing();
                            activeDrawingTransform = null;
                        }
                    }
                    else if (isDrawing && activeDrawingTransform != null)
                    {
                        // Continue drawing using the stored transform, even if not pressing
                        ContinueDrawing(activeDrawingTransform);
                    }
                }
                else
                {
                    // Continuous mode: hold to draw
                    if (controllerGripPressed)
                    {
                        if (!isDrawing)
                        {
                            StartDrawing(activeControllerTransform);
                        }
                        else
                        {
                            ContinueDrawing(activeControllerTransform);
                        }
                    }
                    else if (isDrawing)
                    {
                        StopDrawing();
                    }
                }
                wasControllerGripPressed = controllerGripPressed;
                return;
            }

            // Only allow hand tracking drawing if the controller is not being used
            if (handTracked && !controllerTracked)
            {
                if (useToggleMode)
                {
                    // Toggle mode: detect edge (transition) from not pinching to pinching
                    if (anyPinching && !wasAnyPinching && drawingTransform != null)
                    {
                        if (!isDrawing)
                        {
                            activeDrawingTransform = fingerTipTransform;
                            StartDrawing(drawingTransform);
                        }
                        else
                        {
                            StopDrawing();
                            activeDrawingTransform = null;
                        }
                    }
                    else if (isDrawing && activeDrawingTransform != null)
                    {
                        // Continue drawing using the stored finger tip transform, even when not pinching
                        ContinueDrawing(activeDrawingTransform);
                    }
                }
                else
                {
                    // Continuous mode: hold to draw
                    if (anyPinching && drawingTransform != null)
                    {
                        if (!isDrawing)
                        {
                            StartDrawing(drawingTransform);
                        }
                        else
                        {
                            ContinueDrawing(drawingTransform);
                        }
                    }
                    else if (isDrawing)
                    {
                        StopDrawing();
                    }
                }
                wasAnyPinching = anyPinching;
                return;
            }

            // Reset toggle tracking when neither input is active
            wasControllerGripPressed = false;
            wasAnyPinching = false;
            if (!isDrawing)
            {
                activeDrawingTransform = null;
            }
        }

        void StartDrawing(Transform drawTransform)
        {
            isDrawing = true;
            currentGesture = new List<Vector2>();
            CreateLineRenderer();
            CreateDrawingParticlesAt(drawTransform.position);
            lastRecordedPosition = drawTransform.position;
            AddPoint(drawTransform.position);
            if (audioSource != null && drawLoopSound != null)
            {
                audioSource.clip = drawLoopSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        void ContinueDrawing(Transform drawTransform)
        {
            Vector3 currentPos = drawTransform.position;
            if (Vector3.Distance(currentPos, lastRecordedPosition) >= minPointDistance)
            {
                AddPoint(currentPos);
                lastRecordedPosition = currentPos;
            }
            if (activeParticleSystem != null)
            {
                activeParticleSystem.transform.position = currentPos;
            }
        }

        // Create particles at a specific position
        private void CreateDrawingParticlesAt(Vector3 position)
        {
            if (drawingParticlesPrefab != null)
            {
                GameObject particlesObj = Instantiate(drawingParticlesPrefab.gameObject, position, Quaternion.identity);
                activeParticleSystem = particlesObj.GetComponent<ParticleSystem>();
                if (activeParticleSystem != null)
                {
                    activeParticleSystem.Play();
                    var main = activeParticleSystem.main;
                    main.startColor = drawColor;
                }
            }
        }

        void AddPoint(Vector3 worldPos)
        {
            // Convert to screen space for gesture recognition
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            Vector2 gesturePoint = new Vector2(screenPos.x, screenPos.y);
            currentGesture.Add(gesturePoint);

            // Update line renderer
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = currentGesture.Count;
                lineRenderer.SetPosition(currentGesture.Count - 1, worldPos);
            }
        }

        void StopDrawing()
        {
            isDrawing = false;
            activeDrawingTransform = null; // Clear the active drawing transform

            // Stop looping draw sound and reset audio source
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }

            if (currentGesture.Count >= minPoints)
            {
                var result = recognizer.Recognize(currentGesture);

                if (result.Score >= minScore)
                {
                    Debug.Log($"✓ Recognized: {result.Name} (Score: {result.Score:F2})");
                    // Record successful gesture
                    GameStats.RecordGesture(result.Score, true);
                    // Get the last drawn world position from the line renderer
                    Vector3 lastPoint = Vector3.zero;
                    if (lineRenderer != null && lineRenderer.positionCount > 0)
                        lastPoint = lineRenderer.GetPosition(lineRenderer.positionCount - 1);

                    // Convert shape name to projectile type index using mapping
                    if (shapeToProjectileMap.TryGetValue(result.Name, out int projectileIndex))
                    {
                        OnValidGesture?.Invoke(projectileIndex, lastPoint);
                    }
                    else
                    {
                        Debug.LogWarning($"No projectile mapping found for shape: {result.Name}");
                    }
                }
                else
                {
                    Debug.Log($"? Gesture not recognized (Best: {result.Name}, Score: {result.Score:F2})");
                    // Record failed gesture
                    GameStats.RecordGesture(result.Score, false);
                }
            }
            else
            {
                Debug.Log($"Gesture too short! Need at least {minPoints} points, got {currentGesture.Count}");
                // Record failed gesture (too short = 0 score)
                GameStats.RecordGesture(0f, false);
            }

            // Clean up line renderer and particles
            if (lineObject != null)
                Destroy(lineObject, 0.5f);

            if (activeParticleSystem != null)
            {
                // Stop emission and clear all existing particles immediately
                activeParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                Destroy(activeParticleSystem.gameObject, 0.1f);
                activeParticleSystem = null;
            }
        }


        void CreateLineRenderer()
        {
            lineObject = new GameObject("GestureLine");
            lineRenderer = lineObject.AddComponent<LineRenderer>();

            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = drawColor;
            lineRenderer.endColor = drawColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingOrder = 100;
        }

        void CreateDrawingParticles()
        {
            if (drawingParticlesPrefab != null)
            {
                GameObject particlesObj = Instantiate(drawingParticlesPrefab.gameObject, activeControllerTransform.position, Quaternion.identity);
                activeParticleSystem = particlesObj.GetComponent<ParticleSystem>();

                if (activeParticleSystem != null)
                {
                    activeParticleSystem.Play();

                    // Set particle color to match drawing color
                    var main = activeParticleSystem.main;
                    main.startColor = drawColor;
                }
            }
        }

        public void AddCircleTemplate()
        {
            List<Vector2> circle = new List<Vector2>();
            int numPoints = 32;
            for (int i = 0; i < numPoints; i++)
            {
                float angle = i * 2f * Mathf.PI / numPoints;
                circle.Add(new Vector2(Mathf.Cos(angle) * 100f, Mathf.Sin(angle) * 100f));
            }
            recognizer.AddTemplate("Circle", circle);
        }

        public void AddSquareTemplate()
        {
            // Clockwise square
            List<Vector2> square1 = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(100, 0),
            new Vector2(100, 100),
            new Vector2(0, 100),
            new Vector2(0, 0)
        };
            recognizer.AddTemplate("Square", square1);

            // Counter-clockwise square
            List<Vector2> square2 = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(0, 100),
            new Vector2(100, 100),
            new Vector2(100, 0),
            new Vector2(0, 0)
        };
            recognizer.AddTemplate("Square", square2);
        }

        public void AddTriangleTemplate()
        {
            // Clockwise triangle
            List<Vector2> triangle1 = new List<Vector2>
        {
            new Vector2(50, 0),
            new Vector2(100, 100),
            new Vector2(0, 100),
            new Vector2(50, 0)
        };
            recognizer.AddTemplate("Triangle", triangle1);

            // Counter-clockwise triangle
            List<Vector2> triangle2 = new List<Vector2>
        {
            new Vector2(50, 0),
            new Vector2(0, 100),
            new Vector2(100, 100),
            new Vector2(50, 0)
        };
            recognizer.AddTemplate("Triangle", triangle2);
        }

        public void AddOpenSquareBracketTemplate()
        {
            List<Vector2> bracket = new List<Vector2>
        {
            new Vector2(100, 0),
            new Vector2(0, 0),
            new Vector2(0, 100),
            new Vector2(100, 100)
        };
            recognizer.AddTemplate("OpenSquareBracket", bracket);
        }

        public void AddStarTemplate()
        {
            // First, calculate all 5 points on a circle
            List<Vector2> points = new List<Vector2>();
            int numPoints = 5;
            for (int i = 0; i < numPoints; i++)
            {
                float angle = i * 2f * Mathf.PI / numPoints - Mathf.PI / 2f;
                points.Add(new Vector2(Mathf.Cos(angle) * 100f + 100f, Mathf.Sin(angle) * 100f + 100f));
            }

            // Connect them in star order: 1, 3, 5, 2, 4, 1 (or 0, 2, 4, 1, 3, 0 in 0-based indexing)
            List<Vector2> star = new List<Vector2>
            {
                points[0], // 1
                points[2], // 3
                points[4], // 5
                points[1], // 2
                points[3], // 4
                points[0]  // back to 1 to close
            };
            recognizer.AddTemplate("Star", star);
        }

        public void AddZigzagTemplate()
        {
            List<Vector2> zigzag = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(50, 50),
            new Vector2(0, 100),
            new Vector2(50, 150),
            new Vector2(0, 200)
        };
            recognizer.AddTemplate("Zigzag", zigzag);
        }

        public void AddXTemplate()
        {
            // Triangle shape with horizontal bar (like letter A)
            // Draw: bottom-left → top → bottom-right → middle-right → middle-left
            List<Vector2> x = new List<Vector2>
            {
                new Vector2(10, 100),    // bottom-left
                new Vector2(50, 0),      // top (peak)
                new Vector2(90, 100),    // bottom-right
                new Vector2(60, 50),     // middle-right (shorter crossbar)
                new Vector2(40, 50),     // middle-left (shorter crossbar)
            };
            recognizer.AddTemplate("X", x);
        }

        public void AddDiamondTemplate()
        {
            List<Vector2> diamond = new List<Vector2>
        {
            new Vector2(50, 0),
            new Vector2(100, 50),
            new Vector2(50, 100),
            new Vector2(0, 50),
            new Vector2(50, 0)
        };
            recognizer.AddTemplate("Diamong", diamond);
        }

        public void AddVTemplate()
        {
            List<Vector2> v = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(50, 100),
            new Vector2(100, 0)
        };
            recognizer.AddTemplate("V", v);
        }

        public void AddLineTemplate()
        {
            List<Vector2> line = new List<Vector2>
        {
            new Vector2(0, 50),
            new Vector2(100, 50)
        };
            recognizer.AddTemplate("Line", line);
        }

        public bool IsGridModeActive()
        {
            return GameModeManager.CurrentMode.ToString() == "GRID";
        }

        /// <summary>
        /// Toggles between continuous draw mode (hold to draw) and toggle draw mode (tap to start/stop)
        /// </summary>
        public void SetDrawingMode(bool toggleMode)
        {
            useToggleMode = toggleMode;
            string drawModeText = useToggleMode ? "TOGGLE mode (tap to start/stop)" : "CONTINUOUS mode (hold to draw)";
            Debug.Log($"Drawing mode changed to: {drawModeText}");
        }
    }
}
