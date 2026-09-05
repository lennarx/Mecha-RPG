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

    // Tension resource, defined as HEAT (tech doc 5.3). Field kept named
    // "Tension" for now; UI-facing text says "Heat". Clamped to
    // [0, MaxTension] here so every writer (WeaponData, Mission) gets the
    // cap for free without needing to know about it.
    private int _tension;
    public int Tension
    {
        get => _tension;
        set => _tension = System.Math.Clamp(value, 0, CombatConstants.MaxTension);
    }

    public bool IsOverloaded => Tension >= CombatConstants.MaxTension;

    public bool CanMove = true;
    public bool CanAttack = true;

    // Captured at the start of the turn so DissipateHeat can tell a turn
    // that was blocked by overload apart from one where actions were
    // simply used up -- both dissipate the same, but only the former means
    // CanMove/CanAttack started this turn already false.
    private bool _wasOverloadedAtTurnStart;

    public bool HasActionsLeft => CanMove || CanAttack;

    public void BeginTurn()
    {
        _wasOverloadedAtTurnStart = IsOverloaded;
        CanMove = !_wasOverloadedAtTurnStart;
        CanAttack = !_wasOverloadedAtTurnStart;
    }

    // End-of-turn heat dissipation. A turn where no action was taken --
    // whether by choice (cooling down) or because overload locked the unit
    // out -- dissipates double (tech doc 5.3).
    public void DissipateHeat()
    {
        bool noActionsTaken = _wasOverloadedAtTurnStart || (CanMove && CanAttack);
        int amount = noActionsTaken
            ? CombatConstants.NoActionHeatDissipation
            : CombatConstants.HeatDissipationPerTurn;
        Tension -= amount;
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
