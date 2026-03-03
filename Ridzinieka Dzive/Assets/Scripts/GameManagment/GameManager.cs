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
        Work = 1,
        Shop = 2
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
    [SerializeField] private Destination pendingDestination = Destination.None;

    public Location CurrentLocation => currentLocation;
    public Destination PendingDestination => pendingDestination;

    public event Action<Location> OnLocationChanged;
    public event Action<Destination> OnPendingDestinationChanged;
    
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
    
    public void SetPendingDestination(Destination destination)
    {
        if (destination == Destination.None)
            return;

        if (pendingDestination == destination)
            return;

        pendingDestination = destination;
        OnPendingDestinationChanged?.Invoke(pendingDestination);
    }

    public void ClearPendingDestination()
    {
        if (pendingDestination == Destination.None) return;
        pendingDestination = Destination.None;
        OnPendingDestinationChanged?.Invoke(pendingDestination);
    }

    public void EnterOutside()
    {
        SetLocation(Location.Outside);
    }
    public void ConfirmTravel(TransportMode transportMode)
    {
        if (pendingDestination == Destination.None)
        {
            Debug.LogWarning("GameManager.ConfirmTravel(): No pending destination selected (Work/Shop).");
            return;
        }

        // Optional: validate transport availability using your owned items flags
        if (!IsTransportAvailable(transportMode))
        {
            Debug.LogWarning($"GameManager.ConfirmTravel(): Transport not available: {transportMode}");
            return;
        }

        Location targetLocation = pendingDestination == Destination.Work ? Location.Work : Location.Shop;

        ClearPendingDestination();
        SetLocation(targetLocation);
    }
    private bool IsTransportAvailable(TransportMode mode)
    {
        switch (mode)
        {
            case TransportMode.Walk:
                return true;
            case TransportMode.PublicTrans:
                return false;
            case TransportMode.Taxi:
                return false;
            case TransportMode.OldBike:
                return oldBike;
            case TransportMode.NewBike:
                return newBike;
            case TransportMode.OldCar:
                return oldCar;
            case TransportMode.NewCar:
                return newCar;
            default:
                return false;
        }
    }
    
    private void SetLocation(Location newLocation)
    {
        if (currentLocation == newLocation) return;

        currentLocation = newLocation;
        OnLocationChanged?.Invoke(currentLocation);
    }
 
    
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

