using UnityEngine;

public class OutsideUI : MonoBehaviour
{
    [SerializeField] private GameObject doorMenu;     // the door UI to close
    [SerializeField] private GameObject outsideMenu;  // the outside UI to open
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private GameManager gameManager;

    // NEW: popup reference
    [SerializeField] private TransportPurchasePopup transportPopup;

    [SerializeField] private float outsideLockSecondsAfterOpen = 0.35f;
    private float _outsideAllowedAtUnscaledTime;

    private void Awake()
    {
        _outsideAllowedAtUnscaledTime = 0f;

        if (cameraMovement == null && Camera.main != null)
            cameraMovement = Camera.main.GetComponent<CameraMovement>();

        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (transportPopup == null)
            transportPopup = FindFirstObjectByType<TransportPurchasePopup>();
    }

    private void OnEnable()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager != null)
            gameManager.OnLocationChanged += HandleLocationChanged;

        if (gameManager != null)
            HandleLocationChanged(gameManager.CurrentLocation);
    }

    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.OnLocationChanged -= HandleLocationChanged;

        CloseOutsideMenu();
    }

    private void HandleLocationChanged(GameManager.Location location)
    {
        // If we're not outside anymore, make sure the outside UI is closed and modal released.
        if (location != GameManager.Location.Outside)
            CloseOutsideMenu();
    }
    
    // Call this from Door.cs (or directly from a UI Button if you want later)
    public void Outside()
    {
        if (Time.unscaledTime < _outsideAllowedAtUnscaledTime)
        return;

        // If called while door menu is open, close it (optional)
        if (doorMenu != null && doorMenu.activeSelf)
            doorMenu.SetActive(false);

        if (cameraMovement != null)
            cameraMovement.Outside();

        SetOutsideMenuActive(true);
    }
   
    public void ShowOutsideMenu()
    {
        SetOutsideMenuActive(true);
    }
    
    public void CloseOutsideMenu()
    {
        SetOutsideMenuActive(false);
    }
    
    private void SetOutsideMenuActive(bool active)
    {
        if (outsideMenu == null) return;

        bool wasActive = outsideMenu.activeSelf;
        if (wasActive == active) return;

        outsideMenu.SetActive(active);

        if (active) UIModal.Open();
        else UIModal.Close();
    } 
    
    public void ChooseWalk() => ChooseTransport(GameManager.TransportMode.Walk);
    public void ChoosePublicTrans() => ChooseTransport(GameManager.TransportMode.PublicTrans);
    public void ChooseTaxi() => ChooseTransport(GameManager.TransportMode.Taxi);

    // NEW: these are the ones you wire to your Bike/Car buttons
    public void ChooseBike()
    {
        if (transportPopup == null) return;
        transportPopup.OpenBike();
    }

    public void ChooseCar()
    {
        if (transportPopup == null) return;
        transportPopup.OpenCar();
    }

    private void ChooseTransport(GameManager.TransportMode mode)
    {
        if (gameManager == null) return;

        gameManager.ConfirmTravel(mode);

        CloseOutsideMenu();
    }
}
