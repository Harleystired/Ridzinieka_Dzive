using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverReasonText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    
    [Header("Game Over Settings")]
    [SerializeField] private int daysToTriggerGameOver = 5;
    [SerializeField] private int criticalThreshold = 5;
    
    [Header("Debt Game Over Settings")]
    [SerializeField] private int maxAllowedDebt = 300;
    [SerializeField] private int checkAfterRentDayIndex = 27; // 4th rent day (day 28 in 0-index)
    
    [Header("Stat Tracking")]
    [SerializeField] private int consecutiveHungerDays = 0;
    [SerializeField] private int consecutiveEnergyDays = 0;
    [SerializeField] private int consecutiveStressDays = 0;
    [SerializeField] private int consecutiveHealthDays = 0;
    
    [Header("Debt Tracking")]
    [SerializeField] private bool hasDebtGameOverTriggered = false;
    
    private GameManager gameManager;
    private RentManager rentManager;
    private bool isGameOver = false;
    
    // Track the last day we processed to avoid double-counting
    private int lastProcessedDay = -1;
    
    private enum GameOverReason
    {
        HighHunger,
        LowEnergy,
        HighStress,
        LowHealth,
        Debt
    }
    
    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        rentManager = FindFirstObjectByType<RentManager>();
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(LoadMainMenu);
        
        // Subscribe to day change events
        if (gameManager != null)
            gameManager.OnDayChanged += OnDayChanged;
        
        // Subscribe to rent state changes to check debt
        if (rentManager != null)
            rentManager.OnRentStateChanged += CheckDebtForGameOver;
    }
    
    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.OnDayChanged -= OnDayChanged;
        
        if (rentManager != null)
            rentManager.OnRentStateChanged -= CheckDebtForGameOver;
    }
    
    private void OnDayChanged(int newDayIndex)
    {
        if (isGameOver) return;
        
        // Check if we've already processed this day
        if (lastProcessedDay == newDayIndex) return;
        lastProcessedDay = newDayIndex;
        
        // Check each stat and update consecutive counters
        CheckStatAndUpdateCounter(gameManager.hunger, ref consecutiveHungerDays, 100 - criticalThreshold, 100, "starvation");
        CheckStatAndUpdateCounter(gameManager.energy, ref consecutiveEnergyDays, 0, criticalThreshold, "exhaustion");
        CheckStatAndUpdateCounter(gameManager.stress, ref consecutiveStressDays, 100 - criticalThreshold, 100, "mental breakdown");
        CheckStatAndUpdateCounter(gameManager.health, ref consecutiveHealthDays, 0, criticalThreshold, "severe illness");
        
        // Check if any stat has reached the game over threshold
        CheckForGameOver();
    }
    
    private void CheckStatAndUpdateCounter(int statValue, ref int consecutiveDays, int lowThresholdMin, int lowThresholdMax, string statName)
    {
        bool isInCriticalRange = statValue >= lowThresholdMin && statValue <= lowThresholdMax;
        
        // Special handling for stats that are critical at low values (energy, health)
        // vs stats that are critical at high values (hunger, stress)
        bool isCritical;
        
        if (lowThresholdMin == 0)
        {
            // Critical at low values (0-5 for energy and health)
            isCritical = statValue <= lowThresholdMax;
        }
        else
        {
            // Critical at high values (95-100 for hunger and stress)
            isCritical = statValue >= lowThresholdMin;
        }
        
        if (isCritical)
        {
            consecutiveDays++;
            Debug.Log($"{statName} is critical for {consecutiveDays} consecutive days. (Value: {statValue})");
        }
        else
        {
            consecutiveDays = 0;
        }
    }
    
    private void CheckDebtForGameOver()
    {
        if (isGameOver) return;
        if (hasDebtGameOverTriggered) return;
        if (rentManager == null) return;
        if (gameManager == null) return;
        
        // Only check for debt game over after the 4th rent day (day index 27)
        if (gameManager.CurrentDayIndex < checkAfterRentDayIndex) return;
        
        int currentDebt = rentManager.UnpaidRentDebt;
        
        if (currentDebt >= maxAllowedDebt)
        {
            hasDebtGameOverTriggered = true;
            TriggerGameOver(GameOverReason.Debt);
        }
    }
    
    private void CheckForGameOver()
    {
        if (consecutiveHungerDays >= daysToTriggerGameOver)
        {
            TriggerGameOver(GameOverReason. HighHunger);
        }
        else if (consecutiveEnergyDays >= daysToTriggerGameOver)
        {
            TriggerGameOver(GameOverReason.LowEnergy);
        }
        else if (consecutiveStressDays >= daysToTriggerGameOver)
        {
            TriggerGameOver(GameOverReason. HighStress);
        }
        else if (consecutiveHealthDays >= daysToTriggerGameOver)
        {
            TriggerGameOver(GameOverReason.LowHealth);
        }
    }
    
    private void TriggerGameOver(GameOverReason reason)
    {
        if (isGameOver) return;
        
        isGameOver = true;
        
        string reasonText = "";
        
        switch (reason)
        {
            case GameOverReason. HighHunger:
                reasonText = "Bada sāpes apgrūtina tavu dzīves rutīnu.\nTev vairs nav spēka.\nTev kļuva tik slikti, ka nācās izsaukt ātro palīdzību.\nTu esi dzīvs, bet slimnīcā nāksies pavadīt kādu laiku .";
                break;
            case GameOverReason.LowEnergy:
                reasonText = "Tev vairs nav spēka, esi pilnīgi izdedzis.\nNe spēks, ne vēlme turpināt strādāt.\nLaikam Rīgas dzīve nav domāta tev.\nUz kādu laiku jāpadzīvo pie vecākiem.";
                break;
            case GameOverReason. HighStress:
                reasonText = "Tavā dzīvē viss ir daudz pa daudz.\nTavi nervi vairs nevar to izturēt.\nLaikam Rīgas dzīve nav domāta tev.\nUz kādu laiku jāpadzīvo pie vecākiem";
                break;
            case GameOverReason.LowHealth:
                reasonText = "Dzīves apstākļu dēļ, tava veselība kļuva tik slikta, ka nācās izsaukt ātro palīdzību.\nTu esi dzīvs, bet slimnīcā nāksies pavadīt kādu laiku.";
                break;
            case GameOverReason.Debt:
                int debtAmount = rentManager != null ? rentManager.UnpaidRentDebt : 0;
                reasonText = $"Parāds ir uzkrājies līdz {debtAmount}€.\nIzmaksas ir par lielu, lai to segtu.\nTev nav citas izvēles, kā atgriezties dzīvot pie vecākiem.\nRīgas dzīve tev izrādījās par dārgu.";
                break;
        }
        
        if (gameOverReasonText != null)
            gameOverReasonText.text = reasonText;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        
        // Optional: Pause the game
        Time.timeScale = 0f;
        
        Debug.Log($"Game Over triggered! Reason: {reason}");
    }
    
    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        // Replace "MainMenu" with your actual main menu scene name
        SceneManager.LoadScene("MAINMENU");
    }
    
    // Public method to manually check game over (for testing or immediate checks)
    public void CheckGameOverImmediate()
    {
        if (isGameOver) return;
        
        // Reset counters for immediate check (optional)
        consecutiveHungerDays = 0;
        consecutiveEnergyDays = 0;
        consecutiveStressDays = 0;
        consecutiveHealthDays = 0;
        
        // Re-check all stats
        OnDayChanged(gameManager.CurrentDayIndex);
        
        // Also check debt
        CheckDebtForGameOver();
    }
}
