using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator : MonoBehaviour
{
    public Material baseMaterial;
    public float glowIntensity = 2.0f;
    public Color glowColor = Color.green;

    private float timeElapsed = 0f;

    private bool hasReachedTarget = false;
    private float currentScale = 0f;
    public float targetScale = 1f;

    public float timeToHit;

    private Material materialInstance;
    // Start is called before the first frame update
    void Start()
    {
        InitializeIndicator();
    }

    private void OnEnable()
    {
        InitializeIndicator();
    }

    private void InitializeIndicator()
    {
        // Only create new material instance if we don't have one
        if (materialInstance == null)
        {
            materialInstance = new Material(baseMaterial);
            GetComponent<Renderer>().material = materialInstance;
        }
        ResetIndicator();
    }

    private void ResetIndicator()
    {
        timeElapsed = 0f;
        currentScale = 0f;
        hasReachedTarget = false;
        transform.localScale = Vector3.one * currentScale;

        // Reset material properties
        materialInstance.DisableKeyword("_EMISSION");
        materialInstance.color = baseMaterial.color;
        materialInstance.SetColor("_EmissionColor", Color.black);
    }

    // Update is called once per frame
    void Update()
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
            currentScale = Mathf.MoveTowards(currentScale, targetScale, 1f / timeToHit * Time.deltaTime * targetScale);
            transform.localScale = Vector3.one * currentScale;
        }
    }

    private void ShowTargetReachedEffect()
    {
        // Using emission for a glow effect
        // Debug.Log("Ready for hit!");
        materialInstance.SetColor("_EmissionColor", glowColor * glowIntensity);
        materialInstance.color = glowColor;
        materialInstance.EnableKeyword("_EMISSION");
    }

    void OnDestroy()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}
