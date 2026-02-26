using UnityEngine;

public class Computer : MonoBehaviour, IClickable2D
{
    [SerializeField] GameObject computer; //assigns the UI element
    
    private void Awake() //hides the computer
    {
        if (computer != null)
            computer.SetActive(false);
    }
    
    public void OnClicked(RaycastHit2D hit) //opens the computer upon clicking
    {
        if (computer == null) return;

        bool newState = !computer.activeSelf;
        computer.SetActive(newState);

        if (newState) UIModal.Open(); // makes it so other object can't be clicked through UI
        else UIModal.Close();
    }
    
    public void CloseComputer() //closes the computer
    {
        if (computer == null) return;
        if (!computer.activeSelf) return;

        computer.SetActive(false);

        UIModal.Close(); // allows other objects to be clicked, if this is not done, NOTHING will be clickable
    }
    
}
