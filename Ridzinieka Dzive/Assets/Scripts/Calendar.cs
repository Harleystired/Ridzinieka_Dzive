using UnityEngine;

public class Calendar : MonoBehaviour, IClickable2D
{   [SerializeField] GameObject calendar;
    private IClickable2D _clickable2DImplementation;

    private void Awake()
    {
        if (calendar != null)
            calendar.SetActive(false);
    }

    public void OnClicked(RaycastHit2D hit)
    {
        if (calendar == null) return;

        // Toggle on click (or use SetActive(true) if you never want to close it)
        calendar.SetActive(!calendar.activeSelf);
    }
    
    public void CloseCalendar()
    {
        calendar.SetActive(false);
    }
}
