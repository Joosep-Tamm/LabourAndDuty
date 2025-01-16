using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WeldLineSystem : MonoBehaviour
{
    [Header("Weld Line Settings")]
    [SerializeField] private Transform weldLineStart;
    [SerializeField] private Transform weldLineEnd;
    public float acceptableDistance = 0.02f; // How far from line is acceptable
    [SerializeField] private float minWeldSpeed = 0.1f; // Minimum speed to weld
    [SerializeField] private float maxWeldSpeed = 0.3f; // Maximum speed to weld
    [SerializeField] private int lineSegments = 100; // Number of segments for detailed welding

    [Header("Visual Feedback")]
    [SerializeField] private LineRenderer guideLine; // To show the path
    [SerializeField] private LineRenderer progressLine; // To show welding progress
    [SerializeField] private Material completedWeldMaterial;
    [SerializeField] private Color progressEmissionColor = new Color(1f, 0.5f, 0f) * 2f;
    [SerializeField] private float progressWidth = 0.012f;
    [SerializeField] private float guideWidth = 0.008f;

    private Vector3 lastWeldPosition;
    private List<Vector3> linePoints = new List<Vector3>();
    private List<bool> weldedSegments;
    private bool isWelding = false;
    private Welder activeWelder;

    private List<List<Vector3>> weldedSections = new List<List<Vector3>>(); // Store multiple separate welded sections
    private float lastWeldedPosition = -1f; // Store the last welded position as a percentage along the line
    private bool isStartingNewSection = true;

    void Start()
    {
        SetupLines();
        SetupMaterials();
    }

    private void SetupLines()
    {
        // Initialize points along the line
        linePoints.Clear();
        for (int i = 0; i < lineSegments; i++)
        {
            float t = i / (float)(lineSegments - 1);
            Vector3 point = Vector3.Lerp(weldLineStart.position, weldLineEnd.position, t);
            linePoints.Add(point);
        }

        weldedSegments = new List<bool>(new bool[lineSegments - 1]);

        // Setup initial guide line
        guideLine.positionCount = lineSegments;
        guideLine.SetPositions(linePoints.ToArray());
        guideLine.startWidth = guideWidth;
        guideLine.endWidth = guideWidth;

        // Setup initial progress line (empty)
        progressLine.positionCount = 0;
        progressLine.startWidth = progressWidth;
        progressLine.endWidth = progressWidth;
    }

    private void SetupMaterials()
    {
        // Setup progress line material (emissive)
        Material progressMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        progressMat.EnableKeyword("_EMISSION");
        progressMat.SetColor("_EmissionColor", progressEmissionColor);
        progressMat.SetColor("_BaseColor", Color.black);
        progressLine.material = progressMat;

        // Setup guide line material
        //Material guideMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        //guideMat.SetColor("_BaseColor", new Color(0.5f, 0.5f, 1f, 0.5f));
        //guideLine.material = guideMat;
    }

    void Update()
    {
        if (isWelding && activeWelder != null)
        {
            ProcessWelding();
        }
    }

    private void ProcessWelding()
    {
        Vector3 weldPoint = activeWelder.GetWeldPoint();
        float distanceToLine = GetDistanceToLine(weldPoint);

        if (distanceToLine <= acceptableDistance)
        {
            float speed = Vector3.Distance(weldPoint, lastWeldPosition) / Time.deltaTime;

            if (speed >= minWeldSpeed && speed <= maxWeldSpeed)
            {
                Vector3 projectedPoint = ProjectPointOnLine(weldPoint);
                UpdateWeldProgress(projectedPoint);
            }
        }
        else
        {
            isStartingNewSection = true;
        }

        lastWeldPosition = weldPoint;
    }

    private void UpdateWeldProgress(Vector3 weldPoint)
    {
        int closestSegmentIndex = FindClosestSegment(weldPoint);
        if (closestSegmentIndex < 0) return;

        // Start new section if needed
        if (isStartingNewSection)
        {
            weldedSections.Add(new List<Vector3>());
            isStartingNewSection = false;
        }

        // Add point to current section
        List<Vector3> currentSection = weldedSections[weldedSections.Count - 1];
        currentSection.Add(weldPoint);

        // Mark segment as welded
        weldedSegments[closestSegmentIndex] = true;

        UpdateLineRenderers();
    }

    private float GetProgressPercentage(Vector3 point)
    {
        Vector3 lineVector = weldLineEnd.position - weldLineStart.position;
        Vector3 pointVector = point - weldLineStart.position;
        return Vector3.Dot(pointVector, lineVector.normalized) / lineVector.magnitude;
    }

    private void UpdateWeldProgress(Vector3 weldPoint, float currentPosition)
    {
        int closestSegment = FindClosestSegment(weldPoint);
        if (closestSegment >= 0 && closestSegment < weldedSegments.Count)
        {
            weldedSegments[closestSegment] = true;
            UpdateLineRenderers();
        }
    }

    private void UpdateLineRenderers()
    {
        // Update guide line - only show unwelded segments
        List<Vector3> remainingGuidePoints = new List<Vector3>();

        for (int i = 0; i < linePoints.Count - 1; i++)
        {
            if (!weldedSegments[i])
            {
                remainingGuidePoints.Add(linePoints[i]);
                remainingGuidePoints.Add(linePoints[i + 1]);
            }
        }

        guideLine.positionCount = remainingGuidePoints.Count;
        guideLine.SetPositions(remainingGuidePoints.ToArray());

        // Update progress line - show all welded sections
        int totalProgressPoints = 0;
        foreach (var section in weldedSections)
        {
            totalProgressPoints += section.Count;
        }

        Vector3[] progressPoints = new Vector3[totalProgressPoints];
        int currentIndex = 0;

        foreach (var section in weldedSections)
        {
            foreach (var point in section)
            {
                progressPoints[currentIndex] = point;
                currentIndex++;
            }
        }

        progressLine.positionCount = totalProgressPoints;
        progressLine.SetPositions(progressPoints);
    }

    private int FindClosestSegment(Vector3 point)
    {
        float minDistance = float.MaxValue;
        int closestSegment = -1;

        for (int i = 0; i < linePoints.Count - 1; i++)
        {
            if (weldedSegments[i]) continue; // Skip already welded segments

            Vector3 segmentStart = linePoints[i];
            Vector3 segmentEnd = linePoints[i + 1];

            float distance = DistanceToLineSegment(point, segmentStart, segmentEnd);
            if (distance < minDistance && distance < acceptableDistance)
            {
                minDistance = distance;
                closestSegment = i;
            }
        }

        return closestSegment;
    }

    private float DistanceToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 line = lineEnd - lineStart;
        Vector3 pointToStart = point - lineStart;
        float length = line.magnitude;
        Vector3 lineDirection = line / length;

        float dot = Vector3.Dot(pointToStart, lineDirection);

        if (dot < 0)
            return Vector3.Distance(point, lineStart);
        if (dot > length)
            return Vector3.Distance(point, lineEnd);

        Vector3 projection = lineStart + lineDirection * dot;
        return Vector3.Distance(point, projection);
    }

    public float GetDistanceToLine(Vector3 point)
    {
        Vector3 lineDirection = (weldLineEnd.position - weldLineStart.position).normalized;
        Vector3 pointVector = point - weldLineStart.position;
        Vector3 projection = Vector3.Project(pointVector, lineDirection);
        return Vector3.Distance(pointVector, projection);
    }

    private Vector3 ProjectPointOnLine(Vector3 point)
    {
        Vector3 lineDirection = (weldLineEnd.position - weldLineStart.position).normalized;
        Vector3 pointVector = point - weldLineStart.position;
        float dotProduct = Vector3.Dot(pointVector, lineDirection);
        return weldLineStart.position + lineDirection * dotProduct;
    }

    // Call this when the welder is activated
    public void StartWelding(Welder welder)
    {
        Debug.Log("Started welding");
        activeWelder = welder;
        isWelding = true;
        isStartingNewSection = true;
        lastWeldPosition = welder.GetWeldPoint();
    }

    // Call this when the welder is deactivated
    public void StopWelding(Welder welder)
    {
        if (activeWelder == welder)
        {
            Debug.Log("Stopped welding");
            isWelding = false;
            activeWelder = null;
            isStartingNewSection = true;
        }
    }
}