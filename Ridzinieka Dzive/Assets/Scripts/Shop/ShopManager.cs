using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public TMP_Text totalText;
    private int total = 0;
    [SerializeField] private GameManager gameManager;
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

    public void ClearCart()
    {
        total = 0;
        UpdateTotal();
    }

    private void UpdateTotal()
    {
        totalText.text = "$" + total.ToString();
    }

    public void Pay()
    {
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (total <= 0)
        {
            Debug.Log("Grozs ir tukšs."); return;
        }

        if (!gameManager.SpendMoney(total))
        {
            Debug.Log("Nepietiek naudas!"); return;
        } 
        Debug.Log("Pirkums veiksmīgs!"); ClearCart();
    }
}