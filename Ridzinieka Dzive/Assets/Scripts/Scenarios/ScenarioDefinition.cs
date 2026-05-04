using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scenarios/Scenario Definition")]
public class ScenarioDefinition : ScriptableObject
{
    [Header("Identity")]
    public string scenarioId;

    [Tooltip("Higher = more likely when randomly choosing among eligible scenarios.")]
    [Min(0f)] public float weight = 1f;

    [Header("Repeat Control")]
    [Tooltip("Minimum number of in-game days between this scenario showing again.")]
    [Min(0)] public int minDaysBetweenShows = 3;
    
    [Header("Rules")]
    [Tooltip("If true: player must resolve this scenario before leaving home / advancing time/day.")]
    public bool isMandatory = true;
    
    [Header("Content")]
    [TextArea(2, 6)] public string prompt;

    [Header("Conditions")]
    public List<GameManager.Location> allowedLocations = new();
    public List<GameManager.TimeOfDay> allowedTimes = new();

    [Tooltip("If true, this scenario can only run right after the player wakes up into a new morning.")]
    public bool requiresFreshWakeUpMorning;

    [Tooltip("If true, this scenario can only run on days where the selected job is scheduled to work.")]
    public bool requiresWorkDay;

    [Tooltip("If true, this scenario can only run while the player has a fever.")]
    public bool requiresFever;

    public List<StatRequirement> statRequirements = new();

    [Tooltip("If empty: scenario can run for any job. If not empty: requires the player's selected job to be listed.")]
    public List<GameManager.JobType> allowedJobs = new();

    [Header("Transport-only Scenarios (optional)")]
    [Tooltip("Leave empty for normal scenarios. If set, this scenario is only eligible for these transport modes (used by transport-specific flow).")]
    public List<GameManager.TransportMode> allowedTransportModes = new();

    [Header("Choices (2 to 3)")]
    public Choice[] choices = Array.Empty<Choice>();

    public bool CanRun(GameManager gm, GameManager.TimeOfDay currentTimeOfDay)
    {
        if (gm == null) return false;

        if (allowedLocations.Count > 0 && !allowedLocations.Contains(gm.CurrentLocation))
            return false;

        if (allowedTimes.Count > 0 && !allowedTimes.Contains(gm.CurrentTime))
            return false;

        if (requiresFreshWakeUpMorning && !gm.HasFreshWakeUpMorningScenarioWindow)
            return false;

        if (requiresWorkDay && !IsCurrentDayWorkDay(gm))
            return false;

        if (requiresFever && !gm.HasFever)
            return false;

        if (allowedJobs.Count > 0 && !allowedJobs.Contains(gm.SelectedJob))
            return false;

        if (choices == null || choices.Length < 2 || choices.Length > 3)
            return false;

        if (string.IsNullOrWhiteSpace(choices[0].buttonText)) return false;
        if (string.IsNullOrWhiteSpace(choices[1].buttonText)) return false;

        if (choices.Length == 3 && string.IsNullOrWhiteSpace(choices[2].buttonText))
            return false;

        for (int i = 0; i < statRequirements.Count; i++)
        {
            if (!statRequirements[i].IsMet(gm))
                return false;
        }

        return true;
    }

    private static bool IsCurrentDayWorkDay(GameManager gm)
    {
        if (gm == null) return false;

        JobManager jobManager = UnityEngine.Object.FindFirstObjectByType<JobManager>();
        if (jobManager == null) return false;

        return jobManager.IsWorkDay(gm.CurrentDayIndex);
    }

    [Serializable]
    public struct Choice
    {
        public string buttonText;

        [Tooltip("If true, choosing this option blocks going to work for the rest of the current day.")]
        public bool blocksWorkToday;

        [Header("Food")]
        [Tooltip("If true, this choice consumes one available food item from the player's fridge/inventory. If no food exists, the choice will fail and the scenario stays open.")]
        public bool consumesFridgeFood;

        [Header("Sickness")]
        [Tooltip("If true, choosing this option starts sick leave. Sick leave lasts 3 days, pays salary, and restores health.")]
        public bool startsSickLeave;

        [Tooltip("If true, choosing this option means the player goes to work while sick and loses health.")]
        public bool goesToWorkWhileSick;

        public List<StatDelta> effects;
    }

    [Serializable]
    public struct StatRequirement
    {
        public StatType stat;
        public int minInclusive;
        public int maxInclusive;

        public bool IsMet(GameManager gm)
        {
            int value = ScenarioStatAccess.GetStatValue(gm, stat);
            return value >= minInclusive && value <= maxInclusive;
        }
    }

    [Serializable]
    public struct StatDelta
    {
        public StatType stat;
        public int amount; // positive or negative

        public void Apply(GameManager gm)
        {
            ScenarioStatAccess.ApplyDelta(gm, stat, amount);
        }
    }
}

public enum StatType
{
    Money,
    Hunger,
    Energy,
    Stress,
    Health
}

