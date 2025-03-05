using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class UIRayDebug : MonoBehaviour
{
    [SerializeField] private XRRayInteractor rayInteractor;
    [SerializeField] private LineRenderer lineRenderer;

    void Update()
    {
        if (rayInteractor != null)
        {
            RaycastHit hit;
            bool didHit = rayInteractor.TryGetCurrent3DRaycastHit(out hit);
            //Debug.Log($"Ray hit something: {didHit}");
            if (didHit)
            {
                Debug.Log($"Hit object: {hit.collider.gameObject.name} on layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            }

            // Draw debug line
            Vector3 startPos = rayInteractor.transform.position;
            Vector3 endPos = startPos + rayInteractor.transform.forward * 10f;
            Debug.DrawLine(startPos, endPos, Color.red);
        }
    }
}