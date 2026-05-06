using UnityEngine;

public class CashierWorkComputer : BaseWorkComputer
{
    [Header("Cashier Specific")]
    [SerializeField] private GameObject cashierPanel;
    
    protected override void Awake()
    {
        base.Awake();
        jobType = GameManager.JobType.Cashier;
        
        if (computerUI == null && cashierPanel != null)
            computerUI = cashierPanel;
    }
}
