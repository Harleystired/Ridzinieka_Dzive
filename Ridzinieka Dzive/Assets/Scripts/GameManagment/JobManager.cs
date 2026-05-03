using System;
using UnityEngine;

public class JobManager : MonoBehaviour
{
    [Serializable]
    public class JobSchedule
    {
        public GameManager.JobType jobType;

        [Tooltip("Calendar day indexes where this job works. 0 = Day 1, 1 = Day 2, etc.")]
        public int[] workDayIndexes;

        [Header("Pay")]
        [Min(0)] public int minDailyPay = 25;
        [Min(0)] public int maxDailyPay = 25;
    }

    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Header("Job Schedules")]
    [SerializeField] private JobSchedule[] jobSchedules;

    [Header("Work State")]
    [SerializeField] private bool hasWorkedToday;

    [SerializeField] private bool workBlockedTodayByScenario;

    [Header("Missed Work Punishment")]
    [Tooltip("Pay multiplier used on the next worked day after missing a work day. 0.8 = 20% less pay.")]
    [Range(0f, 1f)]
    [SerializeField] private float missedWorkPayMultiplier = 0.8f;

    [SerializeField] private bool missedPreviousWorkDayPenaltyActive;

    [Header("Pending Pay")]
    [SerializeField] private int pendingPay;

    public bool HasWorkedToday => hasWorkedToday;
    public int PendingPay => pendingPay;
    public bool MissedPreviousWorkDayPenaltyActive => missedPreviousWorkDayPenaltyActive;
    public bool WorkBlockedTodayByScenario => workBlockedTodayByScenario;

    public event Action OnJobWorkStateChanged;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnEnable()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager != null)
        {
            gameManager.OnDayChanged += HandleDayChanged;
            gameManager.OnSelectedJobChanged += HandleSelectedJobChanged;
        }
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnDayChanged -= HandleDayChanged;
            gameManager.OnSelectedJobChanged -= HandleSelectedJobChanged;
        }
    }

    private void HandleDayChanged(int newDayIndex)
    {
        int previousDayIndex = newDayIndex - 1;

        if (previousDayIndex >= 0 && IsWorkDay(previousDayIndex) && !hasWorkedToday)
        {
            missedPreviousWorkDayPenaltyActive = true;
            Debug.Log($"Missed work on day index {previousDayIndex}. Next work pay will be reduced.");
        }

        hasWorkedToday = false;
        workBlockedTodayByScenario = false;

        OnJobWorkStateChanged?.Invoke();
    }

    private void HandleSelectedJobChanged(GameManager.JobType newJob)
    {
        hasWorkedToday = false;
        workBlockedTodayByScenario = false;
        missedPreviousWorkDayPenaltyActive = false;
        OnJobWorkStateChanged?.Invoke();
    }

    public void BlockWorkTodayFromScenario()
    {
        workBlockedTodayByScenario = true;
        OnJobWorkStateChanged?.Invoke();

        Debug.Log("Work has been blocked for today by a scenario choice.");
    }

    public bool IsWorkBlockedToday()
    {
        return workBlockedTodayByScenario;
    }

    public bool IsWorkDay(int dayIndex)
    {
        if (gameManager == null)
            return false;

        return IsWorkDay(gameManager.SelectedJob, dayIndex);
    }

    public bool IsWorkDay(GameManager.JobType jobType, int dayIndex)
    {
        JobSchedule schedule = GetSchedule(jobType);

        if (schedule == null || schedule.workDayIndexes == null)
            return false;

        for (int i = 0; i < schedule.workDayIndexes.Length; i++)
        {
            if (schedule.workDayIndexes[i] == dayIndex)
                return true;
        }

        return false;
    }

    public bool CanWorkToday()
    {
        if (gameManager == null)
            return false;

        if (workBlockedTodayByScenario)
            return false;

        if (hasWorkedToday)
            return false;

        return IsWorkDay(gameManager.CurrentDayIndex);
    }

    public bool TryWorkToday()
    {
        if (!CanWorkToday())
        {
            Debug.Log($"Cannot work today. Day: {gameManager.CurrentDayIndex}, Job: {gameManager.SelectedJob}, HasWorkedToday: {hasWorkedToday}");
            return false;
        }

        JobSchedule schedule = GetCurrentSchedule();

        if (schedule == null)
        {
            Debug.LogWarning($"No job schedule found for {gameManager.SelectedJob}.");
            return false;
        }

        int earnedPay = GetRandomPay(schedule);
        int originalPay = earnedPay;

        if (missedPreviousWorkDayPenaltyActive)
        {
            earnedPay = Mathf.RoundToInt(earnedPay * missedWorkPayMultiplier);
            missedPreviousWorkDayPenaltyActive = false;

            Debug.Log($"Missed work penalty applied. Pay reduced from {originalPay} to {earnedPay}.");
        }

        pendingPay += earnedPay;
        hasWorkedToday = true;

        OnJobWorkStateChanged?.Invoke();

        Debug.Log($"Worked today as {gameManager.SelectedJob}. Earned {earnedPay}. Pending pay is now {pendingPay}.");

        return true;
    }

    public int ClaimPendingPay()
    {
        if (gameManager == null)
            return 0;

        if (pendingPay <= 0)
            return 0;

        int paidAmount = pendingPay;
        pendingPay = 0;

        gameManager.AddMoney(paidAmount);

        OnJobWorkStateChanged?.Invoke();

        Debug.Log($"Received pending work pay: {paidAmount}.");

        return paidAmount;
    }

    public int GetCurrentDailyPay()
    {
        JobSchedule schedule = GetCurrentSchedule();

        if (schedule == null)
            return 0;

        if (schedule.minDailyPay == schedule.maxDailyPay)
            return schedule.minDailyPay;

        return GetRandomPay(schedule);
    }

    private int GetRandomPay(JobSchedule schedule)
    {
        int minPay = Mathf.Min(schedule.minDailyPay, schedule.maxDailyPay);
        int maxPay = Mathf.Max(schedule.minDailyPay, schedule.maxDailyPay);

        return UnityEngine.Random.Range(minPay, maxPay + 1);
    }

    private JobSchedule GetCurrentSchedule()
    {
        if (gameManager == null)
            return null;

        return GetSchedule(gameManager.SelectedJob);
    }

    private JobSchedule GetSchedule(GameManager.JobType jobType)
    {
        if (jobSchedules == null)
            return null;

        for (int i = 0; i < jobSchedules.Length; i++)
        {
            if (jobSchedules[i] != null && jobSchedules[i].jobType == jobType)
                return jobSchedules[i];
        }

        return null;
    }
}
