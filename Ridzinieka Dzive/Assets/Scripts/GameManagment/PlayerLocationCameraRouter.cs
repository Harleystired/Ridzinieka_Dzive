using UnityEngine;

public class PlayerLocationCameraRouter : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CameraMovement cameraMovement;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (cameraMovement == null && Camera.main != null)
            cameraMovement = Camera.main.GetComponent<CameraMovement>();
    }

    private void OnEnable()
    {
        if (gameManager != null)
            gameManager.OnLocationChanged += HandleLocationChanged;

        // Apply current state immediately (important on scene load)
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
        if (cameraMovement == null) return;

        switch (location)
        {
            case GameManager.Location.Home:
                cameraMovement.Computer();
                break;

            case GameManager.Location.Outside:
                cameraMovement.Outside();
                break;

            case GameManager.Location.Work:
                MoveCameraToSelectedJob();
                break;

            case GameManager.Location.Shop:
                cameraMovement.Shop();
                break;

            default:
                cameraMovement.Computer();
                break;
        }
    }
    private void MoveCameraToSelectedJob()
    {
        if (gameManager == null)
        {
            cameraMovement.workOffice();
            return;
        }

        switch (gameManager.SelectedJob)
        {
            case GameManager.JobType.Cashier:
                cameraMovement.workCashier();
                break;

            case GameManager.JobType.Taxi:
                cameraMovement.workTaxi();
                break;

            case GameManager.JobType.Office:
                cameraMovement.workOffice();
                break;

            default:
                cameraMovement.workOffice();
                break;
        }
    }
}
