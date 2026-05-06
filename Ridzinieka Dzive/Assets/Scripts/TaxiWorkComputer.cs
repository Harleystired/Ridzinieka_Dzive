using UnityEngine;

public class TaxiWorkComputer : BaseWorkComputer
{
    [Header("Taxi Specific")]
    [SerializeField] private GameObject taxiPanel;
    
    protected override void Awake()
    {
        base.Awake();
        jobType = GameManager.JobType.Taxi;
        
        if (computerUI == null && taxiPanel != null)
            computerUI = taxiPanel;
    }
}
