using UnityEngine;

public class OutsideUI : MonoBehaviour
{
    [SerializeField] private GameObject doorMenu;     // the door UI to close
    [SerializeField] private GameObject outsideMenu;  // the outside UI to open
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private GameObject roomArrow;
    [SerializeField] private GameManager gameManager;
    
    // makes it so the UI buttons can't be autoclicked upon Ui opening
    [SerializeField] private float outsideLockSecondsAfterOpen = 0.35f;
    private float _outsideAllowedAtUnscaledTime;
    

    private void Awake()
    {
        _outsideAllowedAtUnscaledTime = 0f;

        if (cameraMovement == null && Camera.main != null)
            cameraMovement = Camera.main.GetComponent<CameraMovement>();
        
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
    }

    // Call this from Door.cs (or directly from a UI Button if you want later)
    public void Outside()
    {
        if (Time.unscaledTime < _outsideAllowedAtUnscaledTime)
            return;

        if (doorMenu == null) return;
        if (!doorMenu.activeSelf) return;

        doorMenu.SetActive(false);

        if (roomArrow != null)
            roomArrow.SetActive(false);

        if (cameraMovement != null) // moves the camera to the outside
            cameraMovement.Outside();
        else
            Debug.LogWarning("Outside.Outside(): No CameraMovement reference assigned/found.");

        if (outsideMenu != null)
            outsideMenu.SetActive(true);
    }
    
    public void ChooseWalk() => ChooseTransport(GameManager.TransportMode.Walk);
    public void ChoosePublicTrans() => ChooseTransport(GameManager.TransportMode.PublicTrans);
    public void ChooseTaxi() => ChooseTransport(GameManager.TransportMode.Taxi);
    public void ChooseOldBike() => ChooseTransport(GameManager.TransportMode.OldBike);
    public void ChooseOldCar() => ChooseTransport(GameManager.TransportMode.OldCar);

    private void ChooseTransport(GameManager.TransportMode mode)
    {
        if (gameManager == null) return;

        gameManager.ConfirmTravel(mode);

        // You can close/hide the outside menu here after travel is confirmed
        // (or drive this from OnLocationChanged later if you prefer)
        if (outsideMenu != null)
            outsideMenu.SetActive(false);
    }
}
