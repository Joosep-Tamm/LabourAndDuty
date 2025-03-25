using System.Collections.Generic;
using UnityEngine;

public class WeldPaintSystem : MonoBehaviour
{
    [Header("Basic Settings")]
    [SerializeField] private int textureSize = 512;
    [SerializeField] private float brushSize = 5f;
    [SerializeField] private Color weldColor = new Color(1f, 0.6f, 0f, 1f);
    [SerializeField] private float glowIntensity = 2f;

    [Header("Painting Volume")]
    [SerializeField] private float paintingHeight = 0.08f; // How far above/below the surface to detect
    [SerializeField] private BoxCollider paintingVolume; // Reference to the trigger volume

    [Header("References")]
    [SerializeField] private MeshRenderer weldSurfaceRenderer; // Reference to the quad's renderer

    [Header("Materials")]
    [SerializeField] private Material targetMaterial; // Material to paint on

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool logPaintPositions = true;

    [SerializeField] private WeldGuideSystem guideSystem;

    private RenderTexture weldTexture; // Where we paint
    private RenderTexture guideTexture; // Where the guide line is
    private Texture2D checkTexture; // For checking progress
    private bool isInitialized = false;

    private Texture2D circularBrush;

    private const float TOLERANCE_MULTIPLIER = 1f; // Increase checking area to account for sampling

    private bool canWeld = false;

    [Header("Progress Checking")]
    [SerializeField] private float progressCheckInterval = 0.5f; // Check every half second
    private float lastProgressCheckTime = 0f;
    private float cachedProgress = 0f;

    void Start()
    {
        // Try to find the renderer if not assigned
        if (weldSurfaceRenderer == null)
        {
            weldSurfaceRenderer = GetComponent<MeshRenderer>();
            if (weldSurfaceRenderer == null)
            {
                Debug.LogError("No MeshRenderer found! Please assign the weld surface renderer.");
                return;
            }
        }
        // Create unique material instances for this surface
        targetMaterial = new Material(targetMaterial);

        // Assign the materials to this instance's renderer
        Material[] materials = weldSurfaceRenderer.materials;
        // Update the appropriate material indices as needed
        materials[0] = targetMaterial;
        weldSurfaceRenderer.materials = materials;

        UpdateMaterialProperties();
        SetupPaintingVolume();
        InitializeTextures();
        CreateCircularBrush();

        canWeld = true;
    }

    private void CreateCircularBrush()
    {
        circularBrush = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        float center = 15.5f;

        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Max(0, 1 - (distance / center));
                // Smooth falloff
                alpha = Mathf.Pow(alpha, 2);
                circularBrush.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }

        circularBrush.Apply();
    }

    private void UpdateMaterialProperties()
    {
        targetMaterial.SetColor("_EmissionColor", weldColor * glowIntensity);
    }

    private void InitializeTextures()
    {
        Debug.Log("Initializing textures...");
        // Create the texture we'll paint on
        weldTexture = new RenderTexture(textureSize, textureSize, 0);
        weldTexture.enableRandomWrite = true;
        weldTexture.Create();
        Debug.Log($"Weld texture created: {weldTexture != null}");


        // Clear the textures to black
        RenderTexture.active = weldTexture;
        GL.Clear(true, true, Color.black);

        RenderTexture.active = guideTexture;
        GL.Clear(true, true, Color.black);

        // Assign textures to materials
        if (targetMaterial != null)
        {
            targetMaterial.SetTexture("_MainTex", weldTexture);
            Debug.Log("Target material texture assigned");
        }
        else
        {
            Debug.LogError("Target material is null!");
        }

        // Create texture for checking progress
        checkTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);

        isInitialized = true;
        Debug.Log("Initialization complete");
    }
    private void SetupPaintingVolume()
    {
        if (paintingVolume == null)
        {
            paintingVolume = gameObject.AddComponent<BoxCollider>();
        }

        Bounds worldBounds = weldSurfaceRenderer.bounds;

        Vector3 localSize = new Vector3(
            worldBounds.size.x / transform.lossyScale.x,
            worldBounds.size.y / transform.lossyScale.y,
            paintingHeight * 2f    // Since this is already in local space
        );

        Debug.Log($"World bounds size: {worldBounds.size}");
        Debug.Log($"Local scale: {transform.lossyScale}");
        Debug.Log($"Calculated local size: {localSize}");

        // Make the collider match the surface width/length but taller
        paintingVolume.size = localSize;
        paintingVolume.center = new Vector3(0, 0, 0); // Center it on the surface
        paintingVolume.isTrigger = true; // Make it a trigger

        Debug.Log($"Bounds: {paintingVolume.bounds}");
        Debug.Log($"Size: {paintingVolume.size}");
    }

    private Vector2 GetScaledBrushSize()
    {
        // Get both scale factors
        float scaleX = transform.localScale.y / 2;
        float scaleY = transform.localScale.x / 2;

        // Base brush size adjusted for each dimension
        float brushSizeX = brushSize * scaleX;
        float brushSizeY = brushSize * scaleY;

        // Clamp both dimensions
        brushSizeX = Mathf.Clamp(brushSizeX, 1f, textureSize * 0.25f);
        brushSizeY = Mathf.Clamp(brushSizeY, 1f, textureSize * 0.25f);

        return new Vector2(brushSizeX, brushSizeY);
    }

    public void PaintWeld(Vector3 worldPosition)
    {
        if (!isInitialized || !canWeld || !enabled || weldSurfaceRenderer == null)
        {
            //Debug.Log($"{isInitialized}, {canWeld}, {enabled}");
            return;
        }

        // Project the point onto the weld surface
        Vector3 projectedPoint = ProjectPointOntoSurface(worldPosition);
        Debug.Log("paintpoint: " + projectedPoint);

        // Check if point is within bounds
        Bounds volumeBounds = paintingVolume.bounds;
        if (!volumeBounds.Contains(worldPosition))
        {
            Debug.Log("paint spot out of bounds, point: " + worldPosition + ", bounds: " + volumeBounds);
            return; // Point is not within this surface's volume
        }

        // Convert world position to UV coordinates
        Vector2 uvPosition = WorldToTexturePosition(projectedPoint);

        // Clamp values but log if we're outside expected range
        if (uvPosition.x < 0 || uvPosition.x > textureSize ||
            uvPosition.y < 0 || uvPosition.y > textureSize)
        {
            Debug.LogWarning($"UV position out of range: {uvPosition}");
        }

        uvPosition.x = Mathf.Clamp(uvPosition.x, 0, textureSize);
        uvPosition.y = Mathf.Clamp(uvPosition.y, 0, textureSize);
        Debug.Log($"Painting at UV position: {uvPosition}");

        // Paint at the UV position
        RenderTexture.active = weldTexture;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, textureSize, textureSize, 0);

        /*
         * Graphics.DrawTexture(
            new Rect(uvPosition.x - brushSize / 2, uvPosition.y - brushSize / 2, brushSize, brushSize),
            Texture2D.whiteTexture,
            new Rect(0, 0, 1, 1), 0, 0, 0, 0, weldColor); 
        */

        Vector2 brushSize = GetScaledBrushSize();

        Rect brushRect = new Rect(
            uvPosition.x - brushSize.x / 2,
            (textureSize - uvPosition.y) - brushSize.y / 2,
            brushSize.x,
            brushSize.y
        );

        Graphics.DrawTexture(brushRect, circularBrush, new Rect(0, 0, 1, 1), 0, 0, 0, 0, weldColor);

        GL.PopMatrix();
        RenderTexture.active = null;
    }

    private Vector3 ProjectPointOntoSurface(Vector3 worldPoint)
    {
        // Get the surface plane
        Plane surface = new Plane(weldSurfaceRenderer.transform.forward, weldSurfaceRenderer.transform.position);

        // Project the point onto the plane
        float distance;
        surface.Raycast(new Ray(worldPoint, -surface.normal), out distance);
        return worldPoint - surface.normal * distance;
    }

    private Vector2 WorldToTexturePosition(Vector3 worldPos)
    {
        if (weldSurfaceRenderer == null) return Vector2.zero;

        // Get the plane's transform
        Transform planeTransform = weldSurfaceRenderer.transform;

        // Convert world position to local position on the surface
        Vector3 localPos = transform.InverseTransformPoint(worldPos);

        // Since a quad is 1x1 in local space, we can directly use localPos
        // No need to divide by quadSize as localPos is already in the -0.5 to 0.5 range
        float normalizedX = localPos.x;
        float normalizedY = localPos.y;

        // Convert to 0-1 UV space
        float u = normalizedX + 0.5f;
        float v = normalizedY + 0.5f;

        // Convert to texture space
        float texU = u * textureSize;
        float texV = v * textureSize;

        if (logPaintPositions)
        {
            Debug.Log($"World Pos: {worldPos}");
            Debug.Log($"Local Pos: {localPos}");
            Debug.Log($"Normalized Pos: ({normalizedX}, {normalizedY})");
            Debug.Log($"UV: ({u}, {v})");
            Debug.Log($"Texture Pos: ({texU}, {texV})");
            Debug.Log($"Texture Pos: ({texU}, {texV})");
        }

        return new Vector2(texU, texV);
    }

    private Vector3 TextureToWorldPosition(Vector2 uvPosition)
    {
        // Convert texture coordinates back to world position
        Vector3 localPos = new Vector3(
            (uvPosition.x - 0.5f) * weldSurfaceRenderer.bounds.size.x,
            (uvPosition.y - 0.5f) * weldSurfaceRenderer.bounds.size.y,
            0
        );
        return transform.TransformPoint(localPos);
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        if (weldSurfaceRenderer == null) return;
        // Draw the bounds of the weldable area
        Bounds bounds = weldSurfaceRenderer.bounds;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        // Draw the corners with labels
        Vector3[] corners = new Vector3[]
        {
            new Vector3(-0.5f, 0, -0.5f),
            new Vector3(0.5f, 0, -0.5f),
            new Vector3(-0.5f, 0, 0.5f),
            new Vector3(0.5f, 0, 0.5f)
        };

        foreach (var corner in corners)
        {
            Vector3 worldCorner = transform.TransformPoint(corner);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(worldCorner, 0.005f);
        }
    }

    public float CheckWeldProgress(float toleranceDistance = 0.02f)
    {
        if (Time.time - lastProgressCheckTime < progressCheckInterval)
        {
            return cachedProgress;
        }

        // Update last check time
        lastProgressCheckTime = Time.time;

        if (!isInitialized || guideSystem == null) return 0f;

        toleranceDistance *= TOLERANCE_MULTIPLIER;

        // Get the pixels from the weld texture
        RenderTexture.active = weldTexture;
        checkTexture.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
        checkTexture.Apply();

        Vector3[] linePoints = new Vector3[guideSystem.GetLineRenderer().positionCount];
        guideSystem.GetLineRenderer().GetPositions(linePoints);

        // Calculate appropriate number of samples based on line type and length
        int samples = CalculateSampleCount(linePoints);
        //Debug.Log("samples: " + samples);

        int correctPoints = 0;
        int totalPoints = 0;

        // Sample points along the line
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)(samples - 1);
            // Get point along line (either straight or curved)
            Vector2 centerPoint = GetPointAlongLine(linePoints, t);

            // Calculate perpendicular for width sampling
            Vector2 tangent = GetTangentAtPoint(linePoints, t);
            Vector2 perpendicular = new Vector2(-tangent.y, tangent.x).normalized;

            // Check points across width
            int widthPixels = Mathf.CeilToInt(toleranceDistance * textureSize);

            // Debug first and last points
            if (i == 0 || i == samples - 1)
            {
                //Debug.Log($"Checking point at t={t}: Center={centerPoint}");
            }

            for (int w = -widthPixels; w <= widthPixels; w++)
            {
                Vector2 samplePoint = centerPoint + perpendicular * w;

                int x = Mathf.RoundToInt(samplePoint.x);
                int y = Mathf.RoundToInt(samplePoint.y);

                if (x >= 0 && x < textureSize && y >= 0 && y < textureSize)
                {
                    totalPoints++;
                    Color pixel = checkTexture.GetPixel(x, y);

                    // Debug pixel values occasionally
                    if (i % (samples / 4) == 0 && w == 0)
                    {
                        //Debug.Log($"Pixel at ({x}, {y}): alpha = {pixel.a}, red = {pixel.r}");
                    }

                    if (pixel.r > 0.01f)
                    {
                        correctPoints++;
                    }
                }
            }
        }
        cachedProgress = totalPoints > 0 ? (float)correctPoints / totalPoints : 0f;
        return cachedProgress;
        //Debug.Log($"Weld check complete: {correctPoints}/{totalPoints} points correct = {progress * 100}%");
    }
    public void ResetProgressChecking()
    {
        lastProgressCheckTime = 0f;
        cachedProgress = 0f;
    }

    public float ForceProgressCheck(float toleranceDistance = 0.02f)
    {
        lastProgressCheckTime = 0f; // Reset the timer
        return CheckWeldProgress(toleranceDistance); // Force a new calculation
    }

    private int CalculateSampleCount(Vector3[] points)
    {
        float length;
        if (points.Length == 2)
        {
            // Straight line
            length = Vector3.Distance(points[0], points[1]);
        }
        else if (points.Length == 4)
        {
            // Bezier curve - approximate length
            length = EstimateBezierLength(points[0], points[1], points[2], points[3]);
        }
        else
        {
            Debug.LogError("Unexpected number of control points");
            return 10;
        }

        // Convert world length to texture space
        Vector2 startUV = WorldToTexturePosition(points[0]);
        Vector2 endUV = WorldToTexturePosition(points[points.Length - 1]);
        float textureLength = Vector2.Distance(startUV, endUV);

        // Use the larger of the two to ensure adequate sampling
        float finalLength = Mathf.Max(length * textureSize, textureLength);
        return Mathf.Max(10, Mathf.CeilToInt(finalLength));
    }

    private float EstimateBezierLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int segments = 10)
    {
        float length = 0;
        Vector3 previousPoint = p0;

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 point = CalculateBezierPoint(t, p0, p1, p2, p3);
            length += Vector3.Distance(previousPoint, point);
            previousPoint = point;
        }

        return length;
    }

    private Vector2 GetPointAlongLine(Vector3[] points, float t)
    {
        Vector3 worldPoint;
        if (points.Length == 2)
        {
            // Straight line
            worldPoint = Vector3.Lerp(points[0], points[1], t);
        }
        else
        {
            // Bezier curve
            worldPoint = CalculateBezierPoint(t, points[0], points[1], points[2], points[3]);
        }
        // Debug world to texture conversion
        Vector2 texturePoint = WorldToTexturePosition(worldPoint);
        if (t == 0 || t == 1)
        {
            //Debug.Log($"Converting world point {worldPoint} to texture point {texturePoint}");
        }
        return texturePoint;
    }

    private Vector2 GetTangentAtPoint(Vector3[] points, float t)
    {
        Vector3 worldTangent;
        if (points.Length == 2)
        {
            // Straight line - tangent is constant
            worldTangent = (points[1] - points[0]).normalized;
        }
        else
        {
            // Bezier curve - calculate tangent
            worldTangent = CalculateBezierTangent(t, points[0], points[1], points[2], points[3]);
        }

        // Convert to texture space direction
        Vector2 p1 = WorldToTexturePosition(points[0]);
        Vector2 p2 = WorldToTexturePosition(points[0] + worldTangent);
        return (p2 - p1).normalized;
    }

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

    private Vector3 CalculateBezierTangent(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float uu = u * u;
        float tt = t * t;

        Vector3 tangent = -3 * uu * p0;
        tangent += 3 * uu * p1 - 6 * u * t * p1;
        tangent += 6 * u * t * p2 - 3 * tt * p2;
        tangent += 3 * tt * p3;

        return tangent.normalized;
    }

    public void DisableWelding()
    {
        canWeld = false;
        //enabled = false;
        ResetProgressChecking();
        /*
        // Clear the weld texture when disabled
        if (weldTexture != null)
        {
            RenderTexture.active = weldTexture;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;
        }

        // Disable the painting volume collider
        if (paintingVolume != null)
        {
            paintingVolume.enabled = false;
        }
        */
    }

    public void EnableWelding()
    {
        canWeld = true;
        //enabled = true;
        ResetProgressChecking();
        // Re-enable the painting volume collider
        /*
        if (paintingVolume != null)
        {
            paintingVolume.enabled = true;
        }
        */
    }

    private void OnEnable()
    {
        canWeld = true;
        if (paintingVolume != null)
        {
            paintingVolume.enabled = true;
        }
    }

    private void OnDisable()
    {
        canWeld = false;
        if (paintingVolume != null)
        {
            paintingVolume.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (weldTexture != null) weldTexture.Release();
        if (checkTexture != null) Destroy(checkTexture);
    }
}