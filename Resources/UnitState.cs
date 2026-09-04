using System.Collections.Generic;

// Pure C# — combat state does not know Godot exists.
// This is what allows testing the logic without opening the engine
// (tech doc, section 5.1).
public class UnitState
{
    public string Name = "";
    public int Hp = CombatConstants.DefaultUnitHp;
    public ArmorType Armor = ArmorType.Medium;
    public int MoveRange = CombatConstants.DefaultMoveRange;

    // Tension resource, defined as HEAT (tech doc 5.3). Field name kept as
    // "Tension" because WeaponData.cs (frozen) writes attacker.Tension --
    // renaming to Heat is tracked as debt until that file can be touched.
    public int Tension = 0;

    public bool CanMove = true;
    public bool CanAttack = true;

    public bool HasActionsLeft => CanMove || CanAttack;

    public void BeginTurn()
    {
        CanMove = true;
        CanAttack = true;
    }

    // End-of-turn heat dissipation. The bonus/penalty curve for high heat
    // is a separate iteration -- this is only the cooldown hook.
    public void DissipateHeat()
    {
        Tension = System.Math.Max(0, Tension - CombatConstants.HeatDissipationPerTurn);
    }
}

// Everything an effect needs to know in order to apply itself.
// When the grid arrives, positions, cover and line of sight go here.
public class AttackContext
{
    public UnitState Attacker;
    public UnitState Target;
    public List<string> Log = new();
}
