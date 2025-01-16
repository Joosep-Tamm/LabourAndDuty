using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NailInteraction : MonoBehaviour
{
    public float correctAngleThreshhold = 30f;
    public float correctHitSpeed = 0.7f;

    private Vector3 hammerAxis;
    private Vector3 nailAxis; 
    
    void Start()
    {
        nailAxis = transform.forward;
        //Debug.Log(nailAxis);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hammer"))
        {
            hammerAxis = other.gameObject.transform.right;

            VelocityTracker hammerVelocity = other.gameObject.GetComponent<VelocityTracker>();
            if (hammerVelocity != null)
            {
                CheckHammerHit(hammerVelocity);
            }
        }
    }

    private void CheckHammerHit(VelocityTracker hammerVelocity)
    {
        float angle = Vector3.Angle(hammerAxis, nailAxis);
        Debug.Log(angle);
        if (angle < correctAngleThreshhold)
        {
            float speedTowardNail = Vector3.Dot(hammerVelocity.GetVelocity(), nailAxis);
            if (speedTowardNail > correctHitSpeed) Debug.Log("Hit the nail");
            /*else
            {
                Debug.Log(speedTowardNail);
                Debug.Log(hammerVelocity.GetVelocity());
                Debug.Log(nailAxis);
            }
            */
        }
    }
}
