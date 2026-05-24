using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameEndManager : MonoBehaviour
{
    [Header("Game End UI")]
    [SerializeField] private GameObject gameEndPanel;
    [SerializeField] private TMP_Text gameEndMessageText;
    [SerializeField] private TMP_Text gameEndStatsText;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button playAgainButton;
    
    [Header("Game End Settings")]
    [SerializeField] private int totalDaysToComplete = 31;
    
    private GameManager gameManager;
    private bool isGameEnded = false;
    
    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        
        if (gameEndPanel != null)
            gameEndPanel.SetActive(false);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(LoadMainMenu);
        
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(RestartGame);
        
        // Subscribe to day change events
        if (gameManager != null)
            gameManager.OnGameCompleted += TriggerGameEnd;
    }
    
    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.OnGameCompleted -= TriggerGameEnd;
    }
    
    private void TriggerGameEnd()
    {
        if (isGameEnded) return;
        
        isGameEnded = true;
        
        // Create the congratulatory message
        string message = GenerateCongratulatoryMessage();
        
        if (gameEndMessageText != null)
            gameEndMessageText.text = message;
        
        
        // Show the game end panel
        if (gameEndPanel != null)
            gameEndPanel.SetActive(true);
        
        // Pause the game
        Time.timeScale = 0f;
        
        Debug.Log("Game completed successfully! Player reached day 31.");
    }
    
    private string GenerateCongratulatoryMessage()
    {
        string message = "Apsveicam!\n\n";
        message += "Jūs izdzīvojāt mēnesi Rīgā kā tās iedzīvotājs!\n";
        message += "Tagad jūs varat oficiāli sevisaukt par Rīdzinieku!\n\n";
        
        return message;
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
    
    // Public method to manually trigger game end (for testing)
    public void ForceGameEnd()
    {
        if (!isGameEnded)
            TriggerGameEnd();
    }
}
