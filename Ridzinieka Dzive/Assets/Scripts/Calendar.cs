using System.Text;
using TMPro;
using UnityEngine;

public class Calendar : MonoBehaviour, IClickable2D
{   
    [SerializeField] GameObject calendar; //assigns the UI element
    [SerializeField] private GameManager gameManager;

    [Header("Job")]
    [SerializeField] private JobManager jobManager;
    [SerializeField] private Color normalDayColor = Color.black;
    [SerializeField] private Color workDayColor = Color.red;

    [Header("Rent")]
    [SerializeField] private RentManager rentManager;

    [Header("Tooltip")]
    [SerializeField] private SimpleTooltip calendarTooltip;

    [Header("Scenarios")]
    [SerializeField] private ScenarioManager scenarioManager;

    [Range(0f, 1f)]
    [SerializeField] private float previousDayAlpha = 0.5f;
    [SerializeField] private GameObject timeOfDayUI;


    private void Awake() //hides the calendar
    {
        if (calendar != null)
            calendar.SetActive(false);

        if (scenarioManager == null)
            scenarioManager = FindFirstObjectByType<ScenarioManager>();

        if (jobManager == null)
            jobManager = FindFirstObjectByType<JobManager>();

        if (rentManager == null)
            rentManager = FindFirstObjectByType<RentManager>();

        if (calendarTooltip == null)
            calendarTooltip = FindFirstObjectByType<SimpleTooltip>();

        SetupDayTooltips();
    }

    private void OnEnable() // Handles day changes 
    {
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnDayChanged += HandleDayChanged;
            gameManager.OnSelectedJobChanged += HandleSelectedJobChanged;
        }

        if (jobManager == null)
            jobManager = FindFirstObjectByType<JobManager>();

        if (rentManager == null)
            rentManager = FindFirstObjectByType<RentManager>();

        SetupDayTooltips();

        RefreshVisuals();
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnDayChanged -= HandleDayChanged;
            gameManager.OnSelectedJobChanged -= HandleSelectedJobChanged;
        }
    }

    private void HandleDayChanged(int _)
    {
        RefreshVisuals();
    }

    private void HandleSelectedJobChanged(GameManager.JobType _)
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
            bool isWorkDay = jobManager != null && jobManager.IsWorkDay(i);
            bool isRentDay = rentManager != null && rentManager.IsRentDay(i);

            // Style
            FontStyles style = FontStyles.Normal;

            if (isCurrent)
                style |= FontStyles.Bold;

            if (isRentDay)
                style |= FontStyles.Underline;

            tmp.fontStyle = style;

            // Color
            Color c = isWorkDay ? workDayColor : normalDayColor;

            // Alpha (dim past days)
            c.a = isPast ? previousDayAlpha : 1f;
            tmp.color = c;
        }
    }

    private void SetupDayTooltips()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager == null || gameManager.calendarDay == null)
            return;

        for (int i = 0; i < gameManager.calendarDay.Length; i++)
        {
            GameObject go = gameManager.calendarDay[i];
            if (go == null) continue;

            CalendarDayTooltip dayTooltip = go.GetComponent<CalendarDayTooltip>();
            if (dayTooltip == null)
                dayTooltip = go.AddComponent<CalendarDayTooltip>();

            dayTooltip.Setup(this, i);
        }
    }

    public void ShowDayTooltip(int dayIndex)
    {
        if (calendarTooltip == null)
            calendarTooltip = FindFirstObjectByType<SimpleTooltip>();

        if (calendarTooltip == null)
            return;

        string message = BuildDayTooltipMessage(dayIndex);

        if (string.IsNullOrWhiteSpace(message))
            return;

        calendarTooltip.ShowAtCursor(message, 999f);
    }

    public void HideDayTooltip()
    {
        if (calendarTooltip != null)
            calendarTooltip.Hide();
    }

    private string BuildDayTooltipMessage(int dayIndex)
    {
        if (gameManager == null)
            return "";

        StringBuilder sb = new StringBuilder();

        bool isPast = dayIndex < gameManager.CurrentDayIndex;
        bool isCurrent = dayIndex == gameManager.CurrentDayIndex;
        bool isWorkDay = jobManager != null && jobManager.IsWorkDay(dayIndex);
        bool isRentDay = rentManager != null && rentManager.IsRentDay(dayIndex);

        if (isCurrent)
            sb.AppendLine("Šodiena");

        if (isWorkDay)
            sb.AppendLine("Darba diena");

        if (isRentDay)
            sb.AppendLine("Īres diena");

        if (isPast && !isCurrent)
            sb.AppendLine("Pagājušā diena");

        if (sb.Length == 0)
            sb.AppendLine("Parasta diena");

        return sb.ToString().TrimEnd();
    }

    public void OnClicked(RaycastHit2D hit) //opens the calendar upon clicking
    {
        if (calendar == null) return;

        bool newState = !calendar.activeSelf;

        if (!newState)
            HideDayTooltip();

        calendar.SetActive(newState);

        if (scenarioManager != null)
            scenarioManager.NotifyCalendarOpenStateChanged(newState);
    
        if (timeOfDayUI != null) 
            timeOfDayUI.SetActive(!newState);

        if (newState) UIModal.Open(); // makes it so other object can't be clicked through UI
        else UIModal.Close();
    }

    public void CloseCalendar() //closes the calendar
    {
        if (calendar == null) return;
        if (!calendar.activeSelf) return;

        HideDayTooltip();

        calendar.SetActive(false);

        if (scenarioManager != null)
            scenarioManager.NotifyCalendarOpenStateChanged(false);

        if (timeOfDayUI != null) 
            timeOfDayUI.SetActive(true);
        UIModal.Close(); // allows other objects to be clicked, if this is not done, NOTHING will be clickable
    }
}
