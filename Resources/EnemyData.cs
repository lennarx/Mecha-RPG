using Godot;

// An enemy type is a chassis + weapon pair, nothing more: new enemy types
// come from new .tres files, never from new C# code (definiciones-tecnicas
// section 2). The enemy's displayed name still comes from
// GameConstants.CharacterNames, not from a field here.
[GlobalClass]
public partial class EnemyData : Resource
{
	[Export] public ChassisData Chassis { get; set; }
	[Export] public WeaponData Weapon { get; set; }
}
