using UnityEngine;

public class Door : MonoBehaviour, IClickable2D
{   
    [SerializeField] GameObject doorMenu; //assigns the UI element
    [SerializeField] GameObject outsideMenu; //assigns the UI element
    [SerializeField] CameraMovement cameraMovement; // assign the camera movement script
    [SerializeField] GameObject roomArrow; // assign the arrow (so they can be removed)

    [SerializeField] private OutsideUI outsideController; // NEW: the script that now owns the Outside() logic
    [SerializeField] private GameManager gameManager;

    [Header("Job")]
    [SerializeField] private JobManager jobManager;

    [Header("Scenario Blocking")]
    [SerializeField] private ScenarioManager scenarioManager;
    [SerializeField] private SimpleTooltip tooltip;
    
    // makes it so the UI buttons can't be autoclicked upon Ui opening
    [SerializeField] float outsideLockSecondsAfterOpen = 0.35f;
    private float _outsideAllowedAtUnscaledTime;
    

    private void Awake()
    {
        if (doorMenu != null)
            doorMenu.SetActive(false); //hides the UI element
        
        if (outsideMenu != null)
            outsideMenu.SetActive(false); //hides the UI element
        
        if (cameraMovement == null && Camera.main != null)
            cameraMovement = Camera.main.GetComponent<CameraMovement>(); //assigns the camera movement script
        
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (jobManager == null)
            jobManager = FindFirstObjectByType<JobManager>();
    
        if (scenarioManager == null)
            scenarioManager = FindFirstObjectByType<ScenarioManager>();
    
        _outsideAllowedAtUnscaledTime = 0f;
    }

    public void OnClicked(RaycastHit2D hit) //opens the door upon clicking
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.door);
        if (doorMenu == null) return;

        bool newState = !doorMenu.activeSelf;
        doorMenu.SetActive(newState);

        if (newState) UIModal.Open(); // makes it so other object can't be clicked through UI
        else UIModal.Close();
        

    }
    
    public void CloseDoor() //closes the door
    {
        if (doorMenu == null) return;
        if (!doorMenu.activeSelf) return;

        doorMenu.SetActive(false);
        UIModal.Close(); // allows other objects to be clicked, if this is not done, NOTHING will be clickable
    }
    
    public void Outside()
    {
        if (scenarioManager != null && scenarioManager.IsHomeBlocked(out string reason))
        {
            if (tooltip != null) tooltip.Show(reason);
            else Debug.Log(reason);
            return;
        }

        if (doorMenu != null && doorMenu.activeSelf)
        {
            doorMenu.SetActive(false);
            UIModal.Close();
        }

        if (outsideController == null)
        {
            Debug.LogWarning("Door.Outside(): No Outside reference assigned/found.");
            return;
        }

        if (gameManager != null)
            gameManager.EnterOutside();

        outsideController.ShowOutsideMenu();
    }

    public void GoToWork()
    {
        if (gameManager == null)
            return;

        if (scenarioManager != null && scenarioManager.IsHomeBlocked(out string reason))
        {
            if (tooltip != null) tooltip.Show(reason);
            else Debug.Log(reason);
            return;
        }

        if (jobManager == null)
            jobManager = FindFirstObjectByType<JobManager>();

        if (jobManager == null)
        {
            Debug.LogWarning("Door.GoToWork(): JobManager not found.");
            return;
        }

        if (jobManager.IsWorkBlockedToday())
        {
            string message = "Tu izvēlējies šodien palikt mājās.";

            if (tooltip != null) tooltip.Show(message);
            else Debug.Log(message);

            return;
        }

        if (!jobManager.CanWorkToday())
        {
            string message = jobManager.HasWorkedToday
                ? "Tu jau šodien strādāji."
                : "Šodien nav tava darba diena.";

            if (tooltip != null) tooltip.Show(message);
            else Debug.Log(message);

            return;
        }

        bool worked = jobManager.TryWorkToday();

        if (!worked)
        {
            if (tooltip != null) tooltip.Show("Šobrīd nevari doties uz darbu.");
            else Debug.Log("Cannot go to work right now.");

            return;
        }

        gameManager.SetPendingDestination(GameManager.Destination.Work);

        Outside();
    }




    public void GoToShop()
    {
        if (gameManager == null) return;

        if (scenarioManager != null && scenarioManager.IsHomeBlocked(out string reason))
        {
            if (tooltip != null) tooltip.Show(reason);
            else Debug.Log(reason);
            return;
        }

        gameManager.SetPendingDestination(GameManager.Destination.Shop);

        Outside();
    }

}
