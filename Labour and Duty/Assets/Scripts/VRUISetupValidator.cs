using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class VRUISetupValidator : MonoBehaviour
{
    void Start()
    {
        // Check Canvas setup
        var canvases = FindObjectsOfType<Canvas>();
        foreach (var canvas in canvases)
        {
            ValidateCanvas(canvas);
        }

        // Check XR components
        ValidateXRComponents();
    }

    private void ValidateCanvas(Canvas canvas)
    {
        Debug.Log($"Checking Canvas: {canvas.name}");
        Debug.Log($"Render Mode: {canvas.renderMode}");

        var raycaster = canvas.GetComponent<GraphicRaycaster>();
        Debug.Log($"Has GraphicRaycaster: {raycaster != null}");

        var collider = canvas.GetComponent<Collider>();
        Debug.Log($"Has Collider: {collider != null}");

        if (collider != null)
        {
            Debug.Log($"Collider bounds: {collider.bounds.size}");
        }
    }

    private void ValidateXRComponents()
    {
        var interactionManager = FindObjectOfType<XRInteractionManager>();
        Debug.Log($"Has XR Interaction Manager: {interactionManager != null}");

        var eventSystem = FindObjectOfType<EventSystem>();
        Debug.Log($"Has Event System: {eventSystem != null}");

        if (eventSystem != null)
        {
            var inputModule = eventSystem.GetComponent<XRUIInputModule>();
            Debug.Log($"Has XR UI Input Module: {inputModule != null}");
        }
    }
}