using System;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
   // ---------------------------------------------------------------------
    // Player Stats / Persistent Data
    // ---------------------------------------------------------------------

    [Header("Player Stats")]
    public int money;
    public int hunger = 0;
    public int energy = 100;
    public int stress = 0;
    public int health = 100;
    public int maxHunger = 100;

    [Header("Sickness")]
    [SerializeField] private int sicknessHealthThreshold = 25;
    [SerializeField] private float firstLowHealthSicknessChance = 0.10f;
    [SerializeField] private float extraLowHealthDayChance = 0.05f;
    [SerializeField] private bool hasFever;
    [SerializeField] private bool isOnSickLeave;
    [SerializeField] private int consecutiveLowHealthDays;
    [SerializeField] private int sickLeaveStartDayIndex = -1;
    [SerializeField] private int sickLeaveDaysUsed;

    public bool HasFever => hasFever;
    public bool IsOnSickLeave => isOnSickLeave;
    public int SickLeaveStartDayIndex => sickLeaveStartDayIndex;
    public int SickLeaveDaysUsed => sickLeaveDaysUsed;

    public event Action OnSicknessChanged;

    [Header("Transport Ownership")]
    public bool oldBike = false;
    public bool newBike = false;
    public bool oldCar = false;
    public bool newCar = false;

    // NEW: prices + break chances
    [Header("Transport Shop Settings")]
    [SerializeField] private int oldBikePrice = 50;
    [SerializeField] private int newBikePrice = 200;
    [SerializeField] private int oldCarPrice = 500;
    [SerializeField] private int newCarPrice = 2000;

    [Tooltip("Chance (0..1) that a USED bike breaks when used for travel.")]
    [Range(0f, 1f)][SerializeField] private float oldBikeBreakChance = 0.15f;

    [Tooltip("Chance (0..1) that a USED car breaks when used for travel.")]
    [Range(0f, 1f)][SerializeField] private float oldCarBreakChance = 0.10f;

    public bool HasAnyBike => oldBike || newBike;
    public bool HasAnyCar => oldCar || newCar;

    public int OldBikePrice => oldBikePrice;
    public int NewBikePrice => newBikePrice;
    public int OldCarPrice => oldCarPrice;
    public int NewCarPrice => newCarPrice;

    [Header("Inventory")]
    public List<string> ownedItems = new List<string>();

    public event Action<int> OnMoneyChanged;

    // ---------------------------------------------------------------------
    // Location / Travel
    // ---------------------------------------------------------------------

    public enum Location
    {
        Home = 0,
        Outside = 1,
        Work = 2,
        Shop = 3
    }

    public enum Destination
    {
        None = 0,
        Home = 1,
        Work = 2,
        Shop = 3
    }

    public enum TransportMode
    {
        Walk = 0,
        PublicTrans = 1,
        Taxi = 2,
        OldBike = 3,
        NewBike = 4,
        OldCar = 5,
        NewCar = 6
    }

    [Header("Player Location")]
    [SerializeField] private Location currentLocation = Location.Home;

    // NEW: remembers where the player was before going Outside (so we can decide time changes)
    [SerializeField] private Location lastNonOutsideLocation = Location.Home;

    [SerializeField] private Destination pendingDestination = Destination.None;

    public Location CurrentLocation => currentLocation;
    public Destination PendingDestination => pendingDestination;

    public bool HasFreshWakeUpMorningScenarioWindow { get; private set; }

    public event Action<Location> OnLocationChanged;
    public event Action<Destination> OnPendingDestinationChanged;

    public void MarkFreshWakeUpMorningScenarioWindow()
    {
        HasFreshWakeUpMorningScenarioWindow = true;
    }

    public void ClearFreshWakeUpMorningScenarioWindow()
    {
        HasFreshWakeUpMorningScenarioWindow = false;
    }

    public void EnterOutside()
    {
        // NEW: capture the location we're leaving (Home/Work/Shop) before switching to Outside
        if (currentLocation != Location.Outside)
            lastNonOutsideLocation = currentLocation;

        SetLocation(Location.Outside);
    }

    public void BeginTravelTo(Destination destination)
    {
        if (destination == Destination.None) return;

        SetPendingDestination(destination);
        EnterOutside();
    }

    public void ConfirmTravel(TransportMode transportMode)
    {
        if (pendingDestination == Destination.None)
        {
            Debug.LogWarning("GameManager.ConfirmTravel(): No pending destination selected.");
            return;
        }

        // NEW: block using vehicles you don't own (safety)
        if ((transportMode == TransportMode.OldBike && !oldBike) ||
            (transportMode == TransportMode.NewBike && !newBike) ||
            (transportMode == TransportMode.OldCar && !oldCar) ||
            (transportMode == TransportMode.NewCar && !newCar))
        {
            Debug.LogWarning($"GameManager.ConfirmTravel(): Tried to use unowned transport: {transportMode}");
            return;
        }

        ApplyTransportStatEffects(transportMode);

        // NEW: used vehicles can break when used
        TryBreakUsedTransport(transportMode);

        Location targetLocation;
        switch (pendingDestination)
        {
            case Destination.Home:
                targetLocation = Location.Home;
                break;
            case Destination.Work:
                targetLocation = Location.Work;
                break;
            case Destination.Shop:
                targetLocation = Location.Shop;
                break;
            default:
                Debug.LogWarning($"GameManager.ConfirmTravel(): Unsupported destination: {pendingDestination}");
                return;
        }

        // NEW: adjust time-of-day based on travel
        ApplyTimeOfDayForTravel(lastNonOutsideLocation, targetLocation);

        ClearPendingDestination();
        SetLocation(targetLocation);

        lastNonOutsideLocation = targetLocation;
    }

    private void ApplyTimeOfDayForTravel(Location from, Location to)
    {
        // Rule: going to Work advances Morning -> Day (Home->Work and Shop->Work)
        if (to == Location.Work && currentTimeOfDay == TimeOfDay.Morning)
        {
            SetTimeOfDay(TimeOfDay.Day);
            return;
        }

        // Rule: leaving Work advances Day -> Evening (Work->Home and Work->Shop)
        if (from == Location.Work &&
            (to == Location.Home || to == Location.Shop) &&
            currentTimeOfDay == TimeOfDay.Day)
        {
            SetTimeOfDay(TimeOfDay.Evening);
            return;
        }

        // Otherwise (Home<->Shop travel etc): no change
    }

    private void TryBreakUsedTransport(TransportMode transportMode)
    {
        switch (transportMode)
        {
            case TransportMode.OldBike:
                if (oldBike && UnityEngine.Random.value < oldBikeBreakChance)
                {
                    oldBike = false;
                    Debug.Log("Used bike broke!");
                }
                break;

            case TransportMode.OldCar:
                if (oldCar && UnityEngine.Random.value < oldCarBreakChance)
                {
                    oldCar = false;
                    Debug.Log("Used car broke!");
                }
                break;

            default:
                break;
        }
    }

    private void ApplyTransportStatEffects(TransportMode transportMode)
    {
        switch (transportMode)
        {
            case TransportMode.Walk:
                RemoveEnergy(5);
                AddHealth(5);
                break;

            case TransportMode.PublicTrans:
                // Example effects - edit to whatever you want:
                // Public transport might reduce stress a bit but costs a little energy.
                SpendMoney(2);
                RemoveStress(3);
                break;

            case TransportMode.Taxi:
                // Example effects - edit to whatever you want:
                // Taxi could reduce stress (comfort) but costs money.
                SpendMoney(10);
                RemoveStress(5);
                break;

            case TransportMode.OldBike:
                AddHealth(10);
                RemoveStress(10);
                break;
                
            case TransportMode.NewBike:
                AddHealth(10);
                RemoveStress(10);
                break;
            
            case TransportMode.OldCar:
                RemoveStress(10);
                break;
            
            case TransportMode.NewCar:
                RemoveStress(10);
                break;
                
            default:
                // No stat changes for bikes/cars (or unknown modes)
                break;
        }
    }

    // NEW: purchase methods
    public bool TryBuyOldBike()
    {
        if (oldBike) return true;
        if (!SpendMoney(oldBikePrice)) return false;
        oldBike = true;
        return true;
    }

    public bool TryBuyNewBike()
    {
        if (newBike) return true;
        if (!SpendMoney(newBikePrice)) return false;
        newBike = true;
        return true;
    }

    public bool TryBuyOldCar()
    {
        if (oldCar) return true;
        if (!SpendMoney(oldCarPrice)) return false;
        oldCar = true;
        return true;
    }

    public bool TryBuyNewCar()
    {
        if (newCar) return true;
        if (!SpendMoney(newCarPrice)) return false;
        newCar = true;
        return true;
    }

    public void SetPendingDestination(Destination destination)
    {
        if (destination == Destination.None) return;
        if (pendingDestination == destination) return;

        pendingDestination = destination;
        OnPendingDestinationChanged?.Invoke(pendingDestination);
    }

    public void ClearPendingDestination()
    {
        if (pendingDestination == Destination.None) return;

        pendingDestination = Destination.None;
        OnPendingDestinationChanged?.Invoke(pendingDestination);
    }

    private void SetLocation(Location newLocation)
    {
        if (currentLocation == newLocation) return;

        currentLocation = newLocation;

        if (currentLocation != Location.Home)
            ClearFreshWakeUpMorningScenarioWindow();

        OnLocationChanged?.Invoke(currentLocation);
    }

    // --- Time of Day ---
    public float morning;
    public float day;
    public float evening;
    public float night;

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

        if (currentTimeOfDay != TimeOfDay.Morning)
            ClearFreshWakeUpMorningScenarioWindow();

        OnTimeOfDayChanged?.Invoke(currentTimeOfDay);
    }

    public void AdvanceTimeOfDay()
    {
        int next = ((int)currentTimeOfDay + 1) % 4;
        SetTimeOfDay((TimeOfDay)next);
    }

    // ---------------------------------------------------------------------
    // Job
    // ---------------------------------------------------------------------

    public enum JobType
    {
        Cashier = 0,
        Taxi = 1,
        Office = 2
    }

    [Header("Job")]
    [SerializeField] private JobType selectedJob = JobType.Cashier;

    public JobType SelectedJob => selectedJob;

    public event Action<JobType> OnSelectedJobChanged;

    public void SetSelectedJobFromIndex(int jobIndex)
    {
        if (!Enum.IsDefined(typeof(JobType), jobIndex))
        {
            Debug.LogWarning($"GameManager.SetSelectedJobFromIndex(): Invalid jobIndex: {jobIndex}");
            return;
        }

        JobType newJob = (JobType)jobIndex;
        if (selectedJob == newJob) return;

        selectedJob = newJob;
        OnSelectedJobChanged?.Invoke(selectedJob);
    }

    // ---------------------------------------------------------------------
    // Calendar
    // ---------------------------------------------------------------------

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

    public bool IsSickCalendarDay(int dayIndex)
    {
        if (!hasFever)
            return false;

        if (isOnSickLeave)
            return sickLeaveStartDayIndex >= 0 &&
                   dayIndex >= sickLeaveStartDayIndex &&
                   dayIndex < sickLeaveStartDayIndex + 3;

        return dayIndex == currentDayIndex;
    }

    public bool IsSickLeaveCalendarDay(int dayIndex)
    {
        if (!hasFever || !isOnSickLeave)
            return false;

        return sickLeaveStartDayIndex >= 0 &&
               dayIndex >= sickLeaveStartDayIndex &&
               dayIndex < sickLeaveStartDayIndex + 3;
    }

    public void StartSickLeave()
    {
        if (!hasFever)
            return;

        if (!isOnSickLeave)
        {
            isOnSickLeave = true;
            sickLeaveStartDayIndex = currentDayIndex;
            sickLeaveDaysUsed = 0;
        }

        ApplySickLeaveDayBenefits();
        OnSicknessChanged?.Invoke();
    }

    public void GoToWorkWhileSick()
    {
        if (!hasFever)
            return;

        RemoveHealth(1);
        OnSicknessChanged?.Invoke();
    }

    private void ProcessSicknessForNewDay()
    {
        if (hasFever)
        {
            if (isOnSickLeave)
            {
                ApplySickLeaveDayBenefits();

                if (sickLeaveDaysUsed >= 3)
                    RecoverFromFever();
            }
            else
            {
                OnSicknessChanged?.Invoke();
            }

            return;
        }

        RollForSicknessFromLowHealth();
    }

    private void RollForSicknessFromLowHealth()
    {
        if (health > sicknessHealthThreshold)
        {
            consecutiveLowHealthDays = 0;
            return;
        }

        consecutiveLowHealthDays++;

        float sicknessChance = firstLowHealthSicknessChance +
                               extraLowHealthDayChance * (consecutiveLowHealthDays - 1);

        sicknessChance = Mathf.Clamp01(sicknessChance);

        if (UnityEngine.Random.value <= sicknessChance)
            BecomeSick();
    }

    private void BecomeSick()
    {
        if (hasFever)
            return;

        hasFever = true;
        isOnSickLeave = false;
        sickLeaveStartDayIndex = -1;
        sickLeaveDaysUsed = 0;

        OnSicknessChanged?.Invoke();

        Debug.Log("Player got a fever.");
    }

    private void RecoverFromFever()
    {
        hasFever = false;
        isOnSickLeave = false;
        consecutiveLowHealthDays = 0;
        sickLeaveStartDayIndex = -1;
        sickLeaveDaysUsed = 0;

        OnSicknessChanged?.Invoke();

        Debug.Log("Player recovered from fever.");
    }

    private void ApplySickLeaveDayBenefits()
    {
        if (!hasFever || !isOnSickLeave)
            return;

        if (sickLeaveDaysUsed >= 3)
            return;

        AddHealth(5);
        sickLeaveDaysUsed++;

        JobManager jobManager = FindFirstObjectByType<JobManager>();
        if (jobManager != null && jobManager.IsWorkDay(currentDayIndex))
            jobManager.AddSickLeavePayForToday();

        Debug.Log($"Sick leave day {sickLeaveDaysUsed}/3. Health restored by 5.");
    }

    public void AdvanceDay()
    {
        if (calendarDay == null || calendarDay.Length == 0) return;

        int maxIndex = Mathf.Min(30, calendarDay.Length - 1);
        if (currentDayIndex >= maxIndex) return; // already at last day

        currentDayIndex++;

        ProcessSicknessForNewDay();

        OnDayChanged?.Invoke(currentDayIndex);
    }

    // ---------------------------------------------------------------------
    // Money / Needs
    // ---------------------------------------------------------------------

    private const int StatMin = 0;
    private const int StatMax = 100;

    private static int ClampStat(int value)
    {
        return Mathf.Clamp(value, StatMin, StatMax);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        hunger = ClampStat(hunger);
        energy = ClampStat(energy);
        stress = ClampStat(stress);
        health = ClampStat(health);
    }
#endif

    public void AddMoney(int amount)
    {
        money += amount;
        OnMoneyChanged?.Invoke(money);
    }

    public bool SpendMoney(int amount)
    {
        if (money < amount) return false;

        money -= amount;
        OnMoneyChanged?.Invoke(money);
        return true;
    }

    public void AddHunger(int amount)
    {
        hunger = ClampStat(hunger + amount);
    }

    public void RemoveHunger(int amount)
    {
        hunger = ClampStat(hunger - amount);
    }

    public void AddStress(int amount)
    {
        stress = ClampStat(stress + amount);
    }

    public void RemoveStress(int amount)
    {
        stress = ClampStat(stress - amount);
    }

    public void AddHealth(int amount)
    {
        health = ClampStat(health + amount);
    }

    public void RemoveHealth(int amount)
    {
        health = ClampStat(health - amount);
    }

    public void AddEnergy(int amount)
    {
        energy = ClampStat(energy + amount);
    }

    public void RemoveEnergy(int amount)
    {
        energy = ClampStat(energy - amount);
    }
}
