using UnityEngine;

public class WeldPaintSystem : MonoBehaviour
{
    [Header("Basic Settings")]
    [SerializeField] private int textureSize = 512;
    [SerializeField] private float brushSize = 5f;
    [SerializeField] private Color guideColor = new Color(0.5f, 0.5f, 1f, 0.5f);
    [SerializeField] private Color weldColor = new Color(1f, 0.6f, 0f, 1f);
    [SerializeField] private float glowIntensity = 2f;

    [Header("Painting Volume")]
    [SerializeField] private float paintingHeight = 0.08f; // How far above/below the surface to detect
    [SerializeField] private BoxCollider paintingVolume; // Reference to the trigger volume

    [Header("References")]
    [SerializeField] private MeshRenderer weldSurfaceRenderer; // Reference to the quad's renderer

    [Header("Materials")]
    [SerializeField] private Material targetMaterial; // Material to paint on
    [SerializeField] private Material guideMaterial; // Material showing where to weld

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool logPaintPositions = true;
    [SerializeField] private bool drawTestGuideLine = true;

    private RenderTexture weldTexture; // Where we paint
    private RenderTexture guideTexture; // Where the guide line is
    private Texture2D checkTexture; // For checking progress
    private bool isInitialized = false;

    void Start()
    {
        // Try to find the renderer if not assigned
        if (weldSurfaceRenderer == null)
        {
            weldSurfaceRenderer = GetComponentInChildren<MeshRenderer>();
            if (weldSurfaceRenderer == null)
            {
                Debug.LogError("No MeshRenderer found in children! Please assign the weld surface renderer.");
                return;
            }
        }
        Debug.Log("WeldPaintSystem Starting...");
        UpdateMaterialProperties();
        SetupPaintingVolume();
        InitializeTextures();
    }

    private void UpdateMaterialProperties()
    {
        targetMaterial.SetColor("_EmissionColor", weldColor * glowIntensity);
        guideMaterial.SetColor("_EmissionColor", guideColor);
    }

    private void InitializeTextures()
    {
        Debug.Log("Initializing textures...");
        // Create the texture we'll paint on
        weldTexture = new RenderTexture(textureSize, textureSize, 0);
        weldTexture.enableRandomWrite = true;
        weldTexture.Create();
        Debug.Log($"Weld texture created: {weldTexture != null}");

        // Create the guide texture
        guideTexture = new RenderTexture(textureSize, textureSize, 0);
        guideTexture.enableRandomWrite = true;
        guideTexture.Create();
        Debug.Log($"Guide texture created: {guideTexture != null}");

        // Clear the textures to black
        RenderTexture.active = weldTexture;
        GL.Clear(true, true, Color.black);

        RenderTexture.active = guideTexture;
        GL.Clear(true, true, Color.black);

        // Draw guide line
        if (drawTestGuideLine)
        {
            DrawGuideLine();
        }

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

        if (guideMaterial != null)
        {
            guideMaterial.SetTexture("_MainTex", guideTexture);
            Debug.Log("Guide material texture assigned");
        }
        else
        {
            Debug.LogError("Guide material is null!");
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

        // Get the size of the weld surface
        Vector3 surfaceSize = weldSurfaceRenderer.bounds.size;

        // Make the collider match the surface width/length but taller
        paintingVolume.size = new Vector3(surfaceSize.x, surfaceSize.y, paintingHeight * 2f);
        paintingVolume.center = new Vector3(0, 0, 0); // Center it on the surface
        paintingVolume.isTrigger = true; // Make it a trigger
    }

    private void DrawGuideLine()
    {
        Debug.Log("Drawing guide line...");
        // Draw your guide line here
        RenderTexture.active = guideTexture;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, textureSize, textureSize, 0);

        // Example: Draw a line from left to right
        // Modify this to match your desired weld path

        // Graphics.DrawTexture(new Rect(0, textureSize / 2 - 2, textureSize, 4), Texture2D.whiteTexture);

        int lineWidth = 4; // Thicker line
        Rect lineRect = new Rect(0, textureSize / 2 - lineWidth / 2, textureSize, lineWidth);
        Graphics.DrawTexture(lineRect, Texture2D.whiteTexture, new Rect(0, 0, 1, 1), 0, 0, 0, 0, guideColor);

        GL.PopMatrix();
        RenderTexture.active = null;
        Debug.Log("Guide line drawn");
    }

    public void PaintWeld(Vector3 worldPosition)
    {
        if (!isInitialized || weldSurfaceRenderer == null) return;

        // Project the point onto the weld surface
        Vector3 projectedPoint = ProjectPointOntoSurface(worldPosition);


        // Check if point is within bounds
        /*Bounds bounds = weldSurfaceRenderer.bounds;
        if (!bounds.Contains(projectedPoint))
        {
            if (logPaintPositions)
            {
                Debug.Log($"Point outside bounds: {worldPosition}");
                Debug.Log($"Bounds: {bounds.min} to {bounds.max}");
            }
            return;
        }*/

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

        Rect brushRect = new Rect(
            uvPosition.x - brushSize / 2,
            (textureSize - uvPosition.y) - brushSize / 2,
            brushSize,
            brushSize
        );

        Graphics.DrawTexture(brushRect, Texture2D.whiteTexture, new Rect(0, 0, 1, 1), 0, 0, 0, 0, weldColor);

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

        // Get the actual size of the quad
        Vector3 quadSize = weldSurfaceRenderer.bounds.size;

        // Normalize coordinates to -0.5 to 0.5 range based on quad size
        float normalizedX = localPos.x / quadSize.x;
        float normalizedY = localPos.y / quadSize.y;

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
            Debug.Log($"Quad Size: {quadSize}");
        }

        return new Vector2(texU, texV);
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

    public float CheckWeldProgress()
    {
        RenderTexture.active = weldTexture;
        checkTexture.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
        checkTexture.Apply();

        RenderTexture.active = guideTexture;
        Texture2D guideCheck = new Texture2D(textureSize, textureSize);
        guideCheck.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
        guideCheck.Apply();

        int correctPixels = 0;
        int totalGuidePixels = 0;

        Color[] weldPixels = checkTexture.GetPixels();
        Color[] guidePixels = guideCheck.GetPixels();

        for (int i = 0; i < weldPixels.Length; i++)
        {
            if (guidePixels[i].r > 0.5f) // If this is part of the guide line
            {
                totalGuidePixels++;
                if (weldPixels[i].r > 0.5f) // If this has been welded
                {
                    correctPixels++;
                }
            }
        }

        Destroy(guideCheck);
        return (float)correctPixels / totalGuidePixels;
    }

    private void OnDestroy()
    {
        if (weldTexture != null) weldTexture.Release();
        if (guideTexture != null) guideTexture.Release();
        if (checkTexture != null) Destroy(checkTexture);
    }
}