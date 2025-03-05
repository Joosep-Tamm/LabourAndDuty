using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class VRButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Button button;
    private Image buttonImage;
    private Material originalMaterial;
    [SerializeField] private Material highlightMaterial;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        originalMaterial = buttonImage.material;

        Debug.Log("VRButton initialized on: " + gameObject.name);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Hover Enter on: " + gameObject.name);
        buttonImage.material = highlightMaterial;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Hover Exit on: " + gameObject.name);
        buttonImage.material = originalMaterial;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Click on: " + gameObject.name);
        button.onClick.Invoke();
    }
}