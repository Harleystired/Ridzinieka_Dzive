using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    public GameManager gameManager;

    // Optional: assign these if you want to set/swap icon sprites from code.
    // If icons are static, you can remove these fields and just set sprites in the Inspector.
    public Image hungerIcon;
    public Image energyIcon;
    public Image stressIcon;
    public Image healthIcon;
    public Image budgetIcon;

    // These should now be the VALUE texts next to each icon (not "Name: Value")
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI stressText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI budgetText;

    void OnEnable()
    {
        UpdateStats();
    }

    public void UpdateStats()
    {
        hungerText.text = gameManager.hunger.ToString();
        energyText.text = gameManager.energy.ToString();
        stressText.text = gameManager.stress.ToString();
        healthText.text = gameManager.health.ToString();
        budgetText.text = gameManager.money.ToString();
    }
}



