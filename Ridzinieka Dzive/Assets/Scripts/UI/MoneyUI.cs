using UnityEngine;
using TMPro;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private GameManager gameManager;

    private void Start()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        gameManager.OnMoneyChanged += UpdateMoneyText;
        UpdateMoneyText(gameManager.money);
    }

    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.OnMoneyChanged -= UpdateMoneyText;
    }

    private void UpdateMoneyText(int newAmount)
    {
        moneyText.text = "€" + newAmount.ToString();
    }
}