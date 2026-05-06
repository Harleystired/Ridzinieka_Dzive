using UnityEngine;

public class TaxiWorkComputer : BaseWorkComputer
{
    [Header("Taxi Specific")]
    [SerializeField] private GameObject taxiPanel; // Taxi-specific UI panel
    
    protected override void Awake()
    {
        base.Awake();
        jobType = GameManager.JobType.Taxi;
    }
}
