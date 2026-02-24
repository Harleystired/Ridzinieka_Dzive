using UnityEngine;

public class Calendar : MonoBehaviour, IClickable2D
{   
    [SerializeField] GameObject calendar;
    
    
    public static bool IsCalendarOpen { get; private set; }
    private void Awake()
    {
        if (calendar != null)
            calendar.SetActive(false);
    }

    public void OnClicked(RaycastHit2D hit)
    {
        if (calendar == null) return;

        bool newState = !calendar.activeSelf;
        calendar.SetActive(newState);

        if (newState) UIModal.Open();
        else UIModal.Close();
    }
    
    public void CloseCalendar()
    {
        if (calendar == null) return;
        if (!calendar.activeSelf) return;

        calendar.SetActive(false);

        UIModal.Close();
    }
}
