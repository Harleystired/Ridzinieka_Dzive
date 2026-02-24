using UnityEngine;

public class Door : MonoBehaviour, IClickable2D
{   [SerializeField] GameObject doorMenu;
    private IClickable2D _clickable2DImplementation;
    [SerializeField] CameraMovement cameraMovement;
    [SerializeField] GameObject roomArrow;
    
    public bool work = false;
    public bool shop = false;

    private void Awake()
    {
        if (doorMenu != null)
            doorMenu.SetActive(false);
        
        if (cameraMovement == null && Camera.main != null)
            cameraMovement = Camera.main.GetComponent<CameraMovement>();
    }

    public void OnClicked(RaycastHit2D hit)
    {
        if (doorMenu == null) return;

        bool newState = !doorMenu.activeSelf;
        doorMenu.SetActive(newState);

        if (newState) UIModal.Open();
        else UIModal.Close();
    }
    
    public void CloseDoor()
    {
        if (doorMenu == null) return;
        if (!doorMenu.activeSelf) return;

        doorMenu.SetActive(false);
        UIModal.Close();
    }
    
    public void Outside()
    {
        if (doorMenu == null) return;
        if (!doorMenu.activeSelf) return;

        doorMenu.SetActive(false);
        roomArrow.SetActive(false);
        
        if (cameraMovement != null)
            cameraMovement.Outside();
        else
            Debug.LogWarning("Door.Outside(): No CameraMovement reference assigned/found.");
    }
}
