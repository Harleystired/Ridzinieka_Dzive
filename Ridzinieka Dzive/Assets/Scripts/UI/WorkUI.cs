using UnityEngine;

public class WorkUI : MonoBehaviour
{
  [Header("Job UIs")]
    [SerializeField] private GameObject officeUI;
    [SerializeField] private GameObject cashierUI;
    [SerializeField] private GameObject taxiUI;

    [Header("Other UIs")]
    [SerializeField] private GameObject outsideUI;

    [Header("Refs")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private OutsideUI outsideUIController;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
        
        if (outsideUIController == null)
            outsideUIController = FindFirstObjectByType<OutsideUI>();

        SetAllJobUIsActive(false);
    }
    public void ReturnHome()
    {
        if (gameManager == null) return;

        gameManager.BeginTravelTo(GameManager.Destination.Home);
        if (outsideUIController != null)
            outsideUIController.ShowOutsideMenu();
    }

    private void OnEnable()
    {
        if (gameManager != null)
            gameManager.OnLocationChanged += HandleLocationChanged;

        // Apply current state immediately (important if this object is enabled mid-game)
        if (gameManager != null)
            HandleLocationChanged(gameManager.CurrentLocation);
    }

    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.OnLocationChanged -= HandleLocationChanged;
    }

    private void HandleLocationChanged(GameManager.Location location)
    {
        if (location == GameManager.Location.Work)
        {
            ShowSelectedJobUI();

            // IMPORTANT: don't SetActive(false) on the outside menu directly (it leaks UIModal)
            if (outsideUIController != null)
                outsideUIController.CloseOutsideMenu();

            return;
        }

        SetAllJobUIsActive(false);
    }

    private void ShowSelectedJobUI()
    {
        SetAllJobUIsActive(false);

        if (gameManager == null)
        {
            // Safe default
            if (officeUI != null) officeUI.SetActive(true);
            return;
        }

        switch (gameManager.SelectedJob)
        {
            case GameManager.JobType.Office:
                if (officeUI != null) officeUI.SetActive(true);
                break;

            case GameManager.JobType.Cashier:
                if (cashierUI != null) cashierUI.SetActive(true);
                break;

            case GameManager.JobType.Taxi:
                if (taxiUI != null) taxiUI.SetActive(true);
                break;

            default:
                if (officeUI != null) officeUI.SetActive(true);
                break;
        }
    }

    private void SetAllJobUIsActive(bool active)
    {
        if (officeUI != null) officeUI.SetActive(active);
        if (cashierUI != null) cashierUI.SetActive(active);
        if (taxiUI != null) taxiUI.SetActive(active);
    }

    // Optional: if you still want button-callable methods for debugging/manual control
    public void Office()
    {
        SetAllJobUIsActive(false);
        if (officeUI != null) officeUI.SetActive(true);
    }

    public void Cashier()
    {
        SetAllJobUIsActive(false);
        if (cashierUI != null) cashierUI.SetActive(true);
    }

    public void Taxi()
    {
        SetAllJobUIsActive(false);
        if (taxiUI != null) taxiUI.SetActive(true);
    }
}
