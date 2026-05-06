using UnityEngine;

public abstract class BaseWorkComputer : MonoBehaviour, IClickable2D, IWorkComputer
{
    [Header("Computer UI")]
    [SerializeField] protected GameObject computerUI;
    [SerializeField] protected GameObject timeOfDayUI;
    
    [Header("Scenarios")]
    [SerializeField] protected ScenarioManager scenarioManager;
    
    [Header("Job Type")]
    [SerializeField] protected GameManager.JobType jobType;
    
    protected bool isOpen = false;
    
    public bool IsOpen => isOpen;
    public virtual bool CanInteract => !(scenarioManager != null && scenarioManager.IsScenarioActive);
    public GameManager.JobType JobType => jobType;
    
    protected virtual void Awake()
    {
        if (computerUI != null)
            computerUI.SetActive(false);
            
        if (scenarioManager == null)
            scenarioManager = FindFirstObjectByType<ScenarioManager>();
    }
    
    public virtual void OnClicked(RaycastHit2D hit)
    {
        if (computerUI == null) return;
        if (!CanInteract) return;
        
        if (isOpen)
            Close();
        else
            Open();
    }
    
    public virtual void Open()
    {
        if (computerUI == null) return;
        if (isOpen) return;
        if (!CanInteract) return;
        
        // Play sound
        AudioManager.Instance?.PlaySFX(AudioManager.Instance?.pcStart);
        
        computerUI.SetActive(true);
        isOpen = true;
        
        if (timeOfDayUI != null)
            timeOfDayUI.SetActive(false);
        
        UIModal.Open();
        
        // Notify ScenarioManager with job context
        if (scenarioManager != null)
            scenarioManager.NotifyWorkComputerOpened(jobType);
    }
    
    public virtual void Close()
    {
        if (computerUI == null) return;
        if (!isOpen) return;
        if (!CanInteract) return;
        
        computerUI.SetActive(false);
        isOpen = false;
        
        if (timeOfDayUI != null)
            timeOfDayUI.SetActive(true);
        
        UIModal.Close();
        
        if (scenarioManager != null)
            scenarioManager.NotifyWorkComputerClosed(jobType);
    }
}
