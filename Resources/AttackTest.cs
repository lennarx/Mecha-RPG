using Godot;
using System.Collections.Generic;

// Smoke test for the data system: attach to a Node2D,
// drag a WeaponData .tres into the "Weapon" field in the Inspector, then F6.
public partial class AttackTest : Node2D
{
    [Export] public WeaponData Weapon { get; set; }

    public override void _Ready()
    {
        if (Weapon == null)
        {
            GD.PrintErr("Assign a WeaponData in the Inspector (Weapon field).");
            return;
        }

        // Names come from CharacterNames — never from string literals here.
        var attacker = new UnitState { Name = CharacterNames.Protagonist };
        var target = new UnitState { Name = "Enemy Drone", Hp = 25, Armor = ArmorType.Light };

        var log = new List<string>();
        Weapon.ResolveAttack(attacker, target, log);

        foreach (var line in log)
            GD.Print(line);

        GD.Print($"{target.Name} remaining HP: {target.Hp}");
        GD.Print(target.Hp <= 0 ? "Target destroyed." : "Target still standing.");
    }
}
