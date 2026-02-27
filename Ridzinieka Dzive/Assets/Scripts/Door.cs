using UnityEngine;

public class Door : MonoBehaviour, IClickable2D
{   [SerializeField] GameObject doorMenu; //assigns the UI element
    [SerializeField] GameObject outsideMenu; //assigns the UI element
    [SerializeField] CameraMovement cameraMovement; // assign the camera movement script
    [SerializeField] GameObject roomArrow; // assign the arrow (so they can be removed)
    [SerializeField] private GameObject shopPanel;

    
    // makes it so the UI buttons can't be autoclicked upon Ui opening
    [SerializeField] float outsideLockSecondsAfterOpen = 0.35f;
    private float _outsideAllowedAtUnscaledTime;
    
    //WIP
    public bool work = false;
    public bool shop = false;

    private void Awake()
    {
        if (doorMenu != null)
            doorMenu.SetActive(false); //hides the UI element
        
        if (outsideMenu != null)
            outsideMenu.SetActive(false); //hides the UI element
        
        if (cameraMovement == null && Camera.main != null)
            cameraMovement = Camera.main.GetComponent<CameraMovement>(); //assigns the camera movement script
        
        _outsideAllowedAtUnscaledTime = 0f;
    }

    public void OnClicked(RaycastHit2D hit) //opens the door upon clicking
    {
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
    
    public void Outside() //opens the outside menu
    {
        if (Time.unscaledTime < _outsideAllowedAtUnscaledTime)
            return;
        
        if (doorMenu == null) return;
        if (!doorMenu.activeSelf) return;

        doorMenu.SetActive(false);
        roomArrow.SetActive(false);
        
        if (cameraMovement != null) // moves the camera to the outside
            cameraMovement.Outside();
        else
            Debug.LogWarning("Door.Outside(): No CameraMovement reference assigned/found.");
        
        outsideMenu.SetActive(true);
    }
    public void OpenShop()
    {
        if (shopPanel == null) return;

        shopPanel.SetActive(true);
        UIModal.Open(); 
    }
    public void CloseShop()
    {
        if (shopPanel == null) return;
        ShopManager.Instance.ClearCart();
        shopPanel.SetActive(false);
        UIModal.Close();
    }

}
