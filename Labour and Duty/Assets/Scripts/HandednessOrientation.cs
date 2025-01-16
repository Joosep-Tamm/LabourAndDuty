using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class HandedNessOrientation : MonoBehaviour
{

    [SerializeField] private Transform rightHandGrabPoint;
    [SerializeField] private Transform leftHandGrabPoint;

    private XRGrabInteractable grabInteractable;
    private bool wasRightHand = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        // Subscribe to interaction events
        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // Check which hand grabbed the wrench
        IXRSelectInteractor interactor = args.interactorObject;


        if (interactor != null)
        {
            // Determine if it's the left or right hand based on the interactor's name or tag
            bool isRightHand = interactor.handedness == InteractorHandedness.Right; 
            grabInteractable.attachTransform = isRightHand ? rightHandGrabPoint : leftHandGrabPoint;

            if (isRightHand && !wasRightHand || !isRightHand && wasRightHand)
            {
                wasRightHand = isRightHand;
                grabInteractable.interactionManager.SelectExit(args.interactorObject, grabInteractable);
                grabInteractable.interactionManager.SelectEnter(args.interactorObject, grabInteractable);
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
        }
    }
}
