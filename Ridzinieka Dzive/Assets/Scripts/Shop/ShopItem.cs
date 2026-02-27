using UnityEngine;
using UnityEngine.EventSystems;

public class ShopItem : MonoBehaviour, IPointerClickHandler
{
    public string itemName;
    public int price;
    public int quantity = 0;

    public UIShopItem ui; // atsauce uz UI skriptu
    private void Start()
    {
        ShopManager.Instance.allItems.Add(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        quantity++;
        ShopManager.Instance.AddToCart(this);
        ui.UpdateQuantity(quantity);
    }
}