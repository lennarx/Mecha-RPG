// Static handoff between ConfigScreen and Mission -- Godot scene changes
// don't pass parameters directly, so the config screen writes the player's
// choices here before calling GetTree().ChangeSceneToFile(). Mission.cs
// falls back to each Unit's inspector-assigned Chassis/Weapon when a field
// is left null here, so Mission.tscn still runs standalone (F6) for
// debugging without going through ConfigScreen first.
public static class MissionLoadout
{
	public static ChassisData PlayerChassis;
	public static WeaponData PlayerWeapon;
	public static ChassisData EnemyChassis;
	public static WeaponData EnemyWeapon;
	public static string EnemyName = CharacterNames.TrainingDummy;
}
