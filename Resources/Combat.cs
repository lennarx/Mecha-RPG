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
        { (DamageType.Kinetic,   ArmorType.Medium), CombatConstants.SlightAdvantageMultiplier },
        { (DamageType.Explosive, ArmorType.Heavy),  CombatConstants.StrongMultiplier },
        { (DamageType.Explosive, ArmorType.Light),  CombatConstants.SlightPenaltyMultiplier },
    };

    public static int CalculateDamage(int baseDamage, DamageType type, ArmorType armor)
    {
        float mult = Multipliers.GetValueOrDefault((type, armor), CombatConstants.NeutralMultiplier);
        return (int)(baseDamage * mult);
    }
}
