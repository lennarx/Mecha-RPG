using Godot;
using System.Collections.Generic;

// THE central principle of the tech doc turned into code:
// a weapon is DATA. Each concrete weapon will be a .tres file
// created in the editor, without writing a single line of C#.
[GlobalClass]
public partial class WeaponData : Resource
{
    [Export] public string DisplayName { get; set; } = "";
    [Export] public int Range { get; set; } = CombatConstants.DefaultWeaponRange;
    [Export] public int TensionCost { get; set; } = CombatConstants.DefaultTensionCost;
    [Export] public Godot.Collections.Array<EffectData> Effects { get; set; } = new();

    // Entry point: fires every effect of the weapon, in order.
    public void ResolveAttack(UnitState attacker, UnitState target, List<string> log)
    {
        attacker.Tension += TensionCost;
        log.Add($"{attacker.Name} fires {DisplayName} (+{TensionCost} tension, total {attacker.Tension})");

        var ctx = new AttackContext { Attacker = attacker, Target = target, Log = log };
        foreach (var effect in Effects)
            effect.Apply(ctx);
    }
}
