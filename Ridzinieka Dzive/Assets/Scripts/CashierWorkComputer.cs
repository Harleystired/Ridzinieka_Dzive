using UnityEngine;

public class CashierWorkComputer : BaseWorkComputer
{
    [Header("Cashier Specific")]
    [SerializeField] private GameObject cashierPanel; // Cashier-specific UI panel
    
    protected override void Awake()
    {
        base.Awake();
        jobType = GameManager.JobType.Cashier;
    }
}
