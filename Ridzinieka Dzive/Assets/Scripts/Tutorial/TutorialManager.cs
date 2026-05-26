using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private GameObject skipButton;   

    [Header("Steps")]
    [SerializeField] private TutorialStep[] steps;

    private int currentIndex = 0;
    private int highestStepReached = -1;
    private bool tutorialActive = true;

    public int CurrentStep => currentIndex;

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

        // 0. solis vienmēr atļauts
        if (index == 0)
        {
            currentIndex = 0;
            highestStepReached = 0;

            tooltipPanel.SetActive(true);
            tooltipPanel.transform.position = steps[0].position;
            tooltipText.text = steps[0].text;

            // Skip poga redzama jau no 0. soļa
            if (skipButton != null)
                skipButton.SetActive(true);

            return;
        }

        // Neļauj izlaist soļus
        if (index != currentIndex + 1)
            return;

        // Neļauj atkārtot vai iet atpakaļ
        if (index <= highestStepReached)
            return;

        currentIndex = index;
        highestStepReached = index;

        tooltipPanel.SetActive(true);
        tooltipPanel.transform.position = steps[index].position;
        tooltipText.text = steps[index].text;
        FadeInTooltip();

        // Skip poga vienmēr redzama, kamēr tutorials aktīvs
        if (skipButton != null)
            skipButton.SetActive(true);
    }

    private void FadeInTooltip()
    {
        StopAllCoroutines();
        StartCoroutine(FadeInScale());
    }

    private IEnumerator FadeInScale()
    {
        tooltipPanel.transform.localScale = Vector3.zero;

        float t = 0f;
        float duration = 0.15f; // ← maini šo, lai pagarinātu animāciju

        while (t < duration)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(0f, 1f, t / duration);
            tooltipPanel.transform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        tooltipPanel.transform.localScale = Vector3.one;
    }
    // -----------------------------
    // TRIGGER (ja izmanto ID)
    // -----------------------------
    public void Trigger(string id)
    {
        if (!tutorialActive) return;

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

        if (skipButton != null)
            skipButton.SetActive(false);
    }
    
    // -----------------------------
    // COMPLETE
    // -----------------------------
    public void CompleteTutorial()
    {
        tutorialActive = false;
        tooltipPanel.SetActive(false);

        if (skipButton != null)
            skipButton.SetActive(false);
    }
}
