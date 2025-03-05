using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public class Welder : MonoBehaviour
{
    [SerializeField] private ParticleSystem sparkParticles; // Reference to your spark particle system
    [SerializeField] private Transform weldPoint; // Reference to the tip of the welder
    [SerializeField] private WeldPaintSystem paintSystem;
    private XRGrabInteractable grabInteractable;
    private bool isWelding = false;
    private bool isInPaintingVolume = false;

    void Start()
    {
        // Get the XRGrabInteractable component
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Subscribe to the activate/deactivate events
        grabInteractable.activated.AddListener(StartWelding);
        grabInteractable.deactivated.AddListener(StopWelding);

        // Make sure particles are off at start
        if (sparkParticles != null)
            sparkParticles.Stop();

    }

    private void Update()
    {
        if (isWelding && paintSystem != null && isInPaintingVolume)
        {
            paintSystem.PaintWeld(weldPoint.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        WeldPaintSystem newPaintSystem = other.transform.GetComponent<WeldPaintSystem>();
        if (newPaintSystem != null)
        {
            isInPaintingVolume = true;
            paintSystem = newPaintSystem;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        WeldPaintSystem newPaintSystem = other.transform.GetComponent<WeldPaintSystem>();
        if (newPaintSystem != null)
        {
            isInPaintingVolume = false;
            paintSystem = null;
        }
    }

    private void StartWelding(ActivateEventArgs arg)
    { 
        if (sparkParticles != null && !isWelding)
        {
            sparkParticles.Play();
            // Add any other welding effects here (sound, light, etc.)
        }
        isWelding = true;
    }

    private void StopWelding(DeactivateEventArgs arg)
    {
        if (sparkParticles != null && isWelding)
        {
            sparkParticles.Stop();
            // Stop any other welding effects here
        }
        isWelding = false;
    }

    public Vector3 GetWeldPoint()
    {
        return weldPoint.position;
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(StartWelding);
            grabInteractable.deactivated.RemoveListener(StopWelding);
        }
    }
}
