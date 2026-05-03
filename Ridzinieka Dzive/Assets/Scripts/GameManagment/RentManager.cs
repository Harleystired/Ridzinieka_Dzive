using System;
using UnityEngine;

public class RentManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Header("Rent Settings")]
    [SerializeField] private int weeklyRentAmount = 100;

    [Tooltip("Calendar day indexes when rent is due. 0 = Day 1, 6 = Day 7.")]
    [SerializeField] private int[] rentDayIndexes = { 6, 13, 20, 27 };

    [Header("Debt")]
    [SerializeField] private int unpaidRentDebt;

    public int WeeklyRentAmount => weeklyRentAmount;
    public int UnpaidRentDebt => unpaidRentDebt;

    public event Action<int, int> OnRentPaid;
    public event Action OnRentStateChanged;

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
            gameManager.OnDayChanged += HandleDayChanged;
    }

    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.OnDayChanged -= HandleDayChanged;
    }

    private void HandleDayChanged(int newDayIndex)
    {
        if (!IsRentDay(newDayIndex))
            return;

        PayRent();
    }

    public bool IsRentDay(int dayIndex)
    {
        if (rentDayIndexes == null)
            return false;

        for (int i = 0; i < rentDayIndexes.Length; i++)
        {
            if (rentDayIndexes[i] == dayIndex)
                return true;
        }

        return false;
    }

    public int GetTotalRentDue()
    {
        return weeklyRentAmount + unpaidRentDebt;
    }

    private void PayRent()
    {
        if (gameManager == null)
            return;

        int totalDue = GetTotalRentDue();

        if (totalDue <= 0)
            return;

        int paidAmount = Mathf.Min(gameManager.money, totalDue);

        if (paidAmount > 0)
            gameManager.SpendMoney(paidAmount);

        unpaidRentDebt = totalDue - paidAmount;

        OnRentPaid?.Invoke(paidAmount, unpaidRentDebt);
        OnRentStateChanged?.Invoke();

        if (unpaidRentDebt > 0)
        {
            Debug.Log($"Rent day. Paid {paidAmount}. Unpaid rent debt is now {unpaidRentDebt}.");
        }
        else
        {
            Debug.Log($"Rent day. Paid full rent: {paidAmount}.");
        }
    }
}
