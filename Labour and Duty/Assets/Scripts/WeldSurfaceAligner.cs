// Helper script to align weld surface
using UnityEngine;

public class WeldSurfaceAligner : MonoBehaviour
{
    [SerializeField] private Vector3 dimensions = new Vector3(0.1f, 0.1f, 0.001f);

    void OnDrawGizmos()
    {
        // Visualize weld area
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, dimensions);
    }

    // Optional: Add method to align to surface normal
    public void AlignToSurface(Vector3 position, Vector3 normal)
    {
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(normal) * Quaternion.Euler(90, 0, 0);
    }
}