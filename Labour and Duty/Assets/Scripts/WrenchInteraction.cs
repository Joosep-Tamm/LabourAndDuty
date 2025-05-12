using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WrenchInteraction : MonoBehaviour
{
    public Transform wrenchHead;
    public float correctPositionThreshold = 0.05f;
    public float correctRotationThreshold = 30f;
    public float requiredRotationAngle = 60f;
    
    private bool isWrenchAligned = false;
    private float currentRotation = 0f;
    private Vector3 lastWrenchRotation;
    private Transform bolt;
    private Transform boltHead;
    private Vector3 boltAxis; // The direction of the bolt's threads

    public bool allowClockwiseRotation = true;
    public bool allowCounterClockwiseRotation = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bolt"))
        {
            bolt = other.transform;
            boltHead = other.transform.Find("BoltHead");
            boltAxis = bolt.forward;
            // Debug.Log("Checking alignment");
            CheckWrenchAlignment();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform == bolt)
        {
            if (isWrenchAligned)
            {
                // Debug.Log("Tracking rotation");
                TrackWrenchRotation();
            }
            else
            {
                CheckWrenchAlignment();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == bolt)
        {
            bolt = null;
        }
    }
    // This method was created with the help of Claude Sonnet LLM
    private void CheckWrenchAlignment()
    {
        // Check position
        float distance = Vector3.Distance(wrenchHead.position, boltHead.position);

        // Check if wrench is perpendicular to bolt axis
        Vector3 wrenchForward = wrenchHead.forward;
        float perpendicularity = Vector3.Angle(wrenchForward, boltAxis);
        
        // The angle should be close to 90 degrees (perpendicular)
        bool isPerpendicularToBolt = Mathf.Abs(perpendicularity - 90f) <= correctRotationThreshold;

        // Check if wrench head is aligned with bolt's hex faces
        // Project wrench direction onto plane perpendicular to bolt axis
        Vector3 projectedWrenchDir = Vector3.ProjectOnPlane(wrenchForward, boltAxis).normalized;
        Vector3 boltReference = Vector3.ProjectOnPlane(bolt.right, boltAxis).normalized;
        
        // For a hex bolt, check if aligned with any of the 6 possible positions (60-degree intervals)
        float angleOnPlane = Vector3.SignedAngle(projectedWrenchDir, boltReference, boltAxis);
        angleOnPlane = (angleOnPlane + 360f) % 360f; // Normalize to 0-360
        
        // Check if angle is close to any of the 6 possible positions (0, 60, 120, 180, 240, 300 degrees)
        bool isAlignedWithHex = false;
        for (int i = 0; i < 6; i++)
        {
            float targetAngle = i * 60f + -30f;
            if (Mathf.Abs(Mathf.DeltaAngle(angleOnPlane, targetAngle)) <= correctRotationThreshold)
            {
                isAlignedWithHex = true;
                break;
            }
        }

        isWrenchAligned = distance <= correctPositionThreshold && 
                         isPerpendicularToBolt && 
                         isAlignedWithHex;

        if (isWrenchAligned)
        {
            bolt.gameObject.GetComponent<BoltInteraction>().ReportWrenchAction(true);
            lastWrenchRotation = wrenchHead.eulerAngles;
            // Debug.Log("Wrench aligned");
        }
    }

    private void TrackWrenchRotation()
    {
        if (!isWrenchAligned) return;

        // Project wrench rotation onto plane perpendicular to bolt axis
        Vector3 currentProjected = Vector3.ProjectOnPlane(wrenchHead.forward, boltAxis).normalized;
        Vector3 lastProjected = Vector3.ProjectOnPlane(
            Quaternion.Euler(lastWrenchRotation) * Vector3.forward, 
            boltAxis
        ).normalized;

        // Calculate rotation around bolt axis
        float rotationDelta = Vector3.SignedAngle(lastProjected, currentProjected, boltAxis);

        // Check rotation direction
        if ((!allowClockwiseRotation && rotationDelta > 0) ||
            (!allowCounterClockwiseRotation && rotationDelta < 0))
        {
            return;
        }

        currentRotation += rotationDelta;
        lastWrenchRotation = wrenchHead.eulerAngles;

        // Optional: Add resistance/snapping effect at 60-degree intervals
        float snapAngle = Mathf.Round(currentRotation / 60f) * 60f;
        float snapForce = Mathf.Lerp(0f, 1f, 
            1f - Mathf.Abs(currentRotation - snapAngle) / 30f);

        // Check for completion
        if (Mathf.Abs(currentRotation) >= requiredRotationAngle)
        {
            BoltTurnComplete();
        }
    }

    private void BoltTurnComplete()
    {
        // Debug.Log("Bolt has been turned successfully!");

        // Add your completion logic here
        // For example: animate the bolt, trigger next game event, etc.
        currentRotation = 0f;
        bolt.gameObject.GetComponent<BoltInteraction>().ReportWrenchAction(true);
    }

    // Visual debugging
    private void OnDrawGizmos()
    {
        if (!wrenchHead || !bolt) return;

        // Draw bolt axis
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(bolt.position, boltAxis * 0.2f);

        // Draw wrench direction
        Gizmos.color = isWrenchAligned ? Color.green : Color.red;
        Gizmos.DrawRay(wrenchHead.position, wrenchHead.forward * 0.2f);

        // Draw perpendicular plane
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Vector3 projected = Vector3.ProjectOnPlane(wrenchHead.forward, boltAxis).normalized;
            Gizmos.DrawRay(bolt.position, projected * 0.15f);
            DrawValidPositions();
        }
    }

    // Add helper method to visualize valid wrench positions
    private void DrawValidPositions()
    {
        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f;
            Vector3 direction = Quaternion.AngleAxis(angle, boltAxis) * 
                              Vector3.ProjectOnPlane(bolt.right, boltAxis);
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(bolt.position, direction.normalized * 0.1f);
        }
    }
}