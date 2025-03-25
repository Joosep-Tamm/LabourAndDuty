using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SnapToAssemblyPoint : MonoBehaviour
{
    public string compareTag;
    public System.Action<bool> OnObjectSnapped; // Add this callback
    private bool hasSnapped = false;
    private bool allowSnap = false;
    private GameObject snappedObject = null;

    private void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!hasSnapped && allowSnap && other.CompareTag(compareTag)) // Check if the object is correct
        {
            SnapToPosition(other.gameObject); // Snap the object
            GetComponent<MeshRenderer>().enabled = false;

            // Trigger the callback for rhythm game
            hasSnapped = true;
            allowSnap = false;
            OnObjectSnapped?.Invoke(true);
        }
    }

    private void SnapToPosition(GameObject snapObject)
    {
        Debug.Log("Snapping " + snapObject.name + " to " + gameObject.name);

        // First, force deselect and disable the XR Grab Interactable
        XRGrabInteractable grabInteractable = snapObject.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            Debug.Log("Handling grab interactable");
            if (grabInteractable.isSelected)
            {
                grabInteractable.enabled = false;
            }
            // Store the original parent before disabling
            Transform originalParent = snapObject.transform.parent;
            grabInteractable.enabled = false;

            // If unparented, restore the parent
            if (snapObject.transform.parent == null)
            {
                Debug.Log("Restoring parent after disable");
                snapObject.transform.parent = originalParent;
            }
        }

        // Optionally, disable the Rigidbody if you no longer need physics for the object
        Rigidbody rb = snapObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log($"Starting snap for {snapObject.name}");
            rb.isKinematic = true; // Set Rigidbody to Kinematic (prevents movement)
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Snap the object's position and rotation to the assembly point
        snapObject.transform.position = transform.position;
        snapObject.transform.rotation = transform.rotation;

        Debug.Log("Setting parent");
        snapObject.transform.parent = transform.parent;
        Debug.Log($"Parent of {snapObject.name} is {snapObject.transform.parent?.name ?? "null"}");

        snapObject.tag = "Attached";

        snappedObject = snapObject;
        Debug.Log($"Parent of {snapObject.name} is {snapObject.transform.parent?.name ?? "null"}");
    }

    public void ResetSnap()
    {
        hasSnapped = false;
    }

    public void EnableSnap()
    {
        allowSnap = true;
    }

    public void DisableSnap()
    {
        allowSnap = false;
    }

    private void OnDisable()
    {
        Debug.Log($"SnapToAssemblyPoint disabled. Snapped object: {snappedObject?.name ?? "none"}");
    }

    private void OnDestroy()
    {
        Debug.Log($"SnapToAssemblyPoint destroyed. Snapped object: {snappedObject?.name ?? "none"}");
    }
}

