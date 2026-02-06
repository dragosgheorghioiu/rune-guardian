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

        private static Dictionary<string, int> shapeToProjectileMap = new Dictionary<string, int>();

        void OnEnable()
        {
            RuneGuardianController.OnRuneGuardianInit += Init;
        }

        void OnDisable()
        {
            RuneGuardianController.OnRuneGuardianInit -= Init;
        }

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
        [SerializeField] private Transform rightMiddleTip;
        [SerializeField] private Transform rightRingTip;
        [SerializeField] private Transform rightPinkyTip;
        [SerializeField] private Transform leftIndexTip;
        [SerializeField] private Transform leftMiddleTip;
        [SerializeField] private Transform leftRingTip;
        [SerializeField] private Transform leftPinkyTip;
        [SerializeField] private float pinchStrengthThreshold = 0.3f; // Relaxed threshold for pinch detection

        [Header("Recognition Settings")]
        [SerializeField] private int minPoints = 10;
        [SerializeField] private float minScore = 0.5f;
        [SerializeField] private float minPointDistance = 0.01f; // Minimum distance between points

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
        private Transform currentFingerTip = null; // Track the current finger tip for drawing

        // Toggle mode tracking
        private bool wasControllerGripPressed = false;
        private bool wasHandPinching = false; // Track pinch state for toggle mode
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

        private InputData currentInputData;

        public List<ShapeTemplate> availableShapes;

        [SerializeField] private BulletinBoard bulletinBoard;
        [SerializeField] private GameObject bulletinBoardObject;
        [SerializeField] private List<GameObject> spellTypeBulletinBoardDrawing;
        private bool isSphereMode;
        private List<ShapeTemplate> enabledShapes = new List<ShapeTemplate>();
        private bool shouldRandomizeShapes = false;
        private List<GameObject> spawnedBulletinBoardObjects = new List<GameObject>(); // Track dynamically spawned bulletin board children

        void Init(InputData inputData)
        {
            currentInputData = inputData;
            shouldRandomizeShapes = inputData.randomizeShapes;

            if (inputData != null)
            {
                minScore = inputData.gestureMinScore / 100.0f;
            }

            isSphereMode = inputData.gameMode == GameMode.SPHERE;
            if (isSphereMode) return;

            // ======= NON-SPHERE MODE INITIALIZATION =======
            recognizer = new Unistroke();

            // Reset game statistics when initializing
            GameStats.Reset();

            // Clear any previously spawned bulletin board objects
            bulletinBoardObject.SetActive(true);
            spawnedBulletinBoardObjects.Clear();

            // Collect enabled shapes
            enabledShapes.Clear();
            if (inputData.enabledDirtyObjects)
            {
                enabledShapes.Add(availableShapes[inputData.dirtyObjectsDrawing]);
            }
            if (inputData.enabledDestroyedObjects)
            {
                enabledShapes.Add(availableShapes[inputData.destroyedObjectsDrawing]);
            }
            if (inputData.enabledUncoloredObjects)
            {
                enabledShapes.Add(availableShapes[inputData.uncoloredObjectsDrawing]);
            }

            // Clear and rebuild the shape-to-projectile mapping
            BuildShapeMapping(inputData);

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

            // Set the correct finger tip based on hand and pinch finger
            currentFingerTip = GetFingerTipTransform(useRightHand, pinchFinger);

            activeControllerTransform = useRightHand ? rightControllerTransform : leftControllerTransform;
            UpdateActiveDevice();

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("No AudioSource found on GestureRecognizerExample!");
            }
        }

        /// <summary>
        /// Builds the shape-to-projectile mapping based on InputData
        /// </summary>
        private void BuildShapeMapping(InputData inputData)
        {
            shapeToProjectileMap.Clear();

            int objectTypeCount = 0;
            if (inputData.enabledDirtyObjects)
            {
                ShapeTemplate validShape = availableShapes[inputData.dirtyObjectsDrawing];
                GameObject[] spawned = bulletinBoard.SpawnShapeWithSpell(spellTypeBulletinBoardDrawing[0], validShape, objectTypeCount);
                spawnedBulletinBoardObjects.AddRange(spawned);
                // Add the gesture template for this shape
                validShape.AddTemplateAction?.Invoke();
                // Map this shape name to projectile type 0 (dirty)
                shapeToProjectileMap[validShape.Name] = 0;
                ++objectTypeCount;
            }
            if (inputData.enabledDestroyedObjects)
            {
                ShapeTemplate validShape = availableShapes[inputData.destroyedObjectsDrawing];
                GameObject[] spawned = bulletinBoard.SpawnShapeWithSpell(spellTypeBulletinBoardDrawing[1], validShape, objectTypeCount);
                spawnedBulletinBoardObjects.AddRange(spawned);
                // Add the gesture template for this shape
                validShape.AddTemplateAction?.Invoke();
                // Map this shape name to projectile type 1 (destroyed)
                shapeToProjectileMap[validShape.Name] = 1;
                ++objectTypeCount;
            }
            if (inputData.enabledUncoloredObjects)
            {
                ShapeTemplate validShape = availableShapes[inputData.uncoloredObjectsDrawing];
                GameObject[] spawned = bulletinBoard.SpawnShapeWithSpell(spellTypeBulletinBoardDrawing[2], validShape, objectTypeCount);
                spawnedBulletinBoardObjects.AddRange(spawned);
                // Add the gesture template for this shape
                validShape.AddTemplateAction?.Invoke();
                // Map this shape name to projectile type 2 (uncolored)
                shapeToProjectileMap[validShape.Name] = 2;
            }
        }

        /// <summary>
        /// Randomizes the shape-to-projectile mapping for symbol randomization.
        /// Picks random shapes from all available shapes and updates the bulletin board.
        /// </summary>
        public void RandomizeShapeMapping()
        {
            if (!shouldRandomizeShapes || enabledShapes.Count == 0 || bulletinBoard == null)
            {
                return;
            }

            // Get the number of enabled object types
            int targetShapeCount = currentInputData.enabledDirtyObjects ? 1 : 0;
            targetShapeCount += currentInputData.enabledDestroyedObjects ? 1 : 0;
            targetShapeCount += currentInputData.enabledUncoloredObjects ? 1 : 0;

            if (targetShapeCount == 0 || availableShapes.Count < targetShapeCount)
            {
                Debug.LogWarning("Not enough shapes available for randomization");
                return;
            }

            // Pick random unique shapes from all available shapes
            List<int> selectedIndices = new List<int>();
            List<int> availableIndices = new List<int>();
            for (int i = 0; i < availableShapes.Count; i++)
            {
                availableIndices.Add(i);
            }

            // Shuffle and pick the first N shapes
            for (int i = availableIndices.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                var temp = availableIndices[i];
                availableIndices[i] = availableIndices[randomIndex];
                availableIndices[randomIndex] = temp;
            }

            selectedIndices = availableIndices.GetRange(0, targetShapeCount);
            List<ShapeTemplate> selectedShapes = new List<ShapeTemplate>();
            foreach (var idx in selectedIndices)
            {
                selectedShapes.Add(availableShapes[idx]);
            }

            // Clear only the dynamically spawned bulletin board objects (preserve initial prefab children)
            foreach (GameObject spawnedObject in spawnedBulletinBoardObjects)
            {
                if (spawnedObject != null)
                {
                    Destroy(spawnedObject);
                }
            }
            spawnedBulletinBoardObjects.Clear();

            // Clear old gesture templates from the recognizer to prevent conflicts
            recognizer.ClearTemplates();

            // Clear and rebuild the shape-to-projectile mapping
            shapeToProjectileMap.Clear();

            // Rebuild mapping and bulletin board with new random shapes
            int objectTypeCount = 0;
            if (currentInputData.enabledDirtyObjects && objectTypeCount < selectedShapes.Count)
            {
                ShapeTemplate validShape = selectedShapes[objectTypeCount];
                // Respawn on bulletin board
                GameObject[] spawned = bulletinBoard.SpawnShapeWithSpell(spellTypeBulletinBoardDrawing[0], validShape, objectTypeCount);
                spawnedBulletinBoardObjects.AddRange(spawned);
                // Add the gesture template for this shape
                validShape.AddTemplateAction.Invoke();
                shapeToProjectileMap[validShape.Name] = 0;
                Debug.Log($"Randomized: '{validShape.Name}' -> Dirty Objects (projectile 0)");
                objectTypeCount++;
            }
            if (currentInputData.enabledDestroyedObjects && objectTypeCount < selectedShapes.Count)
            {
                ShapeTemplate validShape = selectedShapes[objectTypeCount];
                GameObject[] spawned = bulletinBoard.SpawnShapeWithSpell(spellTypeBulletinBoardDrawing[1], validShape, objectTypeCount);
                spawnedBulletinBoardObjects.AddRange(spawned);
                // Add the gesture template for this shape
                validShape.AddTemplateAction.Invoke();
                shapeToProjectileMap[validShape.Name] = 1;
                Debug.Log($"Randomized: '{validShape.Name}' -> Destroyed Objects (projectile 1)");
                objectTypeCount++;
            }
            if (currentInputData.enabledUncoloredObjects && objectTypeCount < selectedShapes.Count)
            {
                ShapeTemplate validShape = selectedShapes[objectTypeCount];
                GameObject[] spawned = bulletinBoard.SpawnShapeWithSpell(spellTypeBulletinBoardDrawing[2], validShape, objectTypeCount);
                spawnedBulletinBoardObjects.AddRange(spawned);
                // Add the gesture template for this shape
                validShape.AddTemplateAction.Invoke();
                shapeToProjectileMap[validShape.Name] = 2;
                Debug.Log($"Randomized: '{validShape.Name}' -> Uncolored Objects (projectile 2)");
            }
        }

        /// <summary>
        /// Gets the finger tip transform for the specified hand and finger.
        /// </summary>
        private Transform GetFingerTipTransform(bool rightHand, OVRHand.HandFinger finger)
        {
            if (rightHand)
            {
                return finger switch
                {
                    OVRHand.HandFinger.Index => rightIndexTip,
                    OVRHand.HandFinger.Middle => rightMiddleTip,
                    OVRHand.HandFinger.Ring => rightRingTip,
                    OVRHand.HandFinger.Pinky => rightPinkyTip,
                    _ => rightIndexTip
                };
            }
            else
            {
                return finger switch
                {
                    OVRHand.HandFinger.Index => leftIndexTip,
                    OVRHand.HandFinger.Middle => leftMiddleTip,
                    OVRHand.HandFinger.Ring => leftRingTip,
                    OVRHand.HandFinger.Pinky => leftPinkyTip,
                    _ => leftIndexTip
                };
            }
        }

        /// <summary>
        /// Checks if a finger is pinching with relaxed threshold.
        /// Uses pinch strength instead of strict binary detection.
        /// </summary>
        private bool IsFingerPinching(OVRHand hand, OVRHand.HandFinger finger)
        {
            if (hand == null || !hand.IsTracked)
                return false;

            // Try strict pinching first
            if (hand.GetFingerIsPinching(finger))
                return true;

            // Fallback to pinch strength threshold for relaxed detection
            if (hand.GetFingerPinchStrength(finger) >= pinchStrengthThreshold)
                return true;

            return false;
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
            if (isSphereMode)
            {
                return;
            }

            // Hand tracking detection (both hands) - Pinch or Closed Hand
            bool rightTracked = rightOVRHand != null && rightOVRHand.IsTracked && useRightHand;
            bool leftTracked = leftOVRHand != null && leftOVRHand.IsTracked && !useRightHand;
            bool handTracked = rightTracked || leftTracked;

            // Detect pinch input from configured finger with relaxed detection
            bool rightHandActive = rightTracked && IsFingerPinching(rightOVRHand, pinchFinger);
            bool leftHandActive = leftTracked && IsFingerPinching(leftOVRHand, pinchFinger);
            bool anyHandActive = rightHandActive || leftHandActive;

            // Get the appropriate finger tip transform based on pinch finger
            Transform fingerTipTransform = currentFingerTip;

            // For hand input detection, use the correct finger tip based on active hand
            Transform drawingTransform = null;
            if (rightHandActive && currentFingerTip != null)
                drawingTransform = currentFingerTip;
            else if (leftHandActive && currentFingerTip != null)
                drawingTransform = currentFingerTip;

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
                    if (anyHandActive && !wasHandPinching && drawingTransform != null)
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
                        // Continue drawing using the stored finger tip transform, even when not active
                        ContinueDrawing(activeDrawingTransform);
                    }
                }
                else
                {
                    // Continuous mode: hold to draw
                    if (anyHandActive && drawingTransform != null)
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
                wasHandPinching = anyHandActive;
                return;
            }

            // Reset toggle tracking when neither input is active
            wasControllerGripPressed = false;
            wasHandPinching = false;
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

            if (recognizer != null && currentGesture.Count >= minPoints)
            {
                var result = recognizer.Recognize(currentGesture);

                if (result.Score >= minScore)
                {
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
                    // Record failed gesture
                    GameStats.RecordGesture(result.Score, false);
                }
            }
            else
            {
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


        /// <summary>
        /// Gets the current shape GameObject for a specific projectile type.
        /// Used to display spell symbols on toys.
        /// </summary>
        public GameObject GetShapeForProjectileType(int projectileType)
        {
            // Find which shape is currently mapped to this projectile type
            foreach (var kvp in shapeToProjectileMap)
            {
                if (kvp.Value == projectileType)
                {
                    // Find the shape template by name
                    foreach (var shape in availableShapes)
                    {
                        if (shape.Name == kvp.Key)
                        {
                            return shape.BulletinDrawing;
                        }
                    }
                }
            }
            return null;
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
