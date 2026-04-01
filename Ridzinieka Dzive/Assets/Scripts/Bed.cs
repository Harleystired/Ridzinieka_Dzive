using UnityEngine;

public class Bed : MonoBehaviour, IClickable2D
{
    [SerializeField] GameObject bed; //assigns the UI element
    [SerializeField] private GameManager gameManager;
    
    [Header("Scenario Blocking")]
    [SerializeField] private ScenarioManager scenarioManager;
    [SerializeField] private SimpleTooltip tooltip;
    
    private void Awake() //hides the bed
    {
        if (bed != null)
            bed.SetActive(false);

        if (scenarioManager == null)
            scenarioManager = FindFirstObjectByType<ScenarioManager>();
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

        gameManager.AddHunger(35);

        // Stress vienmēr uz 0
        gameManager.stress = 0;

        // Pāriet uz nākamo dienu
        gameManager.AdvanceDay();

        // No rīta
        gameManager.SetTimeOfDay(GameManager.TimeOfDay.Morning);

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

        var before = gameManager.CurrentTime;

        gameManager.AdvanceTimeOfDay();

        if (before == GameManager.TimeOfDay.Night && gameManager.CurrentTime == GameManager.TimeOfDay.Morning)
            gameManager.AdvanceDay();

        gameManager.AddHunger(20);
        gameManager.AddStress(20);

        var statsUI = FindFirstObjectByType<StatsUI>();
        if (statsUI != null)
            statsUI.UpdateStats();
    }

    public void CloseBed() //closes the bed
    {
        if (bed == null) return;
        if (!bed.activeSelf) return;

        bed.SetActive(false);

        UIModal.Close(); // allows other objects to be clicked
    }
}
