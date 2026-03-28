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

        ClearPendingDestination();
        SetLocation(targetLocation);
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
