using System;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [Header("Panels (assign in Inspector)")]
    [SerializeField] private MonoBehaviour computerPanelBehaviour; // must implement IScenarioPanel
    [SerializeField] private MonoBehaviour phonePanelBehaviour;    // must implement IScenarioPanel

    [Header("Scenario Pool")]
    [SerializeField] private List<ScenarioDefinition> allScenarios = new();

    [Header("Queue")]
    [SerializeField] private int maxQueued = 3;

    [Header("Trigger Chances")]
    [Range(0f, 1f)] [SerializeField] private float outsideChance = 0.35f;
    [Range(0f, 1f)] [SerializeField] private float shopChance = 0.25f;

    private readonly Queue<ScenarioDefinition> _queue = new();
    private bool _isShowing;

    // scenarioId -> last day index shown
    private readonly Dictionary<string, int> _lastShownDayById = new();
    
    private IScenarioPanel ComputerPanel => computerPanelBehaviour as IScenarioPanel;
    private IScenarioPanel PhonePanel => phonePanelBehaviour as IScenarioPanel;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
    }
    
    private void OnEnable()
    {
        if (gameManager == null) return;

        gameManager.OnLocationChanged += HandleLocationChanged;
        gameManager.OnTimeOfDayChanged += HandleTimeChanged;
    }
    
     private void OnDisable()
    {
        if (gameManager == null) return;

        gameManager.OnLocationChanged -= HandleLocationChanged;
        gameManager.OnTimeOfDayChanged -= HandleTimeChanged;
    }

    private void HandleLocationChanged(GameManager.Location location)
    {
        // Trigger attempt on location change (with your rules)
        TryEnqueueFromContext(location);
    }

    private void HandleTimeChanged(GameManager.TimeOfDay _)
    {
        // Optional: you can also trigger on time change.
        // If you find it too frequent, remove this.
        TryEnqueueFromContext(gameManager.CurrentLocation);
    }

    private void TryEnqueueFromContext(GameManager.Location location)
    {
        if (_queue.Count >= maxQueued) return;

        float chance = GetChanceForLocation(location);
        if (chance <= 0f) return;

        // Home/Work = always (chance=1). Outside/Shop = sometimes.
        if (chance < 1f && UnityEngine.Random.value > chance)
            return;

        // Enqueue only ONE scenario per trigger (prevents constant 3)
        var picked = PickRandomEligibleWithCooldown();
        if (picked == null) return;

        _queue.Enqueue(picked);

        if (!_isShowing)
            ShowNext();
    }

    private float GetChanceForLocation(GameManager.Location location)
    {
        switch (location)
        {
            case GameManager.Location.Home:
            case GameManager.Location.Work:
                return 1f;

            case GameManager.Location.Outside:
                return outsideChance;

            case GameManager.Location.Shop:
                return shopChance;

            default:
                return 0f;
        }
    }

    private ScenarioDefinition PickRandomEligibleWithCooldown()
    {
        List<ScenarioDefinition> candidates = null;
        float totalWeight = 0f;

        for (int i = 0; i < allScenarios.Count; i++)
        {
            var s = allScenarios[i];
            if (s == null) continue;

            if (!s.CanRun(gameManager, gameManager.CurrentTime)) continue;
            if (IsAlreadyQueued(s)) continue;
            if (IsOnCooldown(s)) continue;

            float w = Mathf.Max(0f, s.weight);
            if (w <= 0f) continue;

            candidates ??= new List<ScenarioDefinition>();
            candidates.Add(s);
            totalWeight += w;
        }

        if (candidates == null || candidates.Count == 0 || totalWeight <= 0f)
            return null;

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float acc = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            acc += Mathf.Max(0f, candidates[i].weight);
            if (roll <= acc)
                return candidates[i];
        }

        return candidates[candidates.Count - 1];
    }

    private bool IsOnCooldown(ScenarioDefinition s)
    {
        if (s == null) return true;

        // If no id provided, treat as always eligible (or you can treat as never eligible)
        if (string.IsNullOrWhiteSpace(s.scenarioId))
            return false;

        if (!_lastShownDayById.TryGetValue(s.scenarioId, out int lastDay))
            return false;

        int today = gameManager != null ? gameManager.CurrentDayIndex : 0;
        int daysSince = today - lastDay;

        return daysSince < s.minDaysBetweenShows;
    }

    private bool IsAlreadyQueued(ScenarioDefinition s)
    {
        foreach (var q in _queue)
        {
            if (q == s) return true;
        }
        return false;
    }

    private void ShowNext()
    {
        if (_isShowing) return;
        if (_queue.Count == 0) return;

        var scenario = _queue.Dequeue();
        if (scenario == null) { ShowNext(); return; }

        // Re-check conditions (context might have changed)
        if (!scenario.CanRun(gameManager, gameManager.CurrentTime) || IsOnCooldown(scenario))
        {
            ShowNext();
            return;
        }

        var panel = ResolvePanelForCurrentContext();
        if (panel == null)
        {
            Debug.LogWarning("ScenarioManager: No panel available for current context.");
            return;
        }

        _isShowing = true;

        panel.Show(
            scenario.prompt,
            scenario.choices[0].buttonText,
            scenario.choices[1].buttonText,
            scenario.choices[2].buttonText,
            choiceIndex =>
            {
                ApplyChoice(scenario, choiceIndex);
                MarkShownToday(scenario);

                panel.Hide();
                _isShowing = false;

                // Show next queued scenario if any
                ShowNext();
            });
    }

    private void MarkShownToday(ScenarioDefinition s)
    {
        if (s == null) return;
        if (string.IsNullOrWhiteSpace(s.scenarioId)) return;

        int today = gameManager != null ? gameManager.CurrentDayIndex : 0;
        _lastShownDayById[s.scenarioId] = today;
    }

    private IScenarioPanel ResolvePanelForCurrentContext()
    {
        if (gameManager != null && gameManager.CurrentLocation == GameManager.Location.Home)
            return ComputerPanel;

        return PhonePanel;
    }

    private void ApplyChoice(ScenarioDefinition scenario, int choiceIndex)
    {
        if (scenario == null) return;
        if (choiceIndex < 0 || choiceIndex > 2) return;

        var effects = scenario.choices[choiceIndex].effects;
        if (effects == null) return;

        for (int i = 0; i < effects.Count; i++)
            effects[i].Apply(gameManager);
    }
}
