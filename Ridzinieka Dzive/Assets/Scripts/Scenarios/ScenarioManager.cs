using System;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [Header("Panels (assign in Inspector)")]
    [SerializeField] private MonoBehaviour computerPanelBehaviour; // must implement IScenarioPanel
    [SerializeField] private MonoBehaviour phonePanelBehaviour;    // must implement IScenarioPanel

    [Header("Phone Gating")]
    [SerializeField] private PhoneUI phoneUI; // assign in Inspector (recommended)

    [Header("Home Area Gating")]
    [SerializeField] private CameraMovement cameraMovement; // assign in Inspector (recommended)

    [Header("Attention Icons (assign in Inspector)")]
    [SerializeField] private GameObject computerAttentionIcon; // exclamation mark next to computer
    [SerializeField] private GameObject phoneAttentionIcon;    // exclamation mark next to phone

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
    private bool _isPhoneOpen;

    private CameraMovement.HomeArea _homeArea = CameraMovement.HomeArea.Computer;

    private ScenarioDefinition _currentScenario;

    public bool IsScenarioActive => _isShowing;
    public event Action<bool> ScenarioActiveChanged;

    private void SetScenarioActive(bool active)
    {
        if (_isShowing == active)
            return;

        _isShowing = active;
        ScenarioActiveChanged?.Invoke(_isShowing);
    }

    // NEW: “forced scenario” pipeline (used by OutsideUI flow)
    private ScenarioDefinition _forcedScenario;
    private Action _forcedOnComplete;

    // NEW: callback invoked only after ALL scenarios are done (queue drained)
    private Action _afterScenarioDrain;

    // scenarioId -> last day index shown
    private readonly Dictionary<string, int> _lastShownDayById = new();

    private IScenarioPanel ComputerPanel => computerPanelBehaviour as IScenarioPanel;
    private IScenarioPanel PhonePanel => phonePanelBehaviour as IScenarioPanel;

    public bool HasPendingScenarios => _isShowing || _queue.Count > 0 || _forcedScenario != null;

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

    public bool HasPendingMandatoryWorkScenario
    {
        get
        {
            if (gameManager == null) return false;
            if (gameManager.CurrentLocation != GameManager.Location.Work) return false;

            if (_currentScenario != null && _isShowing && _currentScenario.isMandatory) return true;

            if (_forcedScenario != null && _forcedScenario.isMandatory) return true;

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

    public bool IsWorkBlocked(out string reason)
    {
        if (HasPendingMandatoryWorkScenario)
        {
            reason = "Pagaidi! Tev jāatbild uz ziņu telefonā!";
            return true;
        }

        reason = null;
        return false;
    }
    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
        
        if (phoneUI == null)
            phoneUI = FindFirstObjectByType<PhoneUI>();

        if (cameraMovement == null)
            cameraMovement = FindFirstObjectByType<CameraMovement>();
    }
    
    private void OnEnable()
    {
        if (gameManager == null) return;

        gameManager.OnLocationChanged += HandleLocationChanged;
        gameManager.OnTimeOfDayChanged += HandleTimeChanged;
        
        if (phoneUI != null)
        {
            _isPhoneOpen = phoneUI.IsOpen;
            phoneUI.Opened += NotifyPhoneOpened;
            phoneUI.Closed += NotifyPhoneClosed;
        }

        if (cameraMovement != null)
        {
            cameraMovement.OnHomeAreaChanged += HandleHomeAreaChanged;
        }

        // Kick off an attempt immediately for testing / scene load cases
        TryEnqueueFromContext(gameManager.CurrentLocation);

        UpdateAttentionIcons();
    }

    private void OnDisable()
    {
        if (gameManager == null) return;

        gameManager.OnLocationChanged -= HandleLocationChanged;
        gameManager.OnTimeOfDayChanged -= HandleTimeChanged;
        
        if (phoneUI != null)
        {
            phoneUI.Opened -= NotifyPhoneOpened;
            phoneUI.Closed -= NotifyPhoneClosed;
        }

        if (cameraMovement != null)
        {
            cameraMovement.OnHomeAreaChanged -= HandleHomeAreaChanged;
        }

        SetActiveSafe(computerAttentionIcon, false);
        SetActiveSafe(phoneAttentionIcon, false);
    }

    private void HandleHomeAreaChanged(CameraMovement.HomeArea area)
    {
        _homeArea = area;
        UpdateAttentionIcons();
    }

    public void NotifyComputerOpened()
    {
        _isComputerOpen = true;

        UpdateAttentionIcons();

        if (gameManager != null && gameManager.CurrentLocation == GameManager.Location.Home)
            ShowNext();
    }

    public void NotifyComputerClosed()
    {
        _isComputerOpen = false;
        UpdateAttentionIcons();
    }

    private void NotifyPhoneOpened()
    {
        _isPhoneOpen = true;

        UpdateAttentionIcons();

        // NEW: if we were waiting to show a forced phone scenario, do it now
        TryShowForcedScenario();

        // If we're not at home, phone scenarios are allowed once phone is fully open.
        if (gameManager != null && gameManager.CurrentLocation != GameManager.Location.Home)
            ShowNext();
    }

    private void NotifyPhoneClosed()
    {
        _isPhoneOpen = false;
        UpdateAttentionIcons();
    }

    private void HandleLocationChanged(GameManager.Location location)
    {
        TryEnqueueFromContext(location);

        UpdateAttentionIcons();

        // If we arrive home and the computer is open, show queued scenarios.
        if (location == GameManager.Location.Home && _isComputerOpen)
            ShowNext();
    }

    private void HandleTimeChanged(GameManager.TimeOfDay _)
    {
        TryEnqueueFromContext(gameManager.CurrentLocation);

        UpdateAttentionIcons();

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

        UpdateAttentionIcons();

        // IMPORTANT:
        // - Outside/Work/Shop: show immediately (phone UI).
        // - Home: do NOT show until computer is opened.
        if (!_isShowing && location != GameManager.Location.Home && _isPhoneOpen)
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

            // NEW: while Outside, do NOT enqueue transport-specific scenarios.
            // Transport-specific scenarios must only appear via RequestTransportScenario(mode, ...).
            if (gameManager != null &&
                gameManager.CurrentLocation == GameManager.Location.Outside &&
                s.allowedTransportModes != null &&
                s.allowedTransportModes.Count > 0)
            {
                continue;
            }

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

    private void SetAfterDrainAction(Action onComplete)
    {
        if (onComplete == null) return;

        // Only one "after drain" action at a time; last writer wins.
        _afterScenarioDrain = onComplete;
    }

    private void MaybeInvokeAfterDrain()
    {
        if (_afterScenarioDrain == null) return;
        if (HasPendingScenarios) return;

        var a = _afterScenarioDrain;
        _afterScenarioDrain = null;
        a?.Invoke();
    }

    private void ForcePhoneScenario(ScenarioDefinition scenario, Action onComplete)
    {
        if (scenario == null)
        {
            onComplete?.Invoke();
            return;
        }

        // NEW: do not complete until all scenarios are done
        SetAfterDrainAction(onComplete);

        _forcedScenario = scenario;
        _forcedOnComplete = null; // no longer used for immediate completion

        UpdateAttentionIcons();
        TryShowForcedScenario();
    }

    private bool TryShowForcedScenario()
    {
        if (_isShowing) return false;
        if (_forcedScenario == null) return false;
        if (gameManager == null) return false;

        if (!_isPhoneOpen)
        {
            UpdateAttentionIcons();
            return true;
        }

        var panel = PhonePanel;
        if (panel == null)
        {
            Debug.LogWarning("ScenarioManager: Phone panel missing, cannot show forced scenario.");
            // We are "stuck"; allow after-drain to fire only if there is truly nothing pending.
            _forcedScenario = null;
            UpdateAttentionIcons();
            ShowNext();
            MaybeInvokeAfterDrain();
            return true;
        }

        var scenario = _forcedScenario;
        _forcedScenario = null;

        if (!scenario.CanRun(gameManager, gameManager.CurrentTime) || IsOnCooldown(scenario))
        {
            UpdateAttentionIcons();
            ShowNext();
            MaybeInvokeAfterDrain();
            return true;
        }

        _currentScenario = scenario;
        SetScenarioActive(true);

        UpdateAttentionIcons();

        string c1 = scenario.choices[0].buttonText;
        string c2 = scenario.choices[1].buttonText;
        string c3 = scenario.choices.Length >= 3 ? scenario.choices[2].buttonText : null;

        panel.Show(
            scenario.prompt,
            c1,
            c2,
            c3,
            choiceIndex =>
            {
                ApplyChoice(scenario, choiceIndex);
                MarkShownToday(scenario);
                var statsUI = FindFirstObjectByType<StatsUI>();
                if (statsUI != null)
                    statsUI.UpdateStats();

                _currentScenario = null;
                panel.Hide();
                SetScenarioActive(false);

                UpdateAttentionIcons();

                // Continue draining queue (if any)
                ShowNext();

                // NEW: only now (when everything is finished) allow the travel callback to run
                MaybeInvokeAfterDrain();
            });

        return true;
    }

    private void ShowNext()
    {
        if (TryShowForcedScenario())
            return;

        if (_isShowing) return;
        if (_queue.Count == 0)
        {
            UpdateAttentionIcons();
            MaybeInvokeAfterDrain();
            return;
        }
        if (gameManager == null) return;

        // Home gating: only show when computer is open
        if (gameManager.CurrentLocation == GameManager.Location.Home && !_isComputerOpen)
        {
            UpdateAttentionIcons();
            return;
        }

        // Phone gating: only show when phone is fully open
        if (gameManager.CurrentLocation != GameManager.Location.Home && !_isPhoneOpen)
        {
            UpdateAttentionIcons();
            return;
        }

        var panel = ResolvePanelForCurrentContext();
        if (panel == null)
        {
            Debug.LogWarning("ScenarioManager: No panel available for current context.");
            return;
        }

        var scenario = _queue.Dequeue();
        if (scenario == null) { UpdateAttentionIcons(); ShowNext(); return; }

        if (!scenario.CanRun(gameManager, gameManager.CurrentTime) || IsOnCooldown(scenario))
        {
            UpdateAttentionIcons();
            ShowNext();
            return;
        }

        _currentScenario = scenario;
        SetScenarioActive(true);

        UpdateAttentionIcons();

        string c1 = scenario.choices[0].buttonText;
        string c2 = scenario.choices[1].buttonText;
        string c3 = scenario.choices.Length >= 3 ? scenario.choices[2].buttonText : null;

        panel.Show(
            scenario.prompt,
            c1,
            c2,
            c3,
            choiceIndex =>
            {
                ApplyChoice(scenario, choiceIndex);
                MarkShownToday(scenario);
                var statsUI = FindFirstObjectByType<StatsUI>();
                if (statsUI != null)
                    statsUI.UpdateStats();

                _currentScenario = null;
                panel.Hide();
                SetScenarioActive(false);

                UpdateAttentionIcons();

                ShowNext();

                // NEW: in case this was the last one
                MaybeInvokeAfterDrain();
            });
    }

    // NEW: called by OutsideUI - step 1 (normal Outside scenario before transport menu)
    public void RequestPreTransportOutsideScenario(Action onComplete)
    {
        if (gameManager == null)
        {
            onComplete?.Invoke();
            return;
        }

        // If we are not outside (yet), don't force anything.
        if (gameManager.CurrentLocation != GameManager.Location.Outside)
        {
            onComplete?.Invoke();
            return;
        }

        // Pick an “Outside” scenario that is NOT transport-specific (allowedTransportModes empty).
        var scenario = PickRandomEligibleOutsideNonTransportScenario();
        if (scenario == null)
        {
            onComplete?.Invoke();
            return;
        }

        ForcePhoneScenario(scenario, onComplete);
    }

    // NEW: called by OutsideUI - step 2 (transport-specific scenario after choosing transport)
    public void RequestTransportScenario(GameManager.TransportMode mode, Action onComplete)
    {
        if (gameManager == null)
        {
            onComplete?.Invoke();
            return;
        }

        // Must be outside for this flow
        if (gameManager.CurrentLocation != GameManager.Location.Outside)
        {
            onComplete?.Invoke();
            return;
        }

        var scenario = PickRandomEligibleTransportScenario(mode);
        if (scenario == null)
        {
            onComplete?.Invoke();
            return;
        }

        ForcePhoneScenario(scenario, onComplete);
    }

    private ScenarioDefinition PickRandomEligibleOutsideNonTransportScenario()
    {
        List<ScenarioDefinition> candidates = null;
        float totalWeight = 0f;

        for (int i = 0; i < allScenarios.Count; i++)
        {
            var s = allScenarios[i];
            if (s == null) continue;

            // Must be outside-eligible by normal rules
            if (!s.CanRun(gameManager, gameManager.CurrentTime)) continue;
            if (IsOnCooldown(s)) continue;

            // Filter out transport-specific scenarios
            if (s.allowedTransportModes != null && s.allowedTransportModes.Count > 0) continue;

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

    private ScenarioDefinition PickRandomEligibleTransportScenario(GameManager.TransportMode mode)
    {
        List<ScenarioDefinition> candidates = null;
        float totalWeight = 0f;

        for (int i = 0; i < allScenarios.Count; i++)
        {
            var s = allScenarios[i];
            if (s == null) continue;

            if (!s.CanRun(gameManager, gameManager.CurrentTime)) continue;
            if (IsOnCooldown(s)) continue;

            if (s.allowedTransportModes == null || s.allowedTransportModes.Count == 0) continue;
            if (!s.allowedTransportModes.Contains(mode)) continue;

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

    private void UpdateAttentionIcons()
    {
        if (gameManager == null)
        {
            SetActiveSafe(computerAttentionIcon, false);
            SetActiveSafe(phoneAttentionIcon, false);
            return;
        }

        // 1) If the big phone UI is open, never show attention icons (prevents drawing over the phone).
        if (_isPhoneOpen)
        {
            SetActiveSafe(computerAttentionIcon, false);
            SetActiveSafe(phoneAttentionIcon, false);
            return;
        }

        // 2) If we're at home but not in the "Computer" area, hide home icons.
        if (gameManager.CurrentLocation == GameManager.Location.Home &&
            _homeArea != CameraMovement.HomeArea.Computer)
        {
            SetActiveSafe(computerAttentionIcon, false);
            SetActiveSafe(phoneAttentionIcon, false);
            return;
        }

        bool hasQueued = _queue.Count > 0;
        bool hasForcedPhoneScenario = _forcedScenario != null;

        bool needsComputerAttention =
            hasQueued &&
            gameManager.CurrentLocation == GameManager.Location.Home &&
            !_isComputerOpen &&
            !_isShowing;

        bool needsPhoneAttention =
            (hasForcedPhoneScenario || hasQueued) &&
            gameManager.CurrentLocation != GameManager.Location.Home &&
            !_isPhoneOpen &&
            !_isShowing;

        SetActiveSafe(computerAttentionIcon, needsComputerAttention);
        SetActiveSafe(phoneAttentionIcon, needsPhoneAttention);
    }

    private void ApplyChoice(ScenarioDefinition scenario, int choiceIndex)
    {
        if (scenario == null) return;
        if (scenario.choices == null) return;
        if (choiceIndex < 0 || choiceIndex >= scenario.choices.Length) return;

        var effects = scenario.choices[choiceIndex].effects;
        if (effects == null) return;

        for (int i = 0; i < effects.Count; i++)
            effects[i].Apply(gameManager);
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

    private static void SetActiveSafe(GameObject go, bool active)
    {
        if (go == null) return;
        if (go.activeSelf == active) return;
        go.SetActive(active);
    }
}
