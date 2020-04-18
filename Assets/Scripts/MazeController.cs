using System.Collections.Generic;
using CustomDataTypes;
using Puzzles.Maze;
using UnityEngine;

public class MazeController : MonoBehaviour
{
	[SerializeField] private GameObject _boxPrefab;
	[SerializeField] private IntPair startingSize = IntPair.one * 8;

	public IntPair EntryPoint { get; private set; }

	public IntPair ExitPoint { get; private set; }

	public IntPair Size { get; private set; }

	private Dictionary<IntPair, bool> WallGrid { get; set; } = new Dictionary<IntPair, bool>();

	private void Start()
	{
		Size = new IntPair(
			Mathf.Max(3, startingSize.x),
			Mathf.Max(3, startingSize.y));
		EntryPoint = new IntPair(0, TopBoundary);
		ExitPoint = new IntPair(0, BottomBoundary);

		CreateInitialMaze();
		UpdateSize();
	}

	private void UpdateSize()
	{
		float xSize = 15f / Size.x;
		float zSize = 9f / Size.y;
		transform.localScale = new Vector3(xSize, 1f, zSize);
	}

	private void CreateInitialMaze()
	{
		int horizontalStart = LeftBoundary;
		int verticalStart = BottomBoundary;
		IntPair pos = new IntPair();
		Generator gen = new Generator();
		IntPair offset = new IntPair(horizontalStart, verticalStart);
		IntPair[] exits = new IntPair[]
		{
			EntryPoint - offset,
			ExitPoint - offset
		};
		GridMatrix grid = gen.GeneratePuzzle(Size, exits, 1);

		for (int i = 0; i < grid.ArrayLength(); i++)
		{
			bool isWall = grid.Get(i);
			if (!isWall) continue;
			pos = grid.GetPos(i);
			CreateBox(pos.x + offset.x, pos.y + offset.y);
		}
	}

	private void CreateBox(int x, int y)
	{
		GameObject box = Instantiate(_boxPrefab, transform);
		box.transform.localPosition = new Vector3(x, 0, y);
	}

	private int LeftBoundary => -Size.x / 2;

	private int RightBoundary => Size.x / 2;

	private int BottomBoundary => -Size.y / 2;

	private int TopBoundary => Size.y / 2;

	private bool IsWall(IntPair pos)
	{
		if (WallGrid == null || !WallGrid.ContainsKey(pos)) return false;
		return WallGrid[pos];
	}

	private void SetWall(IntPair pos, bool create)
	{
		if (WallGrid == null) return;

		if (WallGrid.ContainsKey(pos))
		{
			WallGrid[pos] = create;
		}
		else
		{
			WallGrid.Add(pos, create);
		}
	}
}
