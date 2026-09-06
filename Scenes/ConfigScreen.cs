using Godot;

// Pre-mission loadout screen: pick a chassis, a weapon and an enemy type,
// then launch Mission.tscn with that selection applied via MissionLoadout.
// This is a plain menu with no grid underneath it, so its Controls keep the
// default mouse_filter (Stop) -- the Ignore rule only applies to overlays
// sitting on top of Mission's grid.
public partial class ConfigScreen : Control
{
	// Parallel to EnemyOptions by index: the enemy's displayed name always
	// comes from GameConstants.CharacterNames, never a literal or a field
	// on EnemyData.
	private static readonly string[] EnemyDisplayNames =
	{
		CharacterNames.TrainingDummy,
		CharacterNames.HeavyDrone,
	};

	[Export] public Godot.Collections.Array<ChassisData> ChassisOptions { get; set; } = new();
	[Export] public Godot.Collections.Array<WeaponData> WeaponOptions { get; set; } = new();
	[Export] public Godot.Collections.Array<EnemyData> EnemyOptions { get; set; } = new();

	private OptionButton _chassisOption;
	private OptionButton _weaponOption;
	private OptionButton _enemyOption;
	private Button _launchButton;

	public override void _Ready()
	{
		_chassisOption = GetNode<OptionButton>("Panel/ChassisOption");
		_weaponOption = GetNode<OptionButton>("Panel/WeaponOption");
		_enemyOption = GetNode<OptionButton>("Panel/EnemyOption");
		_launchButton = GetNode<Button>("Panel/LaunchButton");

		foreach (var chassis in ChassisOptions)
			_chassisOption.AddItem(chassis.DisplayName);
		if (_chassisOption.ItemCount > 0)
			_chassisOption.Select(0);

		foreach (var weapon in WeaponOptions)
			_weaponOption.AddItem(weapon.DisplayName);
		if (_weaponOption.ItemCount > 0)
			_weaponOption.Select(0);

		for (int i = 0; i < EnemyOptions.Count && i < EnemyDisplayNames.Length; i++)
			_enemyOption.AddItem(EnemyDisplayNames[i]);
		if (_enemyOption.ItemCount > 0)
			_enemyOption.Select(0);

		_launchButton.Pressed += OnLaunchPressed;
	}

	private void OnLaunchPressed()
	{
		if (ChassisOptions.Count == 0 || WeaponOptions.Count == 0 || EnemyOptions.Count == 0)
		{
			GD.PrintErr("ConfigScreen is missing Chassis/Weapon/Enemy options in the Inspector.");
			return;
		}

		MissionLoadout.PlayerChassis = ChassisOptions[_chassisOption.Selected];
		MissionLoadout.PlayerWeapon = WeaponOptions[_weaponOption.Selected];

		int enemyIndex = _enemyOption.Selected;
		var enemy = EnemyOptions[enemyIndex];
		MissionLoadout.EnemyChassis = enemy.Chassis;
		MissionLoadout.EnemyWeapon = enemy.Weapon;
		MissionLoadout.EnemyName = EnemyDisplayNames[enemyIndex];

		GetTree().ChangeSceneToFile("res://Scenes/Mission.tscn");
	}
}
