using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRLineVisual : MonoBehaviour
{
    [SerializeField] private XRRayInteractor rayInteractor; // Reference to the working ray interactor

    [Header("Ray Visualization")]
    [SerializeField] private Color rayColor = Color.cyan;

    private GameObject ray;
    private LineRenderer rayLine;

    private void Awake()
    {
        SetupRayVisual();
    }

    private void SetupRayVisual()
    {
        ray = new GameObject("Ray");
        ray.transform.SetParent(transform, false);

        rayLine = CreateLine("Ray", rayColor);
    }

    private LineRenderer CreateLine(string name, Color color)
    {
        GameObject lineObj = new GameObject(name + "Line");
        lineObj.transform.SetParent(ray.transform, false);

        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.startWidth = 0.005f;
        line.endWidth = 0.005f;
        line.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        line.startColor = color;
        line.endColor = color;
        line.positionCount = 2;

        line.enabled = false;

        return line;
    }

    private void LateUpdate()
    {
        // Get the controller transform
        Transform controllerTransform = rayInteractor.transform;

        // Get the ray origin transform
        Vector3 rayOrigin = rayInteractor.rayOriginTransform.position;
        Vector3 rayDirection = rayInteractor.rayOriginTransform.forward;
        Vector3 rayEnd = rayOrigin + rayDirection * rayInteractor.maxRaycastDistance;

        rayLine.SetPosition(0, rayOrigin);
        rayLine.SetPosition(1, rayEnd);


        
    }

    private void OnDestroy()
    {
        if (ray != null)
        {
            Destroy(ray);
        }
    }

    private void OnEnable()
    {
        if (rayLine != null && rayLine.enabled == false)
        {
            rayLine.enabled = true;
        }
    }

    private void OnDisable()
    {
        if (rayLine != null && rayLine.enabled == true)
        {
            rayLine.enabled = false;
        }
    }
}
