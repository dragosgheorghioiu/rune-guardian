using System;
using System.Collections.Generic;
using UnityEngine;
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

    [Header("Recognition Settings")]
    [SerializeField] private int minPoints = 10;
    [SerializeField] private float minScore = 0.5f;

    private GameObject lineObject;

    public ProjectileShooter shooter;
    public GameObject projectileTypeA;
    public GameObject projectileTypeB;

    void Start()
    {
        recognizer = new Unistroke();

        // Get main camera if not assigned
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Add some example templates
        AddCircleTemplate();
        AddSquareTemplate();
        AddTriangleTemplate();

        Debug.Log("Gesture Recognizer ready! Draw on screen to test.");
    }

    void Update()
    {
        // Start drawing on mouse down
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            currentGesture = new List<Vector2>();
            CreateLineRenderer();
        }

        // Collect points while drawing
        if (isDrawing && Input.GetMouseButton(0))
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
            Vector2 screenPos = Input.mousePosition;
            currentGesture.Add(screenPos);

            // Update visual feedback
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = currentGesture.Count;
                lineRenderer.SetPosition(currentGesture.Count - 1, worldPos);
            }
        }

        // Recognize gesture on mouse up
        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            isDrawing = false;
            
            if (currentGesture.Count >= minPoints)
            {
                var result = recognizer.Recognize(currentGesture);
                
                if (result.Score >= minScore)
                {
                    Debug.Log($"<color=green>✓ Recognized: {result.Name} (Score: {result.Score:F2})</color>");
                    if (result.Name.CompareTo("square") == 0) {
                        shooter.Shoot(projectileTypeA);
                    } else if (result.Name.CompareTo("circle") == 0)
                    {
                        shooter.Shoot(projectileTypeB);
                    }
                }
                else
                {
                    Debug.Log($"<color=yellow>? Gesture not recognized (Best: {result.Name}, Score: {result.Score:F2})</color>");
                }
            }
            else
            {
                Debug.Log($"<color=red>Gesture too short! Need at least {minPoints} points.</color>");
            }

            // Clean up line renderer after a delay
            if (lineObject != null)
                Destroy(lineObject, 0.5f);
        }
    }

    void CreateLineRenderer()
    {
        // Create a new GameObject for the line
        lineObject = new GameObject("GestureLine");
        lineRenderer = lineObject.AddComponent<LineRenderer>();
        
        // Configure line renderer
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
        List<Vector2> square = new List<Vector2>
        {
            new Vector2(0, 0), new Vector2(100, 0), new Vector2(100, 100), 
            new Vector2(0, 100), new Vector2(0, 0)
        };
        recognizer.AddTemplate("square", square);

        List<Vector2> square2 = new List<Vector2>
        {
            new Vector2(0, 0), new Vector2(0, 100), new Vector2(100, 100), 
            new Vector2(100, 0), new Vector2(0, 0)
        };
        recognizer.AddTemplate("square", square2);
    }

    void AddTriangleTemplate()
    {
        List<Vector2> triangle = new List<Vector2>
        {
            new Vector2(50, 0), new Vector2(100, 100), 
            new Vector2(0, 100), new Vector2(50, 0)
        };
        recognizer.AddTemplate("triangle", triangle);

        List<Vector2> triangle2 = new List<Vector2>
        {
            new Vector2(50, 0), new Vector2(0, 100), 
            new Vector2(100, 100), new Vector2(50, 0)
        };
        recognizer.AddTemplate("triangle", triangle2);
    }
}