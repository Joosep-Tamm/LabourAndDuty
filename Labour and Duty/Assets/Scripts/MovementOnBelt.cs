using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementOnBelt : MonoBehaviour
{
    [SerializeField] private Vector3 toInteractionArea = Vector3.zero;
    [SerializeField] private Vector3 toDropOff = Vector3.zero;
    public float timeToMove;

    private bool move = false;
    private bool toInteraction = false;

    private float elapsedTime = 0;
    private Vector3 localTargetPosition;
    private Vector3 localStartPosition;
    private float totalDistance;

    public void MoveToInteractionArea(float time)
    {
        timeToMove = time;
        move = true;
        toInteraction = true;
        elapsedTime = 0;

        localStartPosition = transform.localPosition;
        localTargetPosition = transform.position + toInteractionArea;
    }

    public void MoveToDropOff(float time)
    {
        timeToMove = time;
        move = true;
        toInteraction = false;
        elapsedTime = 0;

        localStartPosition = transform.localPosition;
        localTargetPosition = transform.position + toDropOff;
    }

    void FixedUpdate()
    {
        if (move)
        {
            elapsedTime += Time.fixedDeltaTime;
            float percentageComplete = elapsedTime / timeToMove;

            if (percentageComplete <= 1.0f)
            {
                transform.position = Vector3.Lerp(localStartPosition, localTargetPosition, percentageComplete);
            }
            else
            {
                transform.localPosition = localTargetPosition;
                move = false;
            }
        }
    }
}
