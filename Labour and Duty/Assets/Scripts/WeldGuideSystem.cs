using UnityEngine;

public class WeldGuideSystem : MonoBehaviour
{
    [SerializeField] private LineRenderer guideLine;
    [SerializeField] private Color startColor = new Color(0.5f, 0.5f, 1f, 0.5f);
    [SerializeField] private Color glowColor = Color.green;
    [SerializeField] private float readyGlowIntensity = 4.0f;
    [SerializeField] private float lineWidth = 0.01f;
    [SerializeField] private Vector3 startPoint;
    [SerializeField] private Vector3 endPoint;
    [SerializeField] private Material baseMaterial;

    private Vector3[] linePoints;
    private float timeElapsed = 0f;
    private float timeToHit = 0f;
    private Material materialInstance;
    private bool hasReachedTarget = false;
    private bool isInitialized = false;

    private void Awake()
    {
        if (guideLine == null)
        {
            guideLine = gameObject.AddComponent<LineRenderer>();
        }
        SetupGuideLine();
        isInitialized = true;
    }

    private void Start()
    {
        SetStraightLine(startPoint, endPoint);
    }

    private void SetupGuideLine()
    {
        // Create instance of the same material used by indicators
        materialInstance = new Material(baseMaterial);
        guideLine.material = materialInstance;
        //Debug.Log("Material isntance created");

        ResetGuideLine();

        guideLine.startWidth = lineWidth;
        guideLine.endWidth = lineWidth;
        guideLine.useWorldSpace = true;
    }

    private void ResetGuideLine()
    {
        if (materialInstance == null) return;
        timeElapsed = 0f;
        hasReachedTarget = false;
        materialInstance.DisableKeyword("_EMISSION");
        materialInstance.color = startColor;
        materialInstance.SetColor("_EmissionColor", Color.black);
        guideLine.startColor = startColor;
        guideLine.endColor = startColor;
    }

    private void Update()
    {
        if (guideLine.enabled)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= timeToHit)
            {
                if (!hasReachedTarget)
                {
                    hasReachedTarget = true;
                    ShowTargetReachedEffect();
                }
            }
            else
            {
                // Gradually fade the line color
                float t = timeElapsed / timeToHit;
                Color currentColor = Color.Lerp(startColor, glowColor, t);
                materialInstance.color = currentColor;
                materialInstance.SetColor("_EmissionColor", currentColor);
                guideLine.startColor = currentColor;
                guideLine.endColor = currentColor;
            }
        }
    }

    private void ShowTargetReachedEffect()
    {
        materialInstance.EnableKeyword("_EMISSION");
        materialInstance.SetColor("_EmissionColor", glowColor * readyGlowIntensity);
        guideLine.startColor = glowColor;
        guideLine.endColor = glowColor;
        materialInstance.color = glowColor;
        guideLine.startColor = glowColor;
        guideLine.endColor = glowColor;
    }

    public void SetTimeToHit(float time)
    {
        timeToHit = time;
        if (isInitialized)
        {
            ResetGuideLine();
        }
    }

    public void SetStraightLine(Vector3 start, Vector3 end)
    {
        guideLine.positionCount = 2;
        guideLine.SetPosition(0, start + transform.position);
        guideLine.SetPosition(1, end + transform.position);
    }

    public void SetCurvedLine(Vector3[] points)
    {
        guideLine.positionCount = points.Length;
        guideLine.SetPositions(points);
    }

    // This method was created with the help of Claude Sonnet LLM
    public void SetBezierCurve(Vector3 start, Vector3 control1, Vector3 control2, Vector3 end, int segments = 30)
    {
        linePoints = new Vector3[segments];
        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            linePoints[i] = CalculateBezierPoint(t, start, control1, control2, end);
        }

        guideLine.positionCount = segments;
        guideLine.SetPositions(linePoints);
    }
    // This method was created with the help of Claude Sonnet LLM
    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * p0;
        point += 3 * uu * t * p1;
        point += 3 * u * tt * p2;
        point += ttt * p3;

        return point;
    }

    public void ShowGuideLine()
    {
        guideLine.enabled = true;
        ResetGuideLine();
    }

    public void HideGuideLine()
    {
        guideLine.enabled = false;
    }

    public LineRenderer GetLineRenderer()
    {
        return guideLine;
    }

    private void OnDestroy()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}