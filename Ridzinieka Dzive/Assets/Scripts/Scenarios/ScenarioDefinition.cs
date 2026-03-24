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
    
    [Header("Content")]
    [TextArea(2, 6)] public string prompt;

    [Header("Conditions")]
    public List<GameManager.Location> allowedLocations = new();
    public List<GameManager.TimeOfDay> allowedTimes = new();
    public List<StatRequirement> statRequirements = new();

    [Header("Choices (exactly 3)")]
    public Choice[] choices = new Choice[3];

    public bool CanRun(GameManager gm, GameManager.TimeOfDay currentTimeOfDay)
    {
        if (gm == null) return false;

        if (allowedLocations.Count > 0 && !allowedLocations.Contains(gm.CurrentLocation))
            return false;

        if (allowedTimes.Count > 0 && !allowedTimes.Contains(gm.CurrentTime))
            return false;

        if (choices == null || choices.Length != 3)
            return false;

        for (int i = 0; i < statRequirements.Count; i++)
        {
            if (!statRequirements[i].IsMet(gm))
                return false;
        }

        return true;
    }

    [Serializable]
    public struct Choice
    {
        public string buttonText;
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

