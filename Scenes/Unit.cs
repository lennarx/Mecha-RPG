using Godot;

// Thin visual wrapper around a combatant. Knows nothing about the grid,
// selection or turn logic -- Mission.cs owns that. Only tracks world
// placement, its selection highlight and its own UnitState (assigned by
// Mission.cs at spawn time, same pattern as AttackTest.cs).
public partial class Unit : Node2D
{
    [Export] public ChassisData Chassis { get; set; }
    [Export] public WeaponData Weapon { get; set; }

    public UnitState State { get; set; }
    public Vector2I Cell { get; private set; }

    // Set by Mission.cs right after State is built, from the chassis's Hp.
    // UnitState only tracks current Hp, not a max, so the bar's denominator
    // has to live here in the presentation layer instead.
    public int MaxHp { get; set; }

    private const float SelectionHalfSize = 32f;
    private const float SelectionOutlineWidth = 4f;

    // Status bar layout (presentation-only geometry, not balance -- these
    // stay local consts instead of CombatConstants).
    private const float BarWidth = 48f;
    private const float BarHeight = 6f;
    private const float HpBarOffsetY = -46f;
    private const float HeatBarOffsetY = -56f;

    private const float OverloadBadgeOffsetY = -74f;
    private const float OverloadBadgeSize = 9f;

    private const float FloatingDamageRiseDistance = 40f;
    private const float FloatingDamageDurationSeconds = 0.9f;

    private static readonly Color SelectionColor = new(1f, 0.9f, 0.25f);
    private static readonly Color OverloadBadgeColor = new(0.9f, 0.15f, 0.15f);
    private static readonly Color OverloadBadgeMarkColor = Colors.White;
    private static readonly Color FloatingDamageColor = new(1f, 0.85f, 0.2f);

    private ColorRect _visual;
    private ColorRect _hpBarBg;
    private ColorRect _hpBarFill;
    private ColorRect _heatBarBg;
    private ColorRect _heatBarFill;
    private bool _isSelected;
    private bool _overloaded;

    public override void _Ready()
    {
        _visual = GetNode<ColorRect>("Visual");
        _hpBarBg = GetNode<ColorRect>("HpBarBg");
        _hpBarFill = GetNode<ColorRect>("HpBarFill");
        _heatBarBg = GetNode<ColorRect>("HeatBarBg");
        _heatBarFill = GetNode<ColorRect>("HeatBarFill");

        LayOutBar(_hpBarBg, _hpBarFill, HpBarOffsetY);
        LayOutBar(_heatBarBg, _heatBarFill, HeatBarOffsetY);

        RefreshHpBar();
        RefreshHeatBar();
    }

    private static void LayOutBar(ColorRect bg, ColorRect fill, float offsetY)
    {
        var topLeft = new Vector2(-BarWidth / 2f, offsetY);
        bg.Position = topLeft;
        bg.Size = new Vector2(BarWidth, BarHeight);
        fill.Position = topLeft;
        fill.Size = new Vector2(BarWidth, BarHeight);
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

    public void RefreshHpBar()
    {
        if (_hpBarFill == null || State == null || MaxHp <= 0)
            return;

        float ratio = Mathf.Clamp((float)State.Hp / MaxHp, 0f, 1f);
        _hpBarFill.Size = new Vector2(BarWidth * ratio, BarHeight);
    }

    public void RefreshHeatBar()
    {
        if (_heatBarFill == null || State == null)
            return;

        float ratio = (float)State.Tension / CombatConstants.MaxTension;
        _heatBarFill.Size = new Vector2(BarWidth * ratio, BarHeight);
        _heatBarFill.Color = HeatBandColor(Combat.GetHeatBand(State.Tension));
    }

    // The single band-to-color mapping. Every heat-band visual (bar fill,
    // future indicators) reads through here instead of re-deriving its own
    // switch on HeatBand.
    private static Color HeatBandColor(HeatBand band) => band switch
    {
        HeatBand.Cold => new Color(0.55f, 0.6f, 0.65f),
        HeatBand.Optimal => new Color(0.3f, 0.85f, 0.35f),
        HeatBand.Critical => new Color(1f, 0.6f, 0.15f),
        HeatBand.Overload => new Color(0.9f, 0.15f, 0.15f),
        _ => Colors.White,
    };

    // Mission.cs drives this from UnitState.IsOverloaded at the two moments
    // that actually matter -- turn start (is this unit locked right now?)
    // and post-dissipation (did it just cool off enough to unlock?). Unit
    // itself makes no overload/band decision, it only shows what it's told.
    public void SetOverloaded(bool overloaded)
    {
        if (_overloaded == overloaded)
            return;

        _overloaded = overloaded;
        QueueRedraw();
    }

    // Spawns a rising, fading number over the unit. Mission.cs computes the
    // actual damage (Hp before/after ResolveAttack) and hands it over --
    // Unit only displays the number, it never touches Combat/DamageEffect.
    public void ShowFloatingDamage(int amount)
    {
        var label = new Label
        {
            Text = amount.ToString(),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Position = new Vector2(-BarWidth / 2f, HeatBarOffsetY - 14f),
            ZIndex = 10,
        };
        label.AddThemeColorOverride("font_color", FloatingDamageColor);
        label.AddThemeFontSizeOverride("font_size", 20);
        AddChild(label);

        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(label, "position:y", label.Position.Y - FloatingDamageRiseDistance, FloatingDamageDurationSeconds);
        tween.TweenProperty(label, "modulate:a", 0f, FloatingDamageDurationSeconds);
        tween.Chain().TweenCallback(Callable.From(label.QueueFree));
    }

    public override void _Draw()
    {
        if (_isSelected)
        {
            var topLeft = new Vector2(-SelectionHalfSize, -SelectionHalfSize);
            var size = new Vector2(SelectionHalfSize, SelectionHalfSize) * 2f;
            DrawRect(new Rect2(topLeft, size), SelectionColor, filled: false, width: SelectionOutlineWidth);
        }

        if (_overloaded)
            DrawOverloadBadge();
    }

    // A warning triangle with an "!" mark, all drawn from primitives --
    // no image assets. Sits above the heat bar.
    private void DrawOverloadBadge()
    {
        var center = new Vector2(0f, OverloadBadgeOffsetY);
        float h = OverloadBadgeSize;

        var top = center + new Vector2(0f, -h);
        var bottomRight = center + new Vector2(h * 0.87f, h * 0.5f);
        var bottomLeft = center + new Vector2(-h * 0.87f, h * 0.5f);
        DrawColoredPolygon(new[] { top, bottomRight, bottomLeft }, OverloadBadgeColor);

        DrawLine(center + new Vector2(0f, -h * 0.45f), center + new Vector2(0f, h * 0.05f), OverloadBadgeMarkColor, 2f);
        DrawCircle(center + new Vector2(0f, h * 0.3f), 1.3f, OverloadBadgeMarkColor);
    }
}
