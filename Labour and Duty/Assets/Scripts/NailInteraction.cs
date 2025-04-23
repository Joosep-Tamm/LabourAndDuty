using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class NailInteraction : MonoBehaviour
{
    private Vector3 hammerAxis;
    private Vector3 nailAxis;

    private bool hit = false;

    private int currentHit = 0;
    public int CurrentHit => currentHit;

    // Event to notify manager of successful hits
    public System.Action<float, float, GameObject> onNailHit;


    void Start()
    {
        nailAxis = transform.forward;
        //Debug.Log(nailAxis);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.CompareTag("Hammer"))
        {
            Debug.Log("Hammer detected");
            hammerAxis = other.gameObject.transform.right;

            VelocityTracker hammerVelocity = other.gameObject.GetComponent<VelocityTracker>();
            if (hammerVelocity != null)
            {
                Debug.Log("Checking Hammer hit");
                CheckHammerHit(hammerVelocity, other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log(other.name);
        if (other.CompareTag("Hammer")) hit = false;
    }

    private void CheckHammerHit(VelocityTracker hammerVelocity, GameObject hammer)
    {
        if (!hit)
        {
            float angle = Vector3.Angle(hammerAxis, nailAxis);
            float speedTowardNail = Vector3.Dot(hammerVelocity.GetVelocity(), nailAxis);

            onNailHit?.Invoke(angle, speedTowardNail, hammer);
            hit = true;
        }
        else
        {
            Debug.Log("Nail already hit");
        }
    }

    // Public method for manager to move the nail
    public void MoveNail(float distance)
    {
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y - distance,
            transform.position.z
        );
    }

    // Public methods for manager to control nail state
    public void DisableNail()
    {
        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = true;
        enabled = false;
    }

    public void IncrementHit()
    {
        currentHit++;
    }
}
