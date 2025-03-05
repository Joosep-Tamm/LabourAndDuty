// SequenceSelectionUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class SequenceSelectionUI : MonoBehaviour
{
    [SerializeField] private RhythmManager rhythmManager;
    [SerializeField] private Canvas selectionCanvas;
    [SerializeField] private Transform buttonContainer; // Parent object for sequence buttons
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Vector3 spawnPosition = new Vector3(0, 1.7f, 2f); // Position in front of player
    [SerializeField] private Vector3 spawnRotation = new Vector3(0, 180, 0); // Facing player

    private void Start()
    {
        // Position the canvas in front of the player
        transform.position = spawnPosition;
        transform.eulerAngles = spawnRotation;

        CreateSequenceButtons();
    }

    private void Awake()
    {
        // Find all EventSystems in the scene
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();

        // If we have more than one, destroy the extras
        if (eventSystems.Length > 1)
        {
            for (int i = 1; i < eventSystems.Length; i++)
            {
                Destroy(eventSystems[i].gameObject);
            }
        }

        // If we have no EventSystem, create one
        if (eventSystems.Length == 0)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<XRUIInputModule>();
        }
    }

    private void CreateSequenceButtons()
    {
        // Create a button for each sequence
        for (int i = 0; i < rhythmManager.sequences.Length; i++)
        {
            int index = i; // Capture for lambda
            Button newButton = Instantiate(buttonPrefab, buttonContainer);
            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();

            // Set button text to sequence name
            buttonText.text = rhythmManager.sequences[i].sequenceName;

            // Add click handler
            newButton.onClick.AddListener(() => StartSequence(index));
        }
    }

    private void StartSequence(int sequenceIndex)
    {
        rhythmManager.currentSequenceIndex = sequenceIndex;
        rhythmManager.ResetSequence();
        rhythmManager.StartSequence();
        gameObject.SetActive(false); // Hide UI after selection
    }

    // Method to show the UI again
    public void Show()
    {
        gameObject.SetActive(true);
        transform.position = spawnPosition;
        transform.eulerAngles = spawnRotation;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}