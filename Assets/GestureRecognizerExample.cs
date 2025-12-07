using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace RuneGuardian
{
    public class GestureRecognizerExample : MonoBehaviour
    {
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

        public ProjectileShooter shooter;

        void Start()
        {
            recognizer = new Unistroke();

            if (mainCamera == null)
                mainCamera = Camera.main;

            if (rightControllerTransform == null || leftControllerTransform == null)
            {
                FindVRControllers();
            }

            activeNode = useRightHand ? XRNode.RightHand : XRNode.LeftHand;
            activeControllerTransform = useRightHand ? rightControllerTransform : leftControllerTransform;
            UpdateActiveDevice();

            AddCircleTemplate();
            AddSquareTemplate();
            AddTriangleTemplate();

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
                    Debug.Log("Keyboard: Shooting Type A (Square)");
                    OnGestureRecognized("square");
                }
                else if (Input.GetKeyDown(shootTypeBKey))
                {
                    Debug.Log("Keyboard: Shooting Type B (Circle)");
                    OnGestureRecognized("circle");
                }
                else if (Input.GetKeyDown(shootTypeTriangleKey))
                {
                    Debug.Log("Keyboard: Triangle gesture");
                    OnGestureRecognized("triangle");
                }
            }

            // VR Controller handling
            if (!activeDevice.isValid)
                UpdateActiveDevice();

            if (!activeDevice.isValid || activeControllerTransform == null)
                return;

            // Get trigger value (0.0 to 1.0)
            bool triggerPressed = false;
            if (activeDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerButton))
            {
                triggerPressed = triggerButton;
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
                    OnGestureRecognized(result.Name);
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

        void OnGestureRecognized(string gestureName)
        {
            if (shooter == null) return;

            if (gestureName == "square")
            {
                shooter.Shoot(shooter.projectileTypeA);
            }
            else if (gestureName == "circle")
            {
                shooter.Shoot(shooter.projectileTypeB);
            }
            else if (gestureName == "triangle")
            {
                Debug.Log("Triangle recognized! (No projectile assigned)");
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