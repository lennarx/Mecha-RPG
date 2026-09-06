using Godot;

// Chassis-as-data, same principle as WeaponData: concrete chassis are
// authored as .tres files in the editor, never hardcoded in C# (tech doc
// 5.1, 5.2). Mission.cs builds a UnitState from these values instead of
// using literals.
[GlobalClass]
public partial class ChassisData : Resource
{
	[Export] public string DisplayName { get; set; } = "";
	[Export] public int Hp { get; set; } = CombatConstants.DefaultUnitHp;
	[Export] public ArmorType Armor { get; set; } = ArmorType.Medium;
	[Export] public int MoveRange { get; set; } = CombatConstants.DefaultMoveRange;
	[Export] public int WeaponSlots { get; set; } = 1;
}
