using System;
using UnityEngine;
using System.Collections.Generic;


public class GameManager : MonoBehaviour
{
   // ---------------------------------------------------------------------
    // Player Stats / Persistent Data
    // ---------------------------------------------------------------------

    // stores the game data, add any extra data you want to store
    [Header("Player Stats")]
    public int money;
    public int hunger = 100;
    public int energy = 100;
    public int stress = 0;
    public int health = 100;

    [Header("Transport Ownership")]
    public bool oldBike = false;
    public bool newBike = false;
    public bool oldCar = false;
    public bool newCar = false;

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

    [SerializeField] private Destination pendingDestination = Destination.None;

    public Location CurrentLocation => currentLocation;
    public Destination PendingDestination => pendingDestination;

    public event Action<Location> OnLocationChanged;
    public event Action<Destination> OnPendingDestinationChanged;

    public void EnterOutside()
    {
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
        // transportMode currently not used, but kept for future logic
        if (pendingDestination == Destination.None)
        {
            Debug.LogWarning("GameManager.ConfirmTravel(): No pending destination selected.");
            return;
        }

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

        ClearPendingDestination();
        SetLocation(targetLocation);
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
        OnLocationChanged?.Invoke(currentLocation);
    }

    // ---------------------------------------------------------------------
    // Time of Day
    // ---------------------------------------------------------------------

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

    public void AdvanceDay()
    {
        if (calendarDay == null || calendarDay.Length == 0) return;

        int maxIndex = Mathf.Min(30, calendarDay.Length - 1);
        if (currentDayIndex >= maxIndex) return; // already at last day

        currentDayIndex++;
        OnDayChanged?.Invoke(currentDayIndex);
    }

    // ---------------------------------------------------------------------
    // Money / Needs
    // ---------------------------------------------------------------------

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
        hunger += amount;
        if (hunger > 0) 
            hunger = 0;
        
        if (hunger < -100) 
            hunger = -100;
    }

    public void RemoveHunger(int amount)
    {
        hunger -= amount;
        if (hunger < 0) 
            hunger = 0;
    }

    public void AddStress(int amount)
    {
        stress += amount;

        if (stress < 0)
            stress = 0;

        if (stress > 100)
            stress = 100;
    }
    
    public void RemoveStress(int amount)
    {
        stress -= amount;
        if (stress < 0)
            stress = 0;
    }
    
    public void AddHealth(int amount)
    {
        health += amount;
        
        if (health < 0)
            health = 0;
        
        if (health > 100)
            health = 100;
    }
    
    public void RemoveHealth(int amount)
    {
        health -= amount;
        
        if (health < 0)
            health = 0;
    }

    public void AddEnergy(int amount)
    {
        energy += amount;
        if (energy < 0)
            energy = 0;
       
        if (energy > 100)
            energy = 100;
    }
    
    public void RemoveEnergy(int amount)
    {
        energy -= amount;
        if (energy < 0)
            energy = 0;
    }
}

