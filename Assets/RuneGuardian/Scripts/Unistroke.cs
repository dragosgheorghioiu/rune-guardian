using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGuardian
{
    public class Unistroke
    {
        // Constants
        private const int NumPoints = 64;
        private const float SquareSize = 250f;
        private const float AngleRange = 45f;
        private const float AnglePrecision = 2f;
        private static readonly float Phi = 0.5f * (-1f + Mathf.Sqrt(5f));

        // Template storage
        private List<GestureTemplate> Templates = new List<GestureTemplate>();

        /// <summary>
        /// Represents a gesture template
        /// </summary>
        public class GestureTemplate
        {
            public string Name;
            public List<Vector2> Points;

            public GestureTemplate(string name, List<Vector2> points)
            {
                Name = name;
                Points = points;
            }
        }

        /// <summary>
        /// Recognition result
        /// </summary>
        public class RecognitionResult
        {
            public string Name;
            public float Score;
            public float Angle;
            public int TemplateIndex;

            public RecognitionResult(string name, float score, float angle = 0f, int templateIndex = -1)
            {
                Name = name;
                Score = score;
                Angle = angle;
                TemplateIndex = templateIndex;
            }
        }

        /// <summary>
        /// Add a gesture template to the recognizer
        /// </summary>
        public void AddTemplate(string name, List<Vector2> points)
        {
            // Preprocess the template
            points = Resample(points, NumPoints);
            points = RotateToZero(points);
            points = ScaleToSquare(points, SquareSize);
            points = TranslateToOrigin(points);

            // Find if this shape name already exists and append variant number
            string templateName = name;
            int variantCount = 0;
            foreach (var template in Templates)
            {
                if (template.Name.StartsWith(name))
                {
                    variantCount++;
                }
            }

            if (variantCount > 0)
            {
                templateName = $"{name}_{variantCount + 1}";
            }

            Templates.Add(new GestureTemplate(templateName, points));
        }

        /// <summary>
        /// Add a gesture template with a custom variant name
        /// </summary>
        public void AddTemplateVariant(string shapeName, string variantName, List<Vector2> points)
        {
            // Preprocess the template
            points = Resample(points, NumPoints);
            points = RotateToZero(points);
            points = ScaleToSquare(points, SquareSize);
            points = TranslateToOrigin(points);

            string templateName = $"{shapeName}_{variantName}";
            Templates.Add(new GestureTemplate(templateName, points));
        }

        /// <summary>
        /// Recognize a gesture from input points
        /// </summary>
        public RecognitionResult Recognize(List<Vector2> points)
        {
            if (Templates.Count == 0)
            {
                return new RecognitionResult("No templates", 0f);
            }

            // Preprocess the candidate gesture
            points = Resample(points, NumPoints);
            points = RotateToZero(points);
            points = ScaleToSquare(points, SquareSize);
            points = TranslateToOrigin(points);

            // Find the best match
            float bestDistance = float.MaxValue;
            GestureTemplate bestTemplate = null;
            int bestTemplateIndex = -1;

            for (int i = 0; i < Templates.Count; i++)
            {
                var template = Templates[i];
                float distance = DistanceAtBestAngle(points, template.Points,
                    -AngleRange, AngleRange, AnglePrecision);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTemplate = template;
                    bestTemplateIndex = i;
                }
            }

            // Calculate score (0 to 1, where 1 is perfect match)
            float halfDiagonal = 0.5f * Mathf.Sqrt(SquareSize * SquareSize + SquareSize * SquareSize);
            float score = 1f - (bestDistance / halfDiagonal);

            // Extract base shape name (everything before underscore, if present)
            string shapeName = bestTemplate.Name.Contains("_")
                ? bestTemplate.Name.Substring(0, bestTemplate.Name.IndexOf("_"))
                : bestTemplate.Name;

            return new RecognitionResult(shapeName, score, 0f, bestTemplateIndex);
        }

        /// <summary>
        /// Clear all templates
        /// </summary>
        public void ClearTemplates()
        {
            Templates.Clear();
        }

        // ===== STEP 1: RESAMPLE =====
        private List<Vector2> Resample(List<Vector2> points, int n)
        {
            float I = PathLength(points) / (n - 1);
            float D = 0;
            List<Vector2> newPoints = new List<Vector2> { points[0] };

            for (int i = 1; i < points.Count; i++)
            {
                float d = Vector2.Distance(points[i - 1], points[i]);

                if (D + d >= I)
                {
                    float qx = points[i - 1].x + ((I - D) / d) * (points[i].x - points[i - 1].x);
                    float qy = points[i - 1].y + ((I - D) / d) * (points[i].y - points[i - 1].y);
                    Vector2 q = new Vector2(qx, qy);
                    newPoints.Add(q);
                    points.Insert(i, q);
                    D = 0;
                }
                else
                {
                    D += d;
                }
            }

            // Sometimes we fall a rounding-error short of adding the last point, so add it if so
            if (newPoints.Count == n - 1)
            {
                newPoints.Add(points[points.Count - 1]);
            }

            return newPoints;
        }

        private float PathLength(List<Vector2> points)
        {
            float d = 0;
            for (int i = 1; i < points.Count; i++)
            {
                d += Vector2.Distance(points[i - 1], points[i]);
            }
            return d;
        }

        // ===== STEP 2: ROTATE TO ZERO =====
        private List<Vector2> RotateToZero(List<Vector2> points)
        {
            Vector2 c = Centroid(points);
            float theta = Mathf.Atan2(c.y - points[0].y, c.x - points[0].x);
            return RotateBy(points, -theta);
        }

        private List<Vector2> RotateBy(List<Vector2> points, float theta)
        {
            Vector2 c = Centroid(points);
            float cos = Mathf.Cos(theta);
            float sin = Mathf.Sin(theta);
            List<Vector2> newPoints = new List<Vector2>();

            foreach (var p in points)
            {
                float qx = (p.x - c.x) * cos - (p.y - c.y) * sin + c.x;
                float qy = (p.x - c.x) * sin + (p.y - c.y) * cos + c.y;
                newPoints.Add(new Vector2(qx, qy));
            }

            return newPoints;
        }

        private Vector2 Centroid(List<Vector2> points)
        {
            float x = 0, y = 0;
            foreach (var p in points)
            {
                x += p.x;
                y += p.y;
            }
            return new Vector2(x / points.Count, y / points.Count);
        }

        // ===== STEP 3: SCALE AND TRANSLATE =====
        private List<Vector2> ScaleToSquare(List<Vector2> points, float size)
        {
            Rect boundingBox = BoundingBox(points);
            List<Vector2> newPoints = new List<Vector2>();

            foreach (var p in points)
            {
                float qx = p.x * (size / boundingBox.width);
                float qy = p.y * (size / boundingBox.height);
                newPoints.Add(new Vector2(qx, qy));
            }

            return newPoints;
        }

        private List<Vector2> TranslateToOrigin(List<Vector2> points)
        {
            Vector2 c = Centroid(points);
            List<Vector2> newPoints = new List<Vector2>();

            foreach (var p in points)
            {
                newPoints.Add(new Vector2(p.x - c.x, p.y - c.y));
            }

            return newPoints;
        }

        private Rect BoundingBox(List<Vector2> points)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var p in points)
            {
                minX = Mathf.Min(minX, p.x);
                maxX = Mathf.Max(maxX, p.x);
                minY = Mathf.Min(minY, p.y);
                maxY = Mathf.Max(maxY, p.y);
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        // ===== STEP 4: RECOGNIZE =====
        private float DistanceAtBestAngle(List<Vector2> points, List<Vector2> template,
            float thetaA, float thetaB, float thetaDelta)
        {
            // Golden Section Search
            float x1 = Phi * thetaA + (1 - Phi) * thetaB;
            float f1 = DistanceAtAngle(points, template, x1);
            float x2 = (1 - Phi) * thetaA + Phi * thetaB;
            float f2 = DistanceAtAngle(points, template, x2);

            while (Mathf.Abs(thetaB - thetaA) > thetaDelta)
            {
                if (f1 < f2)
                {
                    thetaB = x2;
                    x2 = x1;
                    f2 = f1;
                    x1 = Phi * thetaA + (1 - Phi) * thetaB;
                    f1 = DistanceAtAngle(points, template, x1);
                }
                else
                {
                    thetaA = x1;
                    x1 = x2;
                    f1 = f2;
                    x2 = (1 - Phi) * thetaA + Phi * thetaB;
                    f2 = DistanceAtAngle(points, template, x2);
                }
            }

            return Mathf.Min(f1, f2);
        }

        private float DistanceAtAngle(List<Vector2> points, List<Vector2> template, float theta)
        {
            List<Vector2> newPoints = RotateBy(points, theta * Mathf.Deg2Rad);
            return PathDistance(newPoints, template);
        }

        private float PathDistance(List<Vector2> pointsA, List<Vector2> pointsB)
        {
            float d = 0;
            for (int i = 0; i < pointsA.Count; i++)
            {
                d += Vector2.Distance(pointsA[i], pointsB[i]);
            }
            return d / pointsA.Count;
        }
    }
}