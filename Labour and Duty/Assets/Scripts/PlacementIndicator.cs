using UnityEngine;

public class PlacementIndicator : MonoBehaviour
{
    private MeshRenderer indicatorRenderer;
    private bool isObjectPlaced = false;

    // Optional: Add pulsing effect parameters
    public float pulseSpeed = 2f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 0.6f;
    private Material indicatorMaterial;

    void Start()
    {
        indicatorRenderer = GetComponent<MeshRenderer>();
        indicatorMaterial = indicatorRenderer.material;
    }

    void Update()
    {
        if (!isObjectPlaced)
        {
            // Create a pulsing effect
            float alpha = Mathf.Lerp(minAlpha, maxAlpha,
                (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            Color color = indicatorMaterial.color;
            color.a = alpha;
            indicatorMaterial.color = color;
        }
    }

    public void ObjectPlaced()
    {
        isObjectPlaced = true;
        indicatorRenderer.enabled = false;
    }

    public void ResetPlacement()
    {
        isObjectPlaced = false;
        indicatorRenderer.enabled = true;
    }
}