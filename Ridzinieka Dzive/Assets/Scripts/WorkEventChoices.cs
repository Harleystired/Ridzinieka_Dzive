using UnityEngine;

public class WorkEventChoices : MonoBehaviour
{
    private GameManager gm;

    private void Start()
    {
        gm = FindFirstObjectByType<GameManager>();
    }

    public void Choice1_NoDuties()
    {
        gm.AddStress(-10);
        Close();
    }

    public void Choice2_Tomorrow()
    {
        gm.AddStress(-10);
        gm.energy -= 10;
        Close();
    }

    public void Choice3_StayLate()
    {
        gm.energy -= 15;
        gm.AddMoney(20);
        Close();
    }

    private void Close()
    {
        gameObject.SetActive(false);

        var statsUI = FindFirstObjectByType<StatsUI>();
        if (statsUI != null)
            statsUI.UpdateStats();
    }
}