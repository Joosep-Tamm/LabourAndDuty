using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToParent : MonoBehaviour
{
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Transform parentTransform;

    private void Start()
    {
        // Store the initial local transform values
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        parentTransform = transform.parent;
    }

    public void OnGrab()
    {
        // When grabbed, temporarily unparent the object
        transform.parent = null;
    }

    public void OnRelease()
    {
        // When released, re-parent and restore original local transform
        transform.parent = parentTransform;
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
    }
}