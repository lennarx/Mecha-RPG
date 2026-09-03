using Godot;
using System.Collections.Generic;

// Paints a translucent overlay plus an outline over the cells Mission.cs
// reports as reachable. Owns no state of its own beyond what it's told to draw.
public partial class GridHighlight : Node2D
{
    public TileMapLayer Grid;
    public int TileSize = 64;

    private const float OutlineWidth = 3f;
    private const float OutlineInset = 2f;

    private static readonly Color FillColor = new(0.25f, 0.95f, 0.45f, 0.28f);
    private static readonly Color OutlineColor = new(0.4f, 1f, 0.55f, 0.95f);

    private IReadOnlyCollection<Vector2I> _cells = System.Array.Empty<Vector2I>();

    public void SetCells(IReadOnlyCollection<Vector2I> cells)
    {
        _cells = cells;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var tile = new Vector2(TileSize, TileSize);
        var halfTile = tile / 2f;
        var outlineSize = tile - new Vector2(OutlineInset, OutlineInset) * 2f;

        foreach (var cell in _cells)
        {
            var topLeft = Grid.MapToLocal(cell) - halfTile;
            DrawRect(new Rect2(topLeft, tile), FillColor);
            DrawRect(
                new Rect2(topLeft + new Vector2(OutlineInset, OutlineInset), outlineSize),
                OutlineColor,
                filled: false,
                width: OutlineWidth);
        }
    }
}
