using UnityEngine;

public class VelocityTracker : MonoBehaviour
{
    private Vector3 previousPosition;
    private Vector3 velocity;
    private float smoothingFactor = 0.1f;

    void Start()
    {
        previousPosition = transform.position;
    }

    void Update()
    {
        // Calculate velocity
        Vector3 currentPosition = transform.position;
        Vector3 newVelocity = (currentPosition - previousPosition) / Time.deltaTime;

        // Smooth the velocity
        velocity = Vector3.Lerp(velocity, newVelocity, smoothingFactor);

        previousPosition = currentPosition;
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }

    // Optional: For more accurate throwing calculations
    public Vector3 GetSmoothedVelocity()
    {
        return velocity;
    }
}