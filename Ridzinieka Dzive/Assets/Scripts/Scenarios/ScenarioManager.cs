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
    
    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private readonly Queue<ScenarioDefinition> _queue = new();
    private bool _isShowing;
    private bool _isComputerOpen;
    
    private ScenarioDefinition _currentScenario;

    // scenarioId -> last day index shown
    private readonly Dictionary<string, int> _lastShownDayById = new();

    private IScenarioPanel ComputerPanel => computerPanelBehaviour as IScenarioPanel;
    private IScenarioPanel PhonePanel => phonePanelBehaviour as IScenarioPanel;

    public bool HasPendingScenarios => _isShowing || _queue.Count > 0;

    public bool HasPendingMandatoryHomeScenario
    {
        get
        {
            if (gameManager == null) return false;
            if (gameManager.CurrentLocation != GameManager.Location.Home) return false;

            if (_currentScenario != null && _isShowing && _currentScenario.isMandatory) return true;

            foreach (var s in _queue)
            {
                if (s != null && s.isMandatory) return true;
            }

            return false;
        }
    }

    public bool IsHomeBlocked(out string reason)
    {
        if (HasPendingMandatoryHomeScenario)
        {
            reason = "Pagaidi! Tev atnāca ziņa datorā!";
            return true;
        }

        reason = null;
        return false;
    }
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

        // Kick off an attempt immediately for testing / scene load cases
        TryEnqueueFromContext(gameManager.CurrentLocation);
    }
    
     private void OnDisable()
    {
        if (gameManager == null) return;

        gameManager.OnLocationChanged -= HandleLocationChanged;
        gameManager.OnTimeOfDayChanged -= HandleTimeChanged;
    }
     
    public void NotifyComputerOpened()
    {
        _isComputerOpen = true;

        if (gameManager != null && gameManager.CurrentLocation == GameManager.Location.Home)
            ShowNext();
    }

    public void NotifyComputerClosed()
    {
        _isComputerOpen = false;
    }

    private void HandleLocationChanged(GameManager.Location location)
    {
        TryEnqueueFromContext(location);

        // If we arrive home and the computer is open, show queued scenarios.
        if (location == GameManager.Location.Home && _isComputerOpen)
            ShowNext();
    }
    private void HandleTimeChanged(GameManager.TimeOfDay _)
    {
        TryEnqueueFromContext(gameManager.CurrentLocation);

        if (gameManager != null && gameManager.CurrentLocation == GameManager.Location.Home && _isComputerOpen)
            ShowNext();
    }
    private void TryEnqueueFromContext(GameManager.Location location)
    {
        if (_queue.Count >= maxQueued) return;

        float chance = GetChanceForLocation(location);
        if (chance <= 0f) return;

        if (chance < 1f && UnityEngine.Random.value > chance)
            return;

        var picked = PickRandomEligibleWithCooldown();
        if (picked == null) return;

        _queue.Enqueue(picked);

        // IMPORTANT:
        // - Outside/Work/Shop: show immediately (phone UI).
        // - Home: do NOT show until computer is opened.
        if (!_isShowing && location != GameManager.Location.Home)
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
        if (gameManager == null) return;

        // Home gating: only show when computer is open
        if (gameManager.CurrentLocation == GameManager.Location.Home && !_isComputerOpen)
            return;

        var panel = ResolvePanelForCurrentContext();
        if (panel == null)
        {
            Debug.LogWarning("ScenarioManager: No panel available for current context.");
            return;
        }

        var scenario = _queue.Dequeue();
        if (scenario == null) { ShowNext(); return; }

        if (!scenario.CanRun(gameManager, gameManager.CurrentTime) || IsOnCooldown(scenario))
        {
            ShowNext();
            return;
        }

        _currentScenario = scenario;
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

                _currentScenario = null;
                panel.Hide();
                _isShowing = false;

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
