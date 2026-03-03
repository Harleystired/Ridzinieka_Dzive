using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [Header("Shop UI (button)")]
    [SerializeField] private GameObject shopUI; // this is the button/root that should appear at shop

    [Header("Shop Panel (opened by button)")]
    [SerializeField] private GameObject shopPanel; // this is the actual panel/window

    [Header("Other UIs")]
    [SerializeField] private GameObject outsideUI;

    [Header("Refs")]
    [SerializeField] private GameManager gameManager;

    void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
       
        if (shopUI != null) shopUI.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
    }
    private void OnEnable()
    {
        if (gameManager != null)
            gameManager.OnLocationChanged += HandleLocationChanged;

        if (gameManager != null)
            HandleLocationChanged(gameManager.CurrentLocation);
    }
    
    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.OnLocationChanged += HandleLocationChanged;

        if (gameManager != null)
            HandleLocationChanged(gameManager.CurrentLocation);
    }
    
    private void HandleLocationChanged(GameManager.Location location)
    {
        bool atShop = location == GameManager.Location.Shop;

        if (shopUI != null)
            shopUI.SetActive(atShop);

        if (!atShop)
        {
            // Leaving the shop: ensure the panel is closed and modal released
            CloseShop();
            return;
        }

        // Arriving at the shop: outside UI should be hidden
        if (outsideUI != null)
            outsideUI.SetActive(false);
    }
    // Hook this to the Shop UI button's OnClick()
    public void OpenShop()
    {
        if (shopPanel == null) return;

        shopPanel.SetActive(true);
        UIModal.Open();
    }
    // Hook this to the Shop Panel close button's OnClick()
    public void CloseShop()
    {
        if (shopPanel == null) return;
        if (!shopPanel.activeSelf) return;

        if (ShopManager.Instance != null)
            ShopManager.Instance.ClearCart();

        shopPanel.SetActive(false);
        UIModal.Close();
    }
    
}
