using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabbableWithReturn : XRGrabInteractable
{
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Transform parentTransform;

    protected override void Awake()
    {
        base.Awake();
        // Store the initial local transform values
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        parentTransform = transform.parent;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        // When grabbed, temporarily unparent the object
        transform.parent = null;
        base.OnSelectEntered(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        // When released, re-parent and restore original local transform
        transform.parent = parentTransform;
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
    }
}
