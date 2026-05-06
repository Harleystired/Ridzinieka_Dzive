using UnityEngine;
using UnityEngine.UI;
using TMPro; // if using TextMeshPro

public class WorkScenarioPanel : MonoBehaviour, IWorkScenarioPanel
{
    [Header("UI References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Button choice1Button;
    [SerializeField] private Button choice2Button;
    [SerializeField] private Button choice3Button;
    [SerializeField] private TextMeshProUGUI choice1Text;
    [SerializeField] private TextMeshProUGUI choice2Text;
    [SerializeField] private TextMeshProUGUI choice3Text;
    [SerializeField] private Button closeButton;
    
    [Header("Job-Specific Styling")]
    [SerializeField] private Color cashierColor = Color.white;
    [SerializeField] private Color officeColor = Color.white;
    [SerializeField] private Color taxiColor = Color.white;
    
    private System.Action<int> onChoiceMade;
    private GameManager.JobType currentJob;
    
    private void Awake()
    {
        panelRoot?.SetActive(false);
        
        choice1Button?.onClick.AddListener(() => OnChoiceSelected(0));
        choice2Button?.onClick.AddListener(() => OnChoiceSelected(1));
        if (choice3Button != null)
            choice3Button.onClick.AddListener(() => OnChoiceSelected(2));
        
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }
    
    public void Show(string prompt, string choice1, string choice2, string choice3, System.Action<int> onChoiceMade)
    {
        ShowForJob(prompt, choice1, choice2, choice3, onChoiceMade, currentJob);
    }
    
    public void ShowForJob(string prompt, string choice1, string choice2, string choice3, 
                          System.Action<int> onChoiceMade, GameManager.JobType jobType)
    {
        this.onChoiceMade = onChoiceMade;
        this.currentJob = jobType;
        
        if (promptText != null)
            promptText.text = prompt;
        
        if (choice1Text != null)
            choice1Text.text = choice1;
        if (choice2Text != null)
            choice2Text.text = choice2;
        if (choice3Button != null && choice3Text != null)
        {
            bool hasThird = !string.IsNullOrEmpty(choice3);
            choice3Button.gameObject.SetActive(hasThird);
            if (hasThird)
                choice3Text.text = choice3;
        }
        
        ApplyJobStyling(jobType);
        panelRoot?.SetActive(true);
    }
    
    public void Hide()
    {
        panelRoot?.SetActive(false);
    }

    public bool IsVisible { get; }

    public void SetJobContext(GameManager.JobType jobType)
    {
        currentJob = jobType;
        ApplyJobStyling(jobType);
    }
    
    private void ApplyJobStyling(GameManager.JobType jobType)
    {
        // Apply color based on job type - customize as needed
        Color targetColor = cashierColor;
        switch (jobType)
        {
            case GameManager.JobType.Cashier:
                targetColor = cashierColor;
                break;
            case GameManager.JobType.Office:
                targetColor = officeColor;
                break;
            case GameManager.JobType.Taxi:
                targetColor = taxiColor;
                break;
        }
        
        // Apply to background or specific elements
        if (panelRoot != null)
        {
            var image = panelRoot.GetComponent<Image>();
            if (image != null)
                image.color = targetColor;
        }
    }
    
    private void OnChoiceSelected(int index)
    {
        onChoiceMade?.Invoke(index);
    }
}
