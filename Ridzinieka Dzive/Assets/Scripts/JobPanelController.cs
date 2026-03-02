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

        // Apply stat changes
        gameManager.hunger += job.hungerChange;
        gameManager.energy += job.energyChange;
        gameManager.stress += job.stressChange;
        gameManager.health += job.healthChange;
        gameManager.money += job.moneyEarned;

        Debug.Log("Chosen job: " + job.title);

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
