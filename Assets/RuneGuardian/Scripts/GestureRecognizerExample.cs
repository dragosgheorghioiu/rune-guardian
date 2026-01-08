using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace RuneGuardian
{
    public class GestureRecognizerExample : MonoBehaviour
    {
        public static event Action<int> OnValidGesture;

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
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Color drawColor = Color.white;
        [SerializeField] private float lineWidth = 0.1f;

        [Header("VR Controller Settings")]
        [SerializeField] private Transform rightControllerTransform;
        [SerializeField] private Transform leftControllerTransform;
        [SerializeField] private bool useRightHand = true;

        [Header("Recognition Settings")]
        [SerializeField] private int minPoints = 10;
        [SerializeField] private float minScore = 0.5f;
        [SerializeField] private float minPointDistance = 0.01f; // Minimum distance between points

        [Header("Keyboard Fallback")]
        [SerializeField] private bool enableKeyboardFallback = true;
        [SerializeField] private KeyCode shootTypeAKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode shootTypeBKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode shootTypeTriangleKey = KeyCode.Alpha3;

        private GameObject lineObject;
        private Transform activeControllerTransform;
        private InputDevice activeDevice;
        private XRNode activeNode;
        private Vector3 lastRecordedPosition;

        /// <summary>
        /// Represents a shape template with its name and corresponding add function
        /// </summary>
        private class ShapeTemplate
        {
            public string Name;
            public Action AddTemplateAction;

            public ShapeTemplate(string name, Action addTemplateAction)
            {
                Name = name;
                AddTemplateAction = addTemplateAction;
            }
        }

        private List<ShapeTemplate> availableShapes;
        private List<ShapeTemplate> validShapes;


        void Init(InputData inputData)
        {
            if (inputData != null)
            {
                minScore = inputData.gestureMinScore;
            }

            recognizer = new Unistroke();
            availableShapes = new List<ShapeTemplate>
            {
                new("Circle", AddCircleTemplate),
                new("Square", AddSquareTemplate),
                new("Triangle", AddTriangleTemplate),
            };

            validShapes = new();
            validShapes.Add(availableShapes[inputData.dirtyObjectsDrawing]);
            validShapes.Add(availableShapes[inputData.destroyedObjectsDrawing]);
            validShapes.Add(availableShapes[inputData.uncoloredObjectsDrawing]);
            foreach (var shape in validShapes)
            {
                shape.AddTemplateAction();
            }

            Debug.Log("Gesture Recognizer initialized from RuneGuardianController!");

            mainCamera ??= Camera.main;

            if (rightControllerTransform == null || leftControllerTransform == null)
            {
                FindVRControllers();
            }

            activeNode = useRightHand ? XRNode.RightHand : XRNode.LeftHand;
            activeControllerTransform = useRightHand ? rightControllerTransform : leftControllerTransform;
            UpdateActiveDevice();

            Debug.Log("Gesture Recognizer ready! Use VR controller trigger to draw.");
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
                    OnValidGesture?.Invoke(0);
                }
                else if (Input.GetKeyDown(shootTypeBKey))
                {
                    OnValidGesture?.Invoke(1);
                }
                else if (Input.GetKeyDown(shootTypeTriangleKey))
                {
                    OnValidGesture?.Invoke(2);
                }
            }

            // VR Controller handling
            if (!activeDevice.isValid)
                UpdateActiveDevice();

            if (!activeDevice.isValid || activeControllerTransform == null)
                return;

            // Get grip button (side trigger) instead of main trigger to avoid jumps
            bool triggerPressed = false;
            if (activeDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gripButton))
            {
                triggerPressed = gripButton;
            }

            // Start drawing
            if (triggerPressed && !isDrawing)
            {
                StartDrawing();
            }
            // Continue drawing
            else if (triggerPressed && isDrawing)
            {
                ContinueDrawing();
            }
            // Stop drawing
            else if (!triggerPressed && isDrawing)
            {
                StopDrawing();
            }
        }

        void StartDrawing()
        {
            isDrawing = true;
            currentGesture = new List<Vector2>();
            CreateLineRenderer();
            lastRecordedPosition = activeControllerTransform.position;

            // Add first point
            AddPoint(activeControllerTransform.position);
            Debug.Log("Started drawing gesture");
        }

        void ContinueDrawing()
        {
            Vector3 currentPos = activeControllerTransform.position;

            // Only add point if controller has moved enough
            if (Vector3.Distance(currentPos, lastRecordedPosition) >= minPointDistance)
            {
                AddPoint(currentPos);
                lastRecordedPosition = currentPos;
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
            Debug.Log($"Stopped drawing. Points collected: {currentGesture.Count}");

            if (currentGesture.Count >= minPoints)
            {
                var result = recognizer.Recognize(currentGesture);

                if (result.Score >= minScore)
                {
                    Debug.Log($"✓ Recognized: {result.Name} (Score: {result.Score:F2})");
                    OnValidGesture?.Invoke(result.TemplateIndex);
                }
                else
                {
                    Debug.Log($"? Gesture not recognized (Best: {result.Name}, Score: {result.Score:F2})");
                }
            }
            else
            {
                Debug.Log($"Gesture too short! Need at least {minPoints} points, got {currentGesture.Count}");
            }

            // Clean up line renderer
            if (lineObject != null)
                Destroy(lineObject, 0.5f);
        }


        // TODO(dragos): maybe add a sparkly effect when drawing
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

        void AddCircleTemplate()
        {
            List<Vector2> circle = new List<Vector2>();
            int numPoints = 32;
            for (int i = 0; i < numPoints; i++)
            {
                float angle = i * 2f * Mathf.PI / numPoints;
                circle.Add(new Vector2(Mathf.Cos(angle) * 100f, Mathf.Sin(angle) * 100f));
            }
            recognizer.AddTemplate("circle", circle);
        }

        void AddSquareTemplate()
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
            recognizer.AddTemplate("square", square1);

            // Counter-clockwise square
            List<Vector2> square2 = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(0, 100),
            new Vector2(100, 100),
            new Vector2(100, 0),
            new Vector2(0, 0)
        };
            recognizer.AddTemplate("square", square2);
        }

        void AddTriangleTemplate()
        {
            // Clockwise triangle
            List<Vector2> triangle1 = new List<Vector2>
        {
            new Vector2(50, 0),
            new Vector2(100, 100),
            new Vector2(0, 100),
            new Vector2(50, 0)
        };
            recognizer.AddTemplate("triangle", triangle1);

            // Counter-clockwise triangle
            List<Vector2> triangle2 = new List<Vector2>
        {
            new Vector2(50, 0),
            new Vector2(0, 100),
            new Vector2(100, 100),
            new Vector2(50, 0)
        };
            recognizer.AddTemplate("triangle", triangle2);
        }

    }
}