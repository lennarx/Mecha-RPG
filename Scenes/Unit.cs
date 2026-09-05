using Godot;

// Thin visual wrapper around a combatant. Knows nothing about the grid,
// selection or turn logic -- Mission.cs owns that. Only tracks world
// placement, its selection highlight and its own UnitState (assigned by
// Mission.cs at spawn time, same pattern as AttackTest.cs).
public partial class Unit : Node2D
{
    [Export] public WeaponData Weapon { get; set; }

    public UnitState State { get; set; }
    public Vector2I Cell { get; private set; }

    private const float SelectionHalfSize = 32f;
    private const float SelectionOutlineWidth = 4f;

    private static readonly Color SelectionColor = new(1f, 0.9f, 0.25f);

    private ColorRect _visual;
    private Label _hpLabel;
    private Label _heatLabel;
    private bool _isSelected;

    public override void _Ready()
    {
        _visual = GetNode<ColorRect>("Visual");
        _hpLabel = GetNode<Label>("HpLabel");
        _heatLabel = GetNode<Label>("HeatLabel");
        RefreshHpLabel();
        RefreshHeatLabel();
    }

    public void SetColor(Color color)
    {
        _visual.Color = color;
    }

    // Draws a bright outline around the unit's placeholder rect so the
    // player can tell at a glance which unit the click loop has selected.
    public void SetSelected(bool selected)
    {
        if (_isSelected == selected)
            return;

        _isSelected = selected;
        QueueRedraw();
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

    public void RefreshHeatLabel()
    {
        if (_heatLabel != null && State != null)
            _heatLabel.Text = $"HEAT: {State.Tension}/{CombatConstants.MaxTension} ({Combat.GetHeatBand(State.Tension)})";
    }

    public override void _Draw()
    {
        if (!_isSelected)
            return;

        var topLeft = new Vector2(-SelectionHalfSize, -SelectionHalfSize);
        var size = new Vector2(SelectionHalfSize, SelectionHalfSize) * 2f;
        DrawRect(new Rect2(topLeft, size), SelectionColor, filled: false, width: SelectionOutlineWidth);
    }
}
