using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class JobPanelController : MonoBehaviour
{
    [System.Serializable]
    public class JobData
    {
        public string title;
        public string description;
        public string salary;
        public string hours;

        // Stat impacts
        public int hungerChange;
        public int energyChange;
        public int stressChange;
        public int healthChange;
        public int moneyEarned;
    }

    public JobData[] jobs;

    public TextMeshProUGUI jobTitle;
    public TextMeshProUGUI jobDescription;
    public TextMeshProUGUI jobSalary;
    public TextMeshProUGUI jobHours;

    public Button chooseButton;
    public Button leftArrow;
    public Button rightArrow;

    public TextMeshProUGUI dayText;

    public TextMeshProUGUI budgetText;
    public TextMeshProUGUI stressText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI energyText;

    private int currentJobIndex = 0;
    private GameManager gameManager;

    private void OnEnable()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        UpdateHeader();
        UpdateStats();
        UpdateJobUI();
    }

    private void UpdateHeader()
    {
        dayText.text = "Day " + (gameManager.CurrentDayIndex + 1);
        
    }

    private void UpdateStats()
    {
        budgetText.text = "Budžets: " + gameManager.money;
        stressText.text = "Stress: " + gameManager.stress;
        healthText.text = "Veselība: " + gameManager.health;
        hungerText.text = "Bads: " + gameManager.hunger;
        energyText.text = "Enerģija: " + gameManager.energy;
    }

    private void UpdateJobUI()
    {
        var job = jobs[currentJobIndex];

        jobTitle.text = job.title;
        jobDescription.text = job.description;
        jobSalary.text = job.salary;
        jobHours.text = job.hours;
    }

    public void NextJob()
    {
        currentJobIndex++;
        if (currentJobIndex >= jobs.Length)
            currentJobIndex = 0;

        UpdateJobUI();
    }

    public void PreviousJob()
    {
        currentJobIndex--;
        if (currentJobIndex < 0)
            currentJobIndex = jobs.Length - 1;

        UpdateJobUI();
    }

    public void ChooseJob()
    {
        var job = jobs[currentJobIndex];

        // Apply stat changes via clamped methods (0..100)
        if (job.hungerChange >= 0) gameManager.AddHunger(job.hungerChange);
        else gameManager.RemoveHunger(-job.hungerChange);

        if (job.energyChange >= 0) gameManager.AddEnergy(job.energyChange);
        else gameManager.RemoveEnergy(-job.energyChange);

        if (job.stressChange >= 0) gameManager.AddStress(job.stressChange);
        else gameManager.RemoveStress(-job.stressChange);

        if (job.healthChange >= 0) gameManager.AddHealth(job.healthChange);
        else gameManager.RemoveHealth(-job.healthChange);

        // Money
        if (job.moneyEarned != 0)
            gameManager.AddMoney(job.moneyEarned);

        Debug.Log("Chosen job: " + job.title);

        if (gameManager != null)
            gameManager.SetSelectedJobFromIndex(currentJobIndex);

        var statsUI = FindFirstObjectByType<StatsUI>();
        if (statsUI != null)
            statsUI.UpdateStats();

        StartCoroutine(ClosePanelDelayed());
    }

    private System.Collections.IEnumerator ClosePanelDelayed()
    {
        yield return new WaitForSeconds(0.05f);
        gameObject.SetActive(false);
    }
    
}
