using UnityEngine;

public class Calendar : MonoBehaviour, IClickable2D
{   
    [SerializeField] GameObject calendar; //assigns the UI element
    
    private void Awake() //hides the calendar
    {
        if (calendar != null)
            calendar.SetActive(false);
    }

    public void OnClicked(RaycastHit2D hit) //opens the calendar upon clicking
    {
        if (calendar == null) return;

        bool newState = !calendar.activeSelf;
        calendar.SetActive(newState);

        if (newState) UIModal.Open(); // makes it so other object can't be clicked through UI
        else UIModal.Close();
    }
    
    public void CloseCalendar() //closes the calendar
    {
        if (calendar == null) return;
        if (!calendar.activeSelf) return;

        calendar.SetActive(false);

        UIModal.Close(); // allows other objects to be clicked, if this is not done, NOTHING will be clickable
    }
}
