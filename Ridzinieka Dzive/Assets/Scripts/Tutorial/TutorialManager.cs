using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;

    [Header("Steps")]
    [SerializeField] private TutorialStep[] steps;

    private int currentIndex = 0;
    private bool tutorialActive = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
       
    }

    public void ShowStep(int index)
    {
        if (!tutorialActive) return;

        currentIndex = index;

        tooltipPanel.SetActive(true);
        tooltipPanel.transform.position = steps[index].position;
        tooltipText.text = steps[index].text;
    }

    public void Trigger(string id)
    {
        if (!tutorialActive) return;

        if (steps[currentIndex].id == id)
            NextStep();
    }

    private void NextStep()
    {
        currentIndex++;

        if (currentIndex >= steps.Length)
        {
            CompleteTutorial();
            return;
        }

        ShowStep(currentIndex);
    }

    public void SkipTutorial()
    {
        tutorialActive = false;
        tooltipPanel.SetActive(false);
    }

    private void CompleteTutorial()
    {
        tutorialActive = false;
        tooltipPanel.SetActive(false);
    }
}