using UnityEngine;

public class Bed : MonoBehaviour, IClickable2D
{
    [SerializeField] GameObject bed; //assigns the UI element
    [SerializeField] private GameManager gameManager;
    
    private void Awake() //hides the bed
    {
        if (bed != null)
            bed.SetActive(false);
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

    public void Sleep() //advances the day
    {
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null) return;

        gameManager.AdvanceDay();
        gameManager.SetTimeOfDay(GameManager.TimeOfDay.Morning);
        
        CloseBed();
    }

    public void Nap() // advances time of day
    {
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null) return;

        var before = gameManager.CurrentTime;

        gameManager.AdvanceTimeOfDay();

        if (before == GameManager.TimeOfDay.Night && gameManager.CurrentTime == GameManager.TimeOfDay.Morning)
            gameManager.AdvanceDay();
    }

    public void CloseBed() //closes the bed
    {
        if (bed == null) return;
        if (!bed.activeSelf) return;

        bed.SetActive(false);

        UIModal.Close(); // allows other objects to be clicked, if this is not done, NOTHING will be clickable
    }
}
