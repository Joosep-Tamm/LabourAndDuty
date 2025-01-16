using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SnapToAssemblyPoint : MonoBehaviour
{
    public string compareTag;
    public PlacementIndicator placementIndicator; // Reference to the indicator

    private void Start()
    {
        // Make sure you have assigned the PlacementIndicator in the inspector
        if (placementIndicator == null)
        {
            Debug.LogWarning("Placement Indicator not assigned!");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(compareTag)) // Check if the object is correct
        {
            SnapToPosition(other.gameObject); // Snap the object
            if (placementIndicator != null)  // Hide the indicator when object is placed
            {
                placementIndicator.ObjectPlaced();
            }
        }
    }

    private void SnapToPosition(GameObject snapObject)
    {
        // Snap the object's position and rotation to the assembly point
        snapObject.transform.position = transform.position;
        snapObject.transform.rotation = transform.rotation;

        // Optionally, disable the Rigidbody if you no longer need physics for the object
        Rigidbody rb = snapObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Set Rigidbody to Kinematic (prevents movement)
        }

        snapObject.transform.SetParent(transform);

        // Optionally, you can also disable the XR grab interactable component
        XRGrabInteractable grabInteractable = snapObject.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false; // Disable the interaction after snapping
        }

    }
}

