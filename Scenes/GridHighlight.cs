using Godot;
using System.Collections.Generic;

// Paints a translucent overlay over the cells Mission.cs reports as
// reachable. Owns no state of its own beyond what it's told to draw.
public partial class GridHighlight : Node2D
{
    public TileMapLayer Grid;
    public int TileSize = 64;

    private static readonly Color HighlightColor = new(0.2f, 0.85f, 0.35f, 0.35f);

    private IReadOnlyCollection<Vector2I> _cells = System.Array.Empty<Vector2I>();

    public void SetCells(IReadOnlyCollection<Vector2I> cells)
    {
        _cells = cells;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var halfTile = new Vector2(TileSize, TileSize) / 2f;
        foreach (var cell in _cells)
        {
            var center = Grid.MapToLocal(cell);
            DrawRect(new Rect2(center - halfTile, new Vector2(TileSize, TileSize)), HighlightColor);
        }
    }
}
