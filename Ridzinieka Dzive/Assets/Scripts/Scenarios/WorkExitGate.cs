using System.Collections;
using TMPro;
using UnityEngine;

public class WorkExitGate : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WorkUI workUI;                 // drag the same WorkUI that the button currently uses
    [SerializeField] private ScenarioManager scenarioManager;

    [Header("Work Tooltip UI (different location than Home tooltip)")]
    [SerializeField] private GameObject tooltipRoot;        // panel GameObject at WORK tooltip position
    [SerializeField] private TextMeshProUGUI tooltipText;   // TMP text inside that panel
    [SerializeField] private float autoHideSeconds = 2f;

    private Coroutine _hideRoutine;

    private void Awake()
    {
        if (workUI == null) workUI = FindFirstObjectByType<WorkUI>();
        if (scenarioManager == null) scenarioManager = FindFirstObjectByType<ScenarioManager>();

        HideTooltip();
    }

    public void TryReturnHome()
    {
        if (scenarioManager != null && scenarioManager.IsWorkBlocked(out var reason))
        {
            ShowTooltip(reason);
            return;
        }

        if (workUI != null)
            workUI.ReturnHome();
    }

    private void ShowTooltip(string message)
    {
        if (tooltipRoot == null) return;

        tooltipRoot.SetActive(true);

        if (tooltipText != null)
            tooltipText.text = message;

        if (_hideRoutine != null)
            StopCoroutine(_hideRoutine);

        if (autoHideSeconds > 0f)
            _hideRoutine = StartCoroutine(HideAfterDelay(autoHideSeconds));
    }

    private IEnumerator HideAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        HideTooltip();
    }

    private void HideTooltip()
    {
        if (tooltipRoot != null)
            tooltipRoot.SetActive(false);
    }
}
