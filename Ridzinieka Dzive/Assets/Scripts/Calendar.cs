using TMPro;
using UnityEngine;

public class Calendar : MonoBehaviour, IClickable2D
{   
    [SerializeField] GameObject calendar; //assigns the UI element
    [SerializeField] private GameManager gameManager;
    
    [Range(0f, 1f)]
    [SerializeField] private float previousDayAlpha = 0.5f;
    [SerializeField] private GameObject timeOfDayUI;

    
    private void Awake() //hides the calendar
    {
        if (calendar != null)
            calendar.SetActive(false);
    }
    
    private void OnEnable() // Handles day changes 
    {
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null) gameManager.OnDayChanged += HandleDayChanged;

        RefreshVisuals();
    }
    private void OnDisable()
    {
        if (gameManager != null) gameManager.OnDayChanged -= HandleDayChanged;
    }
    private void HandleDayChanged(int _)
    {
        RefreshVisuals();
    }
    private void RefreshVisuals() // Handles day changes, dimming past days
    {
        if (gameManager == null) return;
        if (gameManager.calendarDay == null) return;

        int current = Mathf.Clamp(gameManager.CurrentDayIndex, 0, gameManager.calendarDay.Length - 1);

        for (int i = 0; i < gameManager.calendarDay.Length; i++)
        {
            GameObject go = gameManager.calendarDay[i];
            if (go == null) continue;

            TMP_Text tmp = go.GetComponent<TMP_Text>();
            if (tmp == null) continue;

            bool isPast = i < current;
            bool isCurrent = i == current;

            // Style
            tmp.fontStyle = isCurrent ? FontStyles.Bold : FontStyles.Normal;

            // Alpha (dim past days)
            Color c = tmp.color;
            c.a = isPast ? previousDayAlpha : 1f;
            tmp.color = c;
        }
    }
    
    public void OnClicked(RaycastHit2D hit) //opens the calendar upon clicking
    {
        if (calendar == null) return;

        bool newState = !calendar.activeSelf;
        calendar.SetActive(newState);
        
        if (timeOfDayUI != null) 
            timeOfDayUI.SetActive(!newState);

        if (newState) UIModal.Open(); // makes it so other object can't be clicked through UI
        else UIModal.Close();
    }
    
    public void CloseCalendar() //closes the calendar
    {
        if (calendar == null) return;
        if (!calendar.activeSelf) return;

        calendar.SetActive(false);

        if (timeOfDayUI != null) 
            timeOfDayUI.SetActive(true);
        UIModal.Close(); // allows other objects to be clicked, if this is not done, NOTHING will be clickable
    }
}
