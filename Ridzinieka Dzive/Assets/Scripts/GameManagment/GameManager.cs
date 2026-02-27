using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // stores the game data, add any extra data you wan't to store'
    
    public int money;
    public int hunger = 100;
    public int energy = 100;
    public int stress = 0;
    public int health = 100;
    
    
    public bool oldBike = false;
    public bool newBike = false;
    public bool oldCar = false;
    public bool newCar = false;
    
    // --- Time of Day ---
    public float morning;
    public float day;
    public float evening;
    public float night;
    public event Action<int> OnMoneyChanged;
    public enum TimeOfDay
    {
        Morning = 0,
        Day = 1,
        Evening = 2,
        Night = 3
    }

    [Header("Time of Day")]
    [SerializeField] private TimeOfDay currentTimeOfDay = TimeOfDay.Morning;
    public TimeOfDay CurrentTime => currentTimeOfDay;

    public event Action<TimeOfDay> OnTimeOfDayChanged;

    public void SetTimeOfDay(TimeOfDay value)
    {
        if (currentTimeOfDay == value) return;
        currentTimeOfDay = value;
        OnTimeOfDayChanged?.Invoke(currentTimeOfDay);
    }

    public void AdvanceTimeOfDay()
    {
        int next = ((int)currentTimeOfDay + 1) % 4;
        SetTimeOfDay((TimeOfDay)next);
    }
    
    // --- Calendar ---
    [Header("Calendar")] //Changes the day of the calendar
    public GameObject[] calendarDay;
    [SerializeField] private int currentDayIndex = 0; // 0..30

    public int CurrentDayIndex => currentDayIndex;

    public event Action<int> OnDayChanged;

    private void Start()
    {
        OnDayChanged?.Invoke(currentDayIndex);
    }

    public void AdvanceDay()
    {
        if (calendarDay == null || calendarDay.Length == 0) return;

        int maxIndex = Mathf.Min(30, calendarDay.Length - 1);
        if (currentDayIndex >= maxIndex) return; // already at last day

        currentDayIndex++;
        OnDayChanged?.Invoke(currentDayIndex);
    }
    public void AddMoney(int amount)
    {
        money += amount;
        OnMoneyChanged?.Invoke(money);
    }

    public bool SpendMoney(int amount)
    {
        if (money < amount)
            return false;

        money -= amount;
        OnMoneyChanged?.Invoke(money);
        return true;
    }

}

