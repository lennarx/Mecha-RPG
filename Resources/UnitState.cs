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

    // Tension resource (heat vs energy — still open in the tech doc).
    // Generic name on purpose: when you decide, you rename one thing.
    public int Tension = 0;
}

// Everything an effect needs to know in order to apply itself.
// When the grid arrives, positions, cover and line of sight go here.
public class AttackContext
{
    public UnitState Attacker;
    public UnitState Target;
    public List<string> Log = new();
}
