using System.Collections.Generic;

// Pure combat logic. No nodes, no scenes, no Godot.
public static class Combat
{
    // Damage-vs-armor table (tech doc section 5.3).
    // Anything not listed here multiplies by NeutralMultiplier (1.0).
    // Values come from CombatConstants — tune there, not here.
    private static readonly Dictionary<(DamageType, ArmorType), float> Multipliers = new()
    {
        { (DamageType.Energy,    ArmorType.Light),  CombatConstants.StrongMultiplier },
        { (DamageType.Energy,    ArmorType.Heavy),  CombatConstants.WeakMultiplier },
        { (DamageType.Kinetic,   ArmorType.Light),  CombatConstants.StrongMultiplier },
        { (DamageType.Kinetic,   ArmorType.Medium), CombatConstants.SlightAdvantageMultiplier },
        { (DamageType.Explosive, ArmorType.Heavy),  CombatConstants.StrongMultiplier },
        { (DamageType.Explosive, ArmorType.Light),  CombatConstants.SlightPenaltyMultiplier },
    };

    public static HeatBand GetHeatBand(int tension)
    {
        if (tension >= CombatConstants.MaxTension) return HeatBand.Overload;
        if (tension >= CombatConstants.CriticalHeatThreshold) return HeatBand.Critical;
        if (tension >= CombatConstants.OptimalHeatThreshold) return HeatBand.Optimal;
        return HeatBand.Cold;
    }

    public static float GetOffensiveHeatMultiplier(int tension)
    {
        var band = GetHeatBand(tension);
        return band is HeatBand.Optimal or HeatBand.Critical or HeatBand.Overload
            ? CombatConstants.HeatDamageBonusMultiplier
            : CombatConstants.NeutralMultiplier;
    }

    public static float GetDefensiveHeatMultiplier(int tension)
    {
        var band = GetHeatBand(tension);
        return band is HeatBand.Critical or HeatBand.Overload
            ? CombatConstants.HeatDefensePenaltyMultiplier
            : CombatConstants.NeutralMultiplier;
    }

    public static int CalculateDamage(int baseDamage, DamageType type, ArmorType armor, int attackerTension, int targetTension)
    {
        float armorMult = Multipliers.GetValueOrDefault((type, armor), CombatConstants.NeutralMultiplier);
        float offensiveHeatMult = GetOffensiveHeatMultiplier(attackerTension);
        float defensiveHeatMult = GetDefensiveHeatMultiplier(targetTension);
        return (int)(baseDamage * armorMult * offensiveHeatMult * defensiveHeatMult);
    }
}
