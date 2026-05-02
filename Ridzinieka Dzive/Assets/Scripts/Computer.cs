using UnityEngine;

public class Computer : MonoBehaviour, IClickable2D
{
    [SerializeField] GameObject computer; //assigns the UI element
    [SerializeField] private GameObject TimeOfDayUI;
    
    [Header("Scenarios")]
    [SerializeField] private ScenarioManager scenarioManager;
    private void Awake() //hides the computer
    {
        if (computer != null)
            computer.SetActive(false);

        if (scenarioManager == null)
            scenarioManager = FindFirstObjectByType<ScenarioManager>();
    }
    
    public void OnClicked(RaycastHit2D hit) //opens the computer upon clicking
    {
        if (computer == null) return;

        bool newState = !computer.activeSelf;

        if (!newState && scenarioManager != null && scenarioManager.IsScenarioActive)
            return;

        computer.SetActive(newState);

        if (TimeOfDayUI != null)
            TimeOfDayUI.SetActive(!newState);

        if (newState)
        {
            UIModal.Open(); // makes it so other object can't be clicked through UI
            if (scenarioManager != null) scenarioManager.NotifyComputerOpened();
        }
        else
        {
            UIModal.Close();
            if (scenarioManager != null) scenarioManager.NotifyComputerClosed();
        }
    }

    public void CloseComputer() //closes the computer
    {
        if (computer == null) return;
        if (!computer.activeSelf) return;

        if (scenarioManager != null && scenarioManager.IsScenarioActive)
            return;

        computer.SetActive(false);
        if (TimeOfDayUI != null)
            TimeOfDayUI.SetActive(true);

        UIModal.Close(); // allows other objects to be clicked

        if (scenarioManager != null) scenarioManager.NotifyComputerClosed();
    }
}
