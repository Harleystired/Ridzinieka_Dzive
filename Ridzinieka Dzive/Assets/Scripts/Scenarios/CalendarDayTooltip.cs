using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class CalendarDayTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Calendar calendar;
    [SerializeField] private int dayIndex;

    public void Setup(Calendar calendarReference, int index)
    {
        calendar = calendarReference;
        dayIndex = index;

        TMP_Text tmp = GetComponent<TMP_Text>();
        if (tmp != null)
            tmp.raycastTarget = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (calendar == null)
            return;

        calendar.ShowDayTooltip(dayIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (calendar == null)
            return;

        calendar.HideDayTooltip();
    }
}
