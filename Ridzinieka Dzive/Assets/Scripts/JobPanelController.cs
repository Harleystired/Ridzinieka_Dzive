using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JobPanelController : MonoBehaviour
{
    [System.Serializable]
    public class JobData
    {
        public string title;
        public string description;
        public string salary;
        public string hours;
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
    public TextMeshProUGUI balanceText;

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
        balanceText.text = "€" + gameManager.money;
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
        Debug.Log("Chosen job: " + jobs[currentJobIndex].title);
        // Te vēlāk varēsi pievienot statiem ietekmi
        // UIManager.Instance.Show("NextPanel");
    }
}
