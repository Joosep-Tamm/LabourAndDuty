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
    private Vector3 targetPosition;
    private Vector3 startPosition;
    private float totalDistance;

    public void MoveToInteractionArea(float time)
    {
        timeToMove = time;
        move = true;
        toInteraction = true;
        elapsedTime = 0;
        startPosition = transform.position;
        targetPosition = transform.position + toInteractionArea;
        totalDistance = Vector3.Distance(startPosition, targetPosition);
    }

    public void MoveToDropOff(float time)
    {
        timeToMove = time;
        move = true;
        toInteraction = false;
        elapsedTime = 0;
        startPosition = transform.position;
        targetPosition = transform.position + toDropOff;
        totalDistance = Vector3.Distance(startPosition, targetPosition);
    }

    void Update()
    {
        if (move)
        {
            elapsedTime += Time.deltaTime;
            float percentageComplete = elapsedTime / timeToMove;

            if (percentageComplete <= 1.0f)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, percentageComplete);
            }
            else
            {
                // Ensure we end up exactly at the target
                transform.position = targetPosition;
                move = false;
            }
        }
    }
}
