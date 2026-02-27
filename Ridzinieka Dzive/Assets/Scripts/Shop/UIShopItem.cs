using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UIShopItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text quantityText;
    public TMP_Text hoverText; // teksts, kas parādās virs ikonas
    public string itemName;

    public void UpdateQuantity(int qty)
    {
        quantityText.text = qty.ToString();
        quantityText.gameObject.SetActive(qty > 0);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverText.text = itemName;
        hoverText.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverText.gameObject.SetActive(false);
    }
}