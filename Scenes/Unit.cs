using Godot;

// Thin visual wrapper around a combatant. Knows nothing about the grid,
// selection or turn logic -- Mission.cs owns that. Only tracks world
// placement and its own UnitState (assigned by Mission.cs at spawn time,
// same pattern as AttackTest.cs).
public partial class Unit : Node2D
{
    [Export] public WeaponData Weapon { get; set; }

    public UnitState State { get; set; }
    public Vector2I Cell { get; private set; }

    private ColorRect _visual;
    private Label _hpLabel;

    public override void _Ready()
    {
        _visual = GetNode<ColorRect>("Visual");
        _hpLabel = GetNode<Label>("HpLabel");
        RefreshHpLabel();
    }

    public void SetColor(Color color)
    {
        _visual.Color = color;
    }

    public void PlaceAt(Vector2I cell, Vector2 worldPosition)
    {
        Cell = cell;
        Position = worldPosition;
    }

    public void RefreshHpLabel()
    {
        if (_hpLabel != null && State != null)
            _hpLabel.Text = State.Hp.ToString();
    }
}
