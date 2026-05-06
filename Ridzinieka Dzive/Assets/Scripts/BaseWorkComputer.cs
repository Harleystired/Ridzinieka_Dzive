using UnityEngine;

public abstract class BaseWorkComputer : MonoBehaviour, IClickable2D, IHoverable2D
{
    [Header("Computer UI")]
    [SerializeField] protected GameObject computerUI;
    [SerializeField] protected GameObject timeOfDayUI;
    
    [Header("Scenarios")]
    [SerializeField] protected ScenarioManager scenarioManager;
    
    [Header("Job Type")]
    [SerializeField] protected GameManager.JobType jobType;
    
    [Header("Auto-Close")]
    [SerializeField] protected bool autoCloseWhenDone = true;
    
    protected bool isOpen = false;
    
    public bool IsOpen => isOpen;
    public GameManager.JobType JobType => jobType;
    
    protected virtual void Awake()
    {
        if (computerUI != null)
            computerUI.SetActive(false);
            
        if (scenarioManager == null)
            scenarioManager = FindFirstObjectByType<ScenarioManager>();
    }
    
    protected virtual void OnEnable()
    {
        // Listen for scenario completion
        if (scenarioManager != null)
        {
            scenarioManager.ScenarioActiveChanged += OnScenarioActiveChanged;
        }
    }
    
    protected virtual void OnDisable()
    {
        // Stop listening
        if (scenarioManager != null)
        {
            scenarioManager.ScenarioActiveChanged -= OnScenarioActiveChanged;
        }
    }
    
    // Alternative: Check queue directly and close immediately
    protected virtual void OnScenarioActiveChanged(bool isActive)
    {
        if (!isActive && autoCloseWhenDone && isOpen)
        {
            // Check if there are no more scenarios pending
            if (scenarioManager != null && scenarioManager.HasNoPendingScenariosAtWork)
            {
                CloseComputer();
            }
        }
    }
    
    protected virtual void DelayedClose()
    {
        // Only close if still open and no scenario active
        if (autoCloseWhenDone && isOpen && scenarioManager != null && !scenarioManager.IsScenarioActive)
        {
            CloseComputer();
        }
    }
    
    public virtual void OnClicked(RaycastHit2D hit)
    {
        if (computerUI == null) return;
        
        bool newState = !computerUI.activeSelf;
        
        // Don't allow closing if a scenario is active
        if (!newState && scenarioManager != null && scenarioManager.IsScenarioActive)
            return;
        
        // Play sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.pcStart);
        
        computerUI.SetActive(newState);
        isOpen = newState;
        
        if (timeOfDayUI != null)
            timeOfDayUI.SetActive(!newState);
        
        if (newState)
        {
            UIModal.Open();
            if (scenarioManager != null) 
                scenarioManager.NotifyWorkComputerOpened(jobType);
        }
        else
        {
            UIModal.Close();
            if (scenarioManager != null) 
                scenarioManager.NotifyWorkComputerClosed(jobType);
        }
    }
    
    
    
    public virtual void OnHoverEnter(RaycastHit2D hit)
    {
        // Optional: Add hover effect
    }
    
    public virtual void OnHoverExit()
    {
        // Optional: Add hover effect
    }
    
    public virtual void CloseComputer()
    {
        if (computerUI == null) return;
        if (!computerUI.activeSelf) return;
        
        if (scenarioManager != null && scenarioManager.IsScenarioActive)
            return;
        
        computerUI.SetActive(false);
        isOpen = false;
        
        if (timeOfDayUI != null)
            timeOfDayUI.SetActive(true);
        
        UIModal.Close();
        
        if (scenarioManager != null) 
            scenarioManager.NotifyWorkComputerClosed(jobType);
    }
}
