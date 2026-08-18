using Godot;
using System.Collections.Generic;

// Mission orchestrator: the only node that knows about the grid,
// AStarGrid2D, selection state and turn flow. Unit.cs stays a dumb visual
// wrapper; Resources/*.cs stays pure C# combat logic. This mirrors
// AttackTest.cs's role (drive UnitState + WeaponData) but interactively.
public partial class Mission : Node2D
{
    private const int GridWidth = 8;
    private const int GridHeight = 8;
    private const int TileSize = 64;

    private static readonly Vector2I[] MoveDirections =
    {
        Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right,
    };

    private TileMapLayer _grid;
    private GridHighlight _highlight;
    private Unit _playerUnit;
    private Unit _enemyUnit;
    private Label _victoryLabel;
    private AStarGrid2D _astar;

    private Unit _selected;
    private HashSet<Vector2I> _reachableCells = new();

    public override void _Ready()
    {
        _grid = GetNode<TileMapLayer>("Grid");
        _highlight = GetNode<GridHighlight>("Highlight");
        _playerUnit = GetNode<Unit>("Units/PlayerUnit");
        _enemyUnit = GetNode<Unit>("Units/EnemyUnit");
        _victoryLabel = GetNode<Label>("VictoryLabel");

        BuildTileSet();
        BuildAstarGrid();

        _highlight.Grid = _grid;
        _highlight.TileSize = TileSize;

        _playerUnit.State = new UnitState { Name = CharacterNames.Protagonist };
        _playerUnit.SetColor(new Color(0.25f, 0.5f, 1f));
        PlaceUnit(_playerUnit, new Vector2I(1, 6));

        _enemyUnit.State = new UnitState { Name = CharacterNames.TrainingDummy, Armor = ArmorType.Light };
        _enemyUnit.SetColor(new Color(1f, 0.3f, 0.3f));
        PlaceUnit(_enemyUnit, new Vector2I(6, 1));

        _playerUnit.RefreshHpLabel();
        _enemyUnit.RefreshHpLabel();

        _victoryLabel.Visible = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
            HandleCellClick(_grid.LocalToMap(_grid.GetLocalMousePosition()));
    }

    private void BuildTileSet()
    {
        var image = Image.CreateEmpty(TileSize, TileSize, false, Image.Format.Rgba8);
        image.Fill(new Color(0.18f, 0.2f, 0.24f));
        var texture = ImageTexture.CreateFromImage(image);

        var source = new TileSetAtlasSource
        {
            Texture = texture,
            TextureRegionSize = new Vector2I(TileSize, TileSize),
        };
        source.CreateTile(Vector2I.Zero);

        var tileSet = new TileSet { TileSize = new Vector2I(TileSize, TileSize) };
        tileSet.AddSource(source, 0);
        _grid.TileSet = tileSet;

        for (int x = 0; x < GridWidth; x++)
            for (int y = 0; y < GridHeight; y++)
                _grid.SetCell(new Vector2I(x, y), 0, Vector2I.Zero);
    }

    private void BuildAstarGrid()
    {
        _astar = new AStarGrid2D
        {
            Region = new Rect2I(0, 0, GridWidth, GridHeight),
            CellSize = new Vector2(TileSize, TileSize),
        };
        _astar.Update();
    }

    private void PlaceUnit(Unit unit, Vector2I cell)
    {
        unit.PlaceAt(cell, _grid.MapToLocal(cell));
    }

    private void HandleCellClick(Vector2I cell)
    {
        if (_selected == null)
        {
            if (cell == _playerUnit.Cell)
                SelectUnit(_playerUnit);
            return;
        }

        if (cell == _selected.Cell)
        {
            Deselect();
            return;
        }

        if (cell == _enemyUnit.Cell)
        {
            TryAttack();
            return;
        }

        if (_reachableCells.Contains(cell))
        {
            PlaceUnit(_selected, cell);
            Deselect();
        }
    }

    private void SelectUnit(Unit unit)
    {
        _selected = unit;
        _reachableCells = ComputeReachableCells(unit.Cell, unit.State.MoveRange);
        _highlight.SetCells(_reachableCells);
    }

    private void Deselect()
    {
        _selected = null;
        _reachableCells = new HashSet<Vector2I>();
        _highlight.SetCells(_reachableCells);
    }

    private void TryAttack()
    {
        if (_selected.Weapon == null)
        {
            GD.PrintErr($"{_selected.State.Name} has no Weapon assigned in the Inspector.");
            return;
        }

        int distance = ChebyshevDistance(_selected.Cell, _enemyUnit.Cell);
        if (distance > _selected.Weapon.Range)
        {
            GD.Print($"{_selected.State.Name} is out of range ({distance} > {_selected.Weapon.Range}).");
            return;
        }

        var log = new List<string>();
        _selected.Weapon.ResolveAttack(_selected.State, _enemyUnit.State, log);
        foreach (var line in log)
            GD.Print(line);

        _enemyUnit.RefreshHpLabel();
        Deselect();

        if (_enemyUnit.State.Hp <= 0)
            ShowVictory();
    }

    private void ShowVictory()
    {
        _victoryLabel.Text = $"VICTORY -- {_enemyUnit.State.Name} destroyed.";
        _victoryLabel.Visible = true;
    }

    // Manhattan-distance BFS bounded by MoveRange, consulting AStarGrid2D
    // for bounds/solid cells so obstacles can be added later without
    // touching this method.
    private HashSet<Vector2I> ComputeReachableCells(Vector2I origin, int moveRange)
    {
        var visited = new Dictionary<Vector2I, int> { [origin] = 0 };
        var frontier = new Queue<Vector2I>();
        frontier.Enqueue(origin);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            int distance = visited[current];
            if (distance >= moveRange)
                continue;

            foreach (var direction in MoveDirections)
            {
                var next = current + direction;
                if (!_astar.IsInBoundsv(next)) continue;
                if (_astar.IsPointSolid(next)) continue;
                if (visited.ContainsKey(next)) continue;
                if (next == _enemyUnit.Cell) continue;

                visited[next] = distance + 1;
                frontier.Enqueue(next);
            }
        }

        visited.Remove(origin);
        return new HashSet<Vector2I>(visited.Keys);
    }

    private static int ChebyshevDistance(Vector2I a, Vector2I b)
    {
        return Mathf.Max(Mathf.Abs(a.X - b.X), Mathf.Abs(a.Y - b.Y));
    }
}
