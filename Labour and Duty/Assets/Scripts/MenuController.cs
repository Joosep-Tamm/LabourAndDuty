using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class MenuController : MonoBehaviour
{
    [SerializeField] private RhythmManager rhythmManager;
    [SerializeField] private InputActionReference menuButtonAction;

    private bool menuVisible = false;

    private void OnEnable()
    {
        menuButtonAction.action.Enable();
        menuButtonAction.action.performed += OnMenuPressed;
    }

    private void OnDisable()
    {
        menuButtonAction.action.Disable();
        menuButtonAction.action.performed -= OnMenuPressed;
    }

    private void OnMenuPressed(InputAction.CallbackContext context)
    {
        Debug.Log("Menu button pressed");
        if (!menuVisible)
        {
            rhythmManager.ShowSequenceSelector();
            menuVisible = true;
        }
        else
        {
            rhythmManager.CloseSequenceSelector();
            menuVisible = false;
        }
    }
}