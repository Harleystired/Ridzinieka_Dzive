using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PhoneOpenClickArea : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private PhoneUI phoneUI;

    [Header("Hover Highlight (UI)")]
    [SerializeField] private Graphic targetGraphic; // Image/TMP text/etc. Anything derived from Graphic
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 0.6f, 1f);

    private void Reset()
    {
        targetGraphic = GetComponent<Graphic>();
        phoneUI = GetComponentInParent<PhoneUI>();
    }

    private void Awake()
    {
        if (phoneUI == null)
            phoneUI = GetComponentInParent<PhoneUI>();

        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();

        ApplyColor(normalColor);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (phoneUI == null) return;

        phoneUI.Open();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ApplyColor(hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ApplyColor(normalColor);
    }

    private void ApplyColor(Color c)
    {
        if (targetGraphic != null)
            targetGraphic.color = c;
    }
}
