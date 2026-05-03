using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class StatChangeTooltipUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text tooltipText;

    [Header("Timing")]
    [SerializeField] private float visibleSeconds = 2.5f;

    private Coroutine _hideRoutine;

    private void Awake()
    {
        HideInstant();
    }

    public void ShowMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (root == null || tooltipText == null)
            return;

        tooltipText.text = message;
        root.SetActive(true);

        if (_hideRoutine != null)
            StopCoroutine(_hideRoutine);

        _hideRoutine = StartCoroutine(HideAfterDelay());
    }

    public void ShowChanges(IReadOnlyList<ScenarioDefinition.StatDelta> changes)
    {
        if (changes == null || changes.Count == 0)
            return;

        if (root == null || tooltipText == null)
            return;

        string message = BuildMessage(changes);

        if (string.IsNullOrWhiteSpace(message))
            return;

        tooltipText.text = message;
        root.SetActive(true);

        if (_hideRoutine != null)
            StopCoroutine(_hideRoutine);

        _hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private string BuildMessage(IReadOnlyList<ScenarioDefinition.StatDelta> changes)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < changes.Count; i++)
        {
            var change = changes[i];

            if (change.amount == 0)
                continue;

            string color = change.amount > 0 ? "#37D667" : "#E64B4B";
            string sign = change.amount > 0 ? "+" : "";

            sb.Append("<color=");
            sb.Append(color);
            sb.Append(">");
            sb.Append(sign);
            sb.Append(change.amount);
            sb.Append(" ");
            sb.Append(GetDisplayName(change.stat));
            sb.Append("</color>");

            if (i < changes.Count - 1)
                sb.AppendLine();
        }

        return sb.ToString();
    }

    private string GetDisplayName(StatType stat)
    {
        switch (stat)
        {
            case StatType.Money:
                return "nauda";

            case StatType.Hunger:
                return "bads";

            case StatType.Energy:
                return "enerģija";

            case StatType.Stress:
                return "stress";

            case StatType.Health:
                return "veselība";

            default:
                return stat.ToString().ToLower();
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(visibleSeconds);

        HideInstant();
        _hideRoutine = null;
    }

    private void HideInstant()
    {
        if (root != null)
            root.SetActive(false);
    }
}
