using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerBody : MonoBehaviour
{
    [SerializeField] private Transform mainCamera;
    [SerializeField] private float heightOffset = -0.5f; // Adjust this to position relative to head height
    [SerializeField] private Vector3 positionOffset; // Optional additional offset

    private void Update()
    {
        // Get camera position but only use X and Z
        Vector3 bodyPosition = new Vector3(
            mainCamera.position.x,
            mainCamera.position.y + heightOffset, // Fixed height offset from camera
            mainCamera.position.z
        );

        // Apply position with offset
        transform.position = bodyPosition + positionOffset;
    }
}
