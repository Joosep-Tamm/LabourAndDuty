using UnityEngine;

public class VelocityAwarePositionTracker : MonoBehaviour
{
    [SerializeField] private float velocitySmoothing = 0.1f;
    [SerializeField] private float positionSmoothing = 0.2f;
    [SerializeField] private bool useFixedUpdate = true;

    private Vector3 smoothedPosition;
    private Vector3 smoothedVelocity;
    private Vector3 lastPosition;
    private float lastUpdateTime;

    private void Start()
    {
        smoothedPosition = transform.position;
        lastPosition = transform.position;
        lastUpdateTime = Time.time;
    }

    private void Update()
    {
        if (!useFixedUpdate)
        {
            UpdatePosition();
        }
    }

    private void FixedUpdate()
    {
        if (useFixedUpdate)
        {
            UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        Vector3 currentPosition = transform.position;
        float deltaTime = Time.time - lastUpdateTime;

        if (deltaTime > 0)
        {
            // Calculate current velocity
            Vector3 currentVelocity = (currentPosition - lastPosition) / deltaTime;

            // Smooth velocity
            smoothedVelocity = Vector3.Lerp(smoothedVelocity, currentVelocity, velocitySmoothing);

            // Predict next position based on velocity
            Vector3 predictedPosition = currentPosition + (smoothedVelocity * deltaTime);

            // Smooth position with prediction
            smoothedPosition = Vector3.Lerp(smoothedPosition, predictedPosition, positionSmoothing);

            lastPosition = currentPosition;
            lastUpdateTime = Time.time;

            if (Time.frameCount % 30 == 0) // Log every 30 frames
            {
                //Debug.Log($"Raw: {currentPosition}, Smoothed: {smoothedPosition}, Velocity: {smoothedVelocity.magnitude}");
            }
        }
    }

    public Vector3 GetTrackedPosition()
    {
        return smoothedPosition;
    }

    public Vector3 GetVelocity()
    {
        return smoothedVelocity;
    }
}