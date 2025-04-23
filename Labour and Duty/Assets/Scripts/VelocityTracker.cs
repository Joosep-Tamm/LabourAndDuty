using System.Collections.Generic;
using UnityEngine;

public class VelocityTracker : MonoBehaviour
{
    private Vector3 previousPosition;
    private Vector3 velocity;
    private float smoothingFactor = 0.1f;

    // Add a multiplier to adjust for Quest 2
    [SerializeField] private float velocityMultiplier = 1f;

    private void Start()
    {
        previousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        Vector3 currentPosition = transform.position;

        // Calculate velocity in FixedUpdate for more consistent physics
        Vector3 newVelocity = (currentPosition - previousPosition) / Time.fixedDeltaTime;

        // Apply multiplier and smoothing
        velocity = Vector3.Lerp(velocity, newVelocity * velocityMultiplier, smoothingFactor);

        previousPosition = currentPosition;
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }

    // Optional: Store multiple frames for more accurate velocity
    private Queue<Vector3> velocityWindow = new Queue<Vector3>(10);
    private int windowSize = 10;

    public Vector3 GetSmoothedVelocity()
    {
        if (velocityWindow.Count == 0) return velocity;

        Vector3 averageVelocity = Vector3.zero;
        foreach (Vector3 v in velocityWindow)
        {
            averageVelocity += v;
        }
        return averageVelocity / velocityWindow.Count;
    }

    private void LateUpdate()
    {
        // Store velocity history
        velocityWindow.Enqueue(velocity);
        if (velocityWindow.Count > windowSize)
        {
            velocityWindow.Dequeue();
        }
    }
}