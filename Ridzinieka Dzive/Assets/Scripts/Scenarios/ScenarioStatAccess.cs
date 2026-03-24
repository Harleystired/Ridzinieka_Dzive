using UnityEngine;

public static class ScenarioStatAccess
{
    public static int GetStatValue(GameManager gm, StatType stat)
    {
        // Map these to your actual GameManager fields.
        // Keep it centralized here.
        switch (stat)
        {
            case StatType.Money:
                return gm.money; // example field name
            case StatType.Energy:
                return gm.energy;
            case StatType.Hunger:
                return gm.hunger;
            case StatType.Stress:
                return gm.stress;
            case StatType.Health:
                return gm.health;
            default:
                Debug.LogWarning($"Unhandled stat: {stat}");
                return 0;
        }
    }

    public static void ApplyDelta(GameManager gm, StatType stat, int amount)
    {
        // Prefer calling your Add/Remove methods here (as you described).
        // If you only have AddX / RemoveX, use amount sign.
        switch (stat)
        {
            case StatType.Money:
                if (amount >= 0) gm.AddMoney(amount);
                else gm.SpendMoney(-amount);
                break;

            case StatType.Energy:
                if (amount >= 0) gm.AddEnergy(amount);
                else gm.RemoveEnergy(-amount);
                break;

            case StatType.Hunger:
                if (amount >= 0) gm.AddHunger(amount);
                else gm.RemoveHunger(-amount);
                break;

            case StatType.Stress:
                if (amount >= 0) gm.AddStress(amount);
                else gm.RemoveStress(-amount);
                break;
            
            case StatType.Health:
                if (amount >= 0) gm.AddHealth(amount);
                else gm.RemoveHealth(-amount);
                break;

            default:
                Debug.LogWarning($"Unhandled stat: {stat}");
                break;
        }
    }
}
