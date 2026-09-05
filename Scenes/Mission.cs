using Godot;
using System.Collections.Generic;

// Mission orchestrator: the only node that knows about the grid,
// AStarGrid2D, selection state and turn flow. Unit.cs stays a dumb visual
// wrapper; Resources/*.cs stays pure C# combat logic. This mirrors
// AttackTest.cs's role (drive UnitState + WeaponData) but interactively.
public partial class Mission : Node2D
{
	private enum TurnSide { Player, Enemy }

	private const int GridWidth = 8;
	private const int GridHeight = 8;
	private const int TileSize = 64;
	private const int TileBorderWidth = 2;

	// Presentation pacing, not balance -- CombatConstants stays free of this.
	private const float EnemyThinkDelaySeconds = 0.45f;
	private const float MessageDisplaySeconds = 2.5f;

	private static readonly Color TileFillColor = new(0.18f, 0.2f, 0.24f);
	private static readonly Color TileBorderColor = new(0.42f, 0.47f, 0.55f);

	private static readonly Vector2I[] MoveDirections =
	{
		Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right,
	};

	private TileMapLayer _grid;
	private GridHighlight _highlight;
	private Unit _playerUnit;
	private Unit _enemyUnit;
	private Label _victoryLabel;
	private Label _turnLabel;
	private Label _defeatLabel;
	private Label _messageLabel;
	private AStarGrid2D _astar;

	private Unit _selected;
	private HashSet<Vector2I> _reachableCells = new();

	private TurnSide _turn = TurnSide.Player;
	private bool _missionOver;
	private int _messageToken;

	public override void _Ready()
	{
		_grid = GetNode<TileMapLayer>("Grid");
		_highlight = GetNode<GridHighlight>("Highlight");
		_playerUnit = GetNode<Unit>("Units/PlayerUnit");
		_enemyUnit = GetNode<Unit>("Units/EnemyUnit");
		_victoryLabel = GetNode<Label>("VictoryLabel");
		_turnLabel = GetNode<Label>("TurnLabel");
		_defeatLabel = GetNode<Label>("DefeatLabel");
		_messageLabel = GetNode<Label>("MessageLabel");

		BuildTileSet();
		BuildAstarGrid();

		_highlight.Grid = _grid;
		_highlight.TileSize = TileSize;

		_playerUnit.State = new UnitState { Name = CharacterNames.Protagonist };
		_playerUnit.SetColor(new Color(0.25f, 0.5f, 1f));
		PlaceUnit(_playerUnit, new Vector2I(1, 6));

		_enemyUnit.State = new UnitState { Name = CharacterNames.TrainingDummy, Armor = ArmorType.Light, Hp = CombatConstants.TrainingDummyHp };
		_enemyUnit.SetColor(new Color(1f, 0.3f, 0.3f));
		PlaceUnit(_enemyUnit, new Vector2I(6, 1));

		_playerUnit.RefreshHpLabel();
		_enemyUnit.RefreshHpLabel();
		_playerUnit.RefreshHeatLabel();
		_enemyUnit.RefreshHeatLabel();

		_victoryLabel.Visible = false;

		StartTurn(TurnSide.Player);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_missionOver || _turn != TurnSide.Player)
			return;

		if (@event.IsActionPressed("ui_accept"))
		{
			EndTurn();
			return;
		}

		if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
			HandleCellClick(_grid.LocalToMap(_grid.GetLocalMousePosition()));
	}

	private void BuildTileSet()
	{
		// The tile texture is drawn as a lighter border rectangle with the
		// fill color painted inside it, so adjacent tiles read as a grid
		// instead of one solid slab. Replaced by real art later.
		var image = Image.CreateEmpty(TileSize, TileSize, false, Image.Format.Rgba8);
		image.Fill(TileBorderColor);
		image.FillRect(
			new Rect2I(TileBorderWidth, TileBorderWidth, TileSize - TileBorderWidth * 2, TileSize - TileBorderWidth * 2),
			TileFillColor);
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
			DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never,
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
			_selected.State.CanMove = false;
			_selected.State.Tension += CombatConstants.MoveHeatCost;
			_selected.RefreshHeatLabel();
			Deselect();
			UpdateTurnLabel();
			EndTurnIfPlayerDone();
			return;
		}

		if (!_selected.State.CanMove)
			ShowMessage($"{_selected.State.Name} already used its move this turn.");
	}

	private void SelectUnit(Unit unit)
	{
		_selected = unit;
		unit.SetSelected(true);
		_reachableCells = unit.State.CanMove
			? ComputeReachableCells(unit.Cell, unit.State.MoveRange, _enemyUnit.Cell)
			: new HashSet<Vector2I>();
		_highlight.SetCells(_reachableCells);
	}

	private void Deselect()
	{
		_selected?.SetSelected(false);
		_selected = null;
		_reachableCells = new HashSet<Vector2I>();
		_highlight.SetCells(_reachableCells);
	}

	private void TryAttack()
	{
		if (!_selected.State.CanAttack)
		{
			GD.Print($"{_selected.State.Name} already used its attack this turn.");
			ShowMessage($"{_selected.State.Name} already used its attack this turn.");
			return;
		}

		if (_selected.Weapon == null)
		{
			GD.PrintErr($"{_selected.State.Name} has no Weapon assigned in the Inspector.");
			return;
		}

		int distance = ManhattanDistance(_selected.Cell, _enemyUnit.Cell);
		if (distance > _selected.Weapon.Range)
		{
			GD.Print($"{_selected.State.Name} is out of range ({distance} > {_selected.Weapon.Range}).");
			ShowMessage($"Target out of range ({distance} > {_selected.Weapon.Range}).");
			return;
		}

		var log = new List<string>();
		_selected.Weapon.ResolveAttack(_selected.State, _enemyUnit.State, log);
		foreach (var line in log)
			GD.Print(line);

		_enemyUnit.RefreshHpLabel();
		_selected.RefreshHeatLabel();
		_selected.State.CanAttack = false;
		Deselect();

		if (_enemyUnit.State.Hp <= 0)
		{
			ShowVictory();
			return;
		}

		UpdateTurnLabel();
		EndTurnIfPlayerDone();
	}

	private void EndTurnIfPlayerDone()
	{
		if (!_playerUnit.State.HasActionsLeft)
			EndTurn();
	}

	private void StartTurn(TurnSide side)
	{
		_turn = side;
		var unit = side == TurnSide.Player ? _playerUnit : _enemyUnit;
		unit.State.BeginTurn();
		UpdateTurnLabel();

		if (side == TurnSide.Player)
			ClearMessage();

		if (unit.State.IsOverloaded)
			ShowMessage($"{unit.State.Name} is overloaded -- locked for this turn.");

		if (side == TurnSide.Enemy)
		{
			if (unit.State.IsOverloaded)
				EndTurn();
			else
				RunEnemyTurnAsync();
		}
	}

	// Shows a transient hint for invalid actions (attack/move already used,
	// target out of range). Cleared at the start of the player's turn or
	// after MessageDisplaySeconds, whichever comes first. The token guards
	// against a stale timer clearing a message shown after it was scheduled.
	private async void ShowMessage(string text)
	{
		_messageLabel.Text = text;
		int token = ++_messageToken;

		await ToSignal(GetTree().CreateTimer(MessageDisplaySeconds), SceneTreeTimer.SignalName.Timeout);

		if (token == _messageToken)
			ClearMessage();
	}

	private void ClearMessage()
	{
		_messageLabel.Text = "";
		_messageToken++;
	}

	private void EndTurn()
	{
		if (_missionOver)
			return;

		Deselect();
		var endingUnit = _turn == TurnSide.Player ? _playerUnit : _enemyUnit;
		endingUnit.State.DissipateHeat();
		StartTurn(_turn == TurnSide.Player ? TurnSide.Enemy : TurnSide.Player);
	}

	private void UpdateTurnLabel()
	{
		if (_turn == TurnSide.Enemy)
		{
			_turnLabel.Text = "ENEMY TURN";
			return;
		}

		string move = _playerUnit.State.CanMove ? "ok" : "used";
		string attack = _playerUnit.State.CanAttack ? "ok" : "used";
		_turnLabel.Text = $"YOUR TURN -- move: {move} / attack: {attack}  [Space] end turn";
	}

	private async void RunEnemyTurnAsync()
	{
		await ToSignal(GetTree().CreateTimer(EnemyThinkDelaySeconds), SceneTreeTimer.SignalName.Timeout);

		if (_missionOver)
			return;

		var enemyCell = _enemyUnit.Cell;
		var playerCell = _playerUnit.Cell;

		if (ManhattanDistance(enemyCell, playerCell) <= _enemyUnit.Weapon.Range)
		{
			var log = new List<string>();
			_enemyUnit.Weapon.ResolveAttack(_enemyUnit.State, _playerUnit.State, log);
			foreach (var line in log)
				GD.Print(line);

			_playerUnit.RefreshHpLabel();
			_enemyUnit.RefreshHeatLabel();
			_enemyUnit.State.CanAttack = false;

			if (_playerUnit.State.Hp <= 0)
			{
				ShowDefeat();
				return;
			}
		}
		else
		{
			var idPath = _astar.GetIdPath(enemyCell, playerCell);
			if (idPath.Count > 1)
			{
				var path = new List<Vector2I>(idPath);
				path.RemoveAt(path.Count - 1); // drop the player's cell, it can't be occupied
				int steps = Mathf.Min(_enemyUnit.State.MoveRange, path.Count - 1);
				PlaceUnit(_enemyUnit, path[steps]);
				_enemyUnit.State.Tension += CombatConstants.MoveHeatCost;
				_enemyUnit.RefreshHeatLabel();
			}

			_enemyUnit.State.CanMove = false;
		}

		EndTurn();
	}

	private void ShowVictory()
	{
		_victoryLabel.Text = $"VICTORY -- {_enemyUnit.State.Name} destroyed.";
		_victoryLabel.Visible = true;
		_missionOver = true;
		_turnLabel.Visible = false;
	}

	private void ShowDefeat()
	{
		_defeatLabel.Text = $"DEFEAT -- {_playerUnit.State.Name} destroyed.";
		_defeatLabel.Visible = true;
		_missionOver = true;
		_turnLabel.Visible = false;
	}

	// Manhattan-distance BFS bounded by MoveRange, consulting AStarGrid2D
	// for bounds/solid cells so obstacles can be added later without
	// touching this method.
	private HashSet<Vector2I> ComputeReachableCells(Vector2I origin, int moveRange, Vector2I blockedCell)
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
				if (next == blockedCell) continue;

				visited[next] = distance + 1;
				frontier.Enqueue(next);
			}
		}

		visited.Remove(origin);
		return new HashSet<Vector2I>(visited.Keys);
	}

	private static int ManhattanDistance(Vector2I a, Vector2I b)
	{
		return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
	}
}
