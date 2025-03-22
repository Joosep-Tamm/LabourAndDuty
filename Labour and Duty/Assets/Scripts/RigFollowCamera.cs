using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform xrRig;
    [SerializeField] private Transform mainCamera;

    private Vector3 lastHeadsetPosition;
    private Transform cameraParent;

    private void Start()
    {
        // Store the camera's original parent (probably the camera offset)
        cameraParent = mainCamera.parent;

        // Get initial position in local space of the rig
        lastHeadsetPosition = cameraParent.InverseTransformPoint(mainCamera.position);
    }

    private void Update()
    {
        // Get current headset position in local space of the rig
        Vector3 currentHeadsetPosition = cameraParent.InverseTransformPoint(mainCamera.position);

        // Calculate the delta in local space
        Vector3 delta = currentHeadsetPosition - lastHeadsetPosition;
        delta.y = 0; // Ignore vertical movement

        // Convert the delta to world space movement
        Vector3 worldSpaceDelta = cameraParent.TransformVector(delta);

        // Move the rig
        xrRig.position += worldSpaceDelta;

        // Update last position
        lastHeadsetPosition = currentHeadsetPosition;
    }
}
