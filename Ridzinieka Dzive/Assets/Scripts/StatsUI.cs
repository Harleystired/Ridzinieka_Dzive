using UnityEngine;
using TMPro;

public class StatsUI : MonoBehaviour
{
    public GameManager gameManager;

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
        hungerText.text = "Bads: " + gameManager.hunger;
        energyText.text = "Enerģija: " + gameManager.energy;
        stressText.text = "Stress: " + gameManager.stress;
        healthText.text = "Veselība: " + gameManager.health;
        budgetText.text = "Budžets: " + gameManager.money;
    }
}



