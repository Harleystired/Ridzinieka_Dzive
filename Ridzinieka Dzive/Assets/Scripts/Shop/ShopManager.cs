using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public TMP_Text totalText;
    private int total = 0;

    [SerializeField] private GameManager gameManager;

    // ← JAUNS SARAKSTS, kur glabājas visas preces
    public List<ShopItem> allItems = new List<ShopItem>();

    private void Awake()
    {
        Instance = this;
        UpdateTotal();
    }

    public void AddToCart(ShopItem item)
    {
        total += item.price;
        UpdateTotal();
    }

    // ← PILNS ClearCart, kas iztīra quantity
    public void ClearCart()
    {
        total = 0;
        UpdateTotal();

        foreach (var item in allItems)
        {
            item.quantity = 0;
            item.ui.UpdateQuantity(0);
        }
    }

    private void UpdateTotal()
    {
        totalText.text = "€" + total.ToString();
    }

    public void Pay()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (total <= 0)
        {
            Debug.Log("Grozs ir tukšs.");
            return;
        }

        if (!gameManager.SpendMoney(total))
        {
            Debug.Log("Nepietiek naudas!");
            return;
        }

        Debug.Log("Pirkums veiksmīgs!");
        ClearCart();
    }
}