using System.Collections.Generic;
using UnityEngine;

public class Bed : MonoBehaviour, IClickable2D
{
    [SerializeField] GameObject bed; //assigns the UI element
    [SerializeField] private GameManager gameManager;

    [Header("Job")]
    [SerializeField] private JobManager jobManager;

    [Header("Rent")]
    [SerializeField] private RentManager rentManager;
    private int lastRentPaidAmount;
    private int lastUnpaidRentDebt;

    [Header("Stat Tooltip")]
    [SerializeField] private StatChangeTooltipUI statChangeTooltip;

    [Header("Scenario Blocking")]
    [SerializeField] private ScenarioManager scenarioManager;
    [SerializeField] private SimpleTooltip tooltip;

    private void Awake() //hides the bed
    {
        if (bed != null)
            bed.SetActive(false);

        if (scenarioManager == null)
            scenarioManager = FindFirstObjectByType<ScenarioManager>();

        if (jobManager == null)
            jobManager = FindFirstObjectByType<JobManager>();

        if (rentManager == null)
            rentManager = FindFirstObjectByType<RentManager>();

        if (statChangeTooltip == null)
            statChangeTooltip = FindFirstObjectByType<StatChangeTooltipUI>();
    }

    private void OnEnable()
    {
        if (rentManager == null)
            rentManager = FindFirstObjectByType<RentManager>();

        if (rentManager != null)
            rentManager.OnRentPaid += HandleRentPaid;
    }

    private void OnDisable()
    {
        if (rentManager != null)
            rentManager.OnRentPaid -= HandleRentPaid;
    }

    private void HandleRentPaid(int paidAmount, int unpaidDebt)
    {
        lastRentPaidAmount = paidAmount;
        lastUnpaidRentDebt = unpaidDebt;
    }

    private void Start()
    {
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
    }

    public void OnClicked(RaycastHit2D hit) //opens the bed upon clicking
    {
        if (bed == null) return;

        bool newState = !bed.activeSelf;
        bed.SetActive(newState);

        if (newState) UIModal.Open(); // makes it so other object can't be clicked through UI
        else UIModal.Close();
    }

    public void Sleep()
    {
        if (scenarioManager != null && scenarioManager.IsHomeBlocked(out string reason))
        {
            if (tooltip != null) tooltip.Show(reason);
            else Debug.Log(reason);
            return;
        }

        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null) return;

        List<ScenarioDefinition.StatDelta> changes = new List<ScenarioDefinition.StatDelta>();

        int hungerBefore = gameManager.hunger;
        int stressBefore = gameManager.stress;

        gameManager.AddHunger(35);

        int hungerChange = gameManager.hunger - hungerBefore;
        if (hungerChange != 0)
        {
            changes.Add(new ScenarioDefinition.StatDelta
            {
                stat = StatType.Hunger,
                amount = hungerChange
            });
        }

        // Stress vienmēr -7
        gameManager.RemoveStress(7);

        int stressChange = gameManager.stress - stressBefore;
        if (stressChange != 0)
        {
            changes.Add(new ScenarioDefinition.StatDelta
            {
                stat = StatType.Stress,
                amount = stressChange
            });
        }
        gameManager.AddEnergy(25);

        lastRentPaidAmount = 0;
        lastUnpaidRentDebt = 0;

        gameManager.MarkFreshWakeUpMorningScenarioWindow();

        // Pāriet uz nākamo dienu
        gameManager.AdvanceDay();

        // No rīta
        gameManager.SetTimeOfDay(GameManager.TimeOfDay.Morning);

        int paidAmount = PayPendingWorkMoney();
        if (paidAmount > 0)
        {
            changes.Add(new ScenarioDefinition.StatDelta
            {
                stat = StatType.Money,
                amount = paidAmount
            });
        }

        if (lastRentPaidAmount > 0)
        {
            changes.Add(new ScenarioDefinition.StatDelta
            {
                stat = StatType.Money,
                amount = -lastRentPaidAmount
            });
        }

        if (lastUnpaidRentDebt > 0)
            Debug.Log($"Unpaid rent debt remaining: {lastUnpaidRentDebt}");

        ShowStatChanges(changes);

        // Atjauno UI, ja ir StatsUI
        var statsUI = FindFirstObjectByType<StatsUI>();
        if (statsUI != null)
            statsUI.UpdateStats();

        // Aizver gultas UI
        CloseBed();
    }


    public void Nap() // advances time of day
    {
        if (scenarioManager != null && scenarioManager.IsHomeBlocked(out string reason))
        {
            if (tooltip != null) tooltip.Show(reason);
            else Debug.Log(reason);
            return;
        }

        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null) return;

        List<ScenarioDefinition.StatDelta> changes = new List<ScenarioDefinition.StatDelta>();

        var before = gameManager.CurrentTime;

        int hungerBefore = gameManager.hunger;
        int stressBefore = gameManager.stress;

        if (before == GameManager.TimeOfDay.Night)
            gameManager.MarkFreshWakeUpMorningScenarioWindow();

        gameManager.AdvanceTimeOfDay();

        if (before == GameManager.TimeOfDay.Night && gameManager.CurrentTime == GameManager.TimeOfDay.Morning)
        {
            lastRentPaidAmount = 0;
            lastUnpaidRentDebt = 0;

            gameManager.AdvanceDay();

            int paidAmount = PayPendingWorkMoney();
            if (paidAmount > 0)
            {
                changes.Add(new ScenarioDefinition.StatDelta
                {
                    stat = StatType.Money,
                    amount = paidAmount
                });
            }

            if (lastRentPaidAmount > 0)
            {
                changes.Add(new ScenarioDefinition.StatDelta
                {
                    stat = StatType.Money,
                    amount = -lastRentPaidAmount
                });
            }

            if (lastUnpaidRentDebt > 0)
                Debug.Log($"Unpaid rent debt remaining: {lastUnpaidRentDebt}");
        }

        gameManager.AddHunger(20);
        gameManager.RemoveStress(5);
        gameManager.AddEnergy(20);

        int hungerChange = gameManager.hunger - hungerBefore;
        if (hungerChange != 0)
        {
            changes.Add(new ScenarioDefinition.StatDelta
            {
                stat = StatType.Hunger,
                amount = hungerChange
            });
        }

        int stressChange = gameManager.stress - stressBefore;
        if (stressChange != 0)
        {
            changes.Add(new ScenarioDefinition.StatDelta
            {
                stat = StatType.Stress,
                amount = stressChange
            });
        }

        ShowStatChanges(changes);

        var statsUI = FindFirstObjectByType<StatsUI>();
        if (statsUI != null)
            statsUI.UpdateStats();
    }

    private int PayPendingWorkMoney()
    {
        if (jobManager == null)
            jobManager = FindFirstObjectByType<JobManager>();

        if (jobManager == null)
            return 0;

        return jobManager.ClaimPendingPay();
    }

    private void ShowStatChanges(List<ScenarioDefinition.StatDelta> changes)
    {
        if (changes == null || changes.Count == 0)
            return;

        if (statChangeTooltip == null)
            statChangeTooltip = FindFirstObjectByType<StatChangeTooltipUI>();

        if (statChangeTooltip == null)
            return;

        statChangeTooltip.ShowChanges(changes);
    }

    public void CloseBed() //closes the bed
    {
        if (bed == null) return;
        if (!bed.activeSelf) return;

        bed.SetActive(false);

        UIModal.Close(); // allows other objects to be clicked
    }
}
