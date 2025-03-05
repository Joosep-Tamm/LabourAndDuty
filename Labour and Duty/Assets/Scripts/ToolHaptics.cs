using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ToolHaptics : MonoBehaviour
{
    public float hapticAmplitude = 0.7f;
    public float hapticDuration = 0.1f;

    public void TriggerHaptic()
    {
        // Get the grab interactable
        XRGrabInteractable grabInteractable = gameObject.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null && grabInteractable.isSelected)
        {
            // Get the current interactor
            var interactor = grabInteractable.interactorsSelecting[0];
            if (interactor != null)
            {
                // Try to get the haptic player from the interactor's GameObject
                HapticImpulsePlayer haptic = interactor.transform.GetComponentInParent<HapticImpulsePlayer>();
                if (haptic != null)
                {
                    haptic.SendHapticImpulse(hapticAmplitude, hapticDuration, 0);
                }
            }
        }
    }
}
