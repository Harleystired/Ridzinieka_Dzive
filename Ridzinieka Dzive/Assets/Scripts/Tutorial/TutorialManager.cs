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
    private int highestStepReached = -1;   //  Neļauj atkārtot vai iet atpakaļ
    public int CurrentStep => currentIndex;

    
    private bool tutorialActive = true;
    

    private void Awake()
    {
        Instance = this;
    }

    // -----------------------------
    // SHOW STEP
    // -----------------------------
    public void ShowStep(int index)
    {
        if (!tutorialActive) return;

        //  Ļauj parādīt 0. soli vienmēr
        if (index == 0)
        {
            currentIndex = 0;
            highestStepReached = 0;

            tooltipPanel.SetActive(true);
            tooltipPanel.transform.position = steps[0].position;
            tooltipText.text = steps[0].text;
            return;
        }

        //  Neļauj izlaist soļus
        if (index != currentIndex + 1)
            return;

        //  Neļauj atkārtot vai iet atpakaļ
        if (index <= highestStepReached)
            return;

        currentIndex = index;
        highestStepReached = index;

        tooltipPanel.SetActive(true);
        tooltipPanel.transform.position = steps[index].position;
        tooltipText.text = steps[index].text;
    }


    // -----------------------------
    // TRIGGER (ja izmanto ID)
    // -----------------------------
    public void Trigger(string id)
    {
        if (!tutorialActive) return;

        // Tikai tad, ja ID sakrīt ar pašreizējo soli
        if (steps[currentIndex].id == id)
            NextStep();
    }

    // -----------------------------
    // NEXT STEP
    // -----------------------------
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

    // -----------------------------
    // SKIP
    // -----------------------------
    public void SkipTutorial()
    {
        tutorialActive = false;
        tooltipPanel.SetActive(false);
    }

    // -----------------------------
    // COMPLETE
    // -----------------------------
    
    public void CompleteTutorial()
    {
        tutorialActive = false;
        tooltipPanel.SetActive(false);
    }
    
}
