using UnityEngine;

public class OfficeWorkComputer : BaseWorkComputer
{
    [Header("Office Specific")]
    [SerializeField] private GameObject officePanel; // Office-specific UI panel
    
    protected override void Awake()
    {
        base.Awake();
        jobType = GameManager.JobType.Office;
    }
}
