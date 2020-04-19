using System.Collections.Generic;
using CustomDataTypes;
using Puzzles.Maze;
using UnityEngine;

public class MazeController : MonoBehaviour
{
	[SerializeField] private Wall _boxPrefab;
	[SerializeField] private IntPair _startingSize = IntPair.one * 8;
	[SerializeField] private Camera _cam;
	[SerializeField] private GameObject _pointer;
	[SerializeField] private LayerMask _planeMask;
	[SerializeField] private PersonSpawner _spawner;
	[SerializeField] private ErrorMessage _errorMessage;

	private void Start()
	{
		Size = new IntPair(
			Mathf.Max(3, _startingSize.x),
			Mathf.Max(3, _startingSize.y));
		EntryPoint = new IntPair(0, TopBoundary);
		ExitPoint = new IntPair(0, BottomBoundary);

		CreateInitialMaze();
		UpdateSize();
	}

	private void Update()
	{
		//update pointer position
		Vector3 mousePos = Input.mousePosition;
		Ray r = _cam.ScreenPointToRay(mousePos);
		if (Physics.Raycast(r, out RaycastHit hit, Mathf.Infinity, _planeMask))
		{
			Vector3 worldPos = hit.point;
			IntPair mazePos = WorldPositionToMazePoint(worldPos);
			if (IsWithinMaze(mazePos))
			{
				PointerPos = mazePos;
				_pointer.transform.position = MazePointToWorldPosition(mazePos);
			}
		}

		//check for mouse click
		if (Input.GetMouseButtonDown(0) && CursorIsOnPlane() && Time.timeScale > 0f)
		{
			IntPair mazePos = PointerPos;
			if (IsEdgeTile(mazePos)) return;
			if (IsWall(mazePos))
			{
				Wall wall = WallGrid[mazePos];
				wall.Shrink();
				SetWall(mazePos, null);
			}
			else
			{
				if (IsAllowedToBlockOff(mazePos))
				{
					SetWall(mazePos, CreateBox(mazePos));
				}
				else
				{
					_errorMessage.Reveal();
				}
			}
		}
	}

	private void OnDrawGizmos()
	{
		//draw spaces
		Gizmos.color = Color.grey;
		//foreach (IntPair pos in WallGrid.Keys)
		//{
		//	bool isWall = WallGrid[pos];
		//	if (!isWall)
		//	{
		//		Gizmos.DrawSphere(MazePointToWorldPosition(pos), 0.1f);
		//	}
		//}
		//draw occupied coords
		Gizmos.color = Color.white;
		foreach (IntPair coords in OccupiedCoordinates.Keys)
		{
			if (IsOccupied(coords))
			{
				Gizmos.DrawSphere(MazePointToWorldPosition(coords), 0.2f);
			}
		}
	}

	public IntPair EntryPoint { get; private set; }

	public IntPair ExitPoint { get; private set; }

	public IntPair Size { get; private set; }

	public Vector3 MazePointToLocalPosition(IntPair point) => new Vector3(point.x, 0f, point.y);

	private Dictionary<IntPair, HashSet<Person>> OccupiedCoordinates { get; set; } = new Dictionary<IntPair, HashSet<Person>>();

	public Vector3 MazePointToWorldPosition(IntPair point)
	{
		Vector3 localPos = MazePointToLocalPosition(point);
		return transform.TransformPoint(localPos);
	}

	private bool CursorIsOnPlane()
	{
		Vector3 mousePos = Input.mousePosition;
		Ray r = _cam.ScreenPointToRay(mousePos);
		return Physics.Raycast(r, out RaycastHit hit, Mathf.Infinity, _planeMask);
	}

	public IntPair LocalPositionToMazePoint(Vector3 localPos)
	{
		return new IntPair(Mathf.FloorToInt(localPos.x + 0.5f), Mathf.FloorToInt(localPos.z + 0.5f));
	}

	public IntPair WorldPositionToMazePoint(Vector3 position)
	{
		position = transform.InverseTransformPoint(position);
		return new IntPair(Mathf.FloorToInt(position.x + 0.5f), Mathf.FloorToInt(position.z + 0.5f));
	}

	private Dictionary<IntPair, Wall> WallGrid { get; set; } = new Dictionary<IntPair, Wall>();

	private IntPair PointerPos { get; set; }

	private void UpdateSize()
	{
		float xSize = 15f / Size.x;
		float zSize = 9f / Size.y;
		transform.localScale = new Vector3(xSize, 1f, zSize);
	}

	public void RemovePersonFromCoordinates(Person p, IntPair coords)
	{
		if (!IsWithinMaze(coords)) return;

		if (!OccupiedCoordinates.ContainsKey(coords))
		{
			OccupiedCoordinates.Add(coords, new HashSet<Person>());
		}
		OccupiedCoordinates[coords].Remove(p);
	}

	public void AddPersonToCoordinates(Person p, IntPair coords)
	{
		if (!IsWithinMaze(coords)) return;

		if (!OccupiedCoordinates.ContainsKey(coords))
		{
			OccupiedCoordinates.Add(coords, new HashSet<Person>());
		}
		OccupiedCoordinates[coords].Add(p);
	}

	public bool ContainsPersonAtCoordinates(Person p, IntPair coords)
	{
		if (!OccupiedCoordinates.ContainsKey(coords)) return false;
		return OccupiedCoordinates[coords].Contains(p);
	}

	public bool IsWithinMaze(IntPair point)
		=> point.x >= LeftBoundary
		   && point.x <= RightBoundary
		   && point.y >= BottomBoundary
		   && point.y <= TopBoundary;

	public bool IsOpen(IntPair point) => IsWithinMaze(point) && !IsWall(point);

	private void CreateInitialMaze()
	{
		IntPair pos = new IntPair();
		Generator gen = new Generator();
		IntPair[] exits = new IntPair[]
		{
			EntryPoint - Offset,
			ExitPoint - Offset
		};
		GridMatrix grid = gen.GeneratePuzzle(Size, exits, 1);

		for (int i = 0; i < grid.ArrayLength(); i++)
		{
			bool isWall = grid.Get(i);
			pos = grid.GetPos(i);
			IntPair offsetPos = pos + Offset;
			Wall wall = isWall ? CreateBox(offsetPos) : null;
			SetWall(offsetPos, wall);
		}
	}

	private IntPair Offset => new IntPair(LeftBoundary, BottomBoundary);

	private Wall CreateBox(IntPair pos)
	{
		Wall box = Instantiate(_boxPrefab, transform);
		box.transform.localPosition = MazePointToLocalPosition(pos);
		return box;
	}

	private int LeftBoundary => -Size.x / 2;

	private int RightBoundary => Size.x / 2;

	private int BottomBoundary => -Size.y / 2;

	private int TopBoundary => Size.y / 2;

	public bool IsWall(IntPair pos)
	{
		if (WallGrid == null || !WallGrid.ContainsKey(pos)) return false;
		return WallGrid[pos] != null;
	}

	private bool IsEdgeTile(IntPair point)
		=> point.x == LeftBoundary
		   || point.x == RightBoundary
		   || point.y == BottomBoundary
		   || point.y == TopBoundary;

	private void SetWall(IntPair pos, Wall wall)
	{
		if (WallGrid == null) return;

		if (WallGrid.ContainsKey(pos))
		{
			WallGrid[pos] = wall;
		}
		else
		{
			WallGrid.Add(pos, wall);
		}
	}

	private bool IsAllowedToBlockOff(IntPair point)
	{
		if (IsOccupied(point)) return false;

		IntPair up = point + IntPair.up;
		IntPair down = point + IntPair.down;
		IntPair left = point + IntPair.left;
		IntPair right = point + IntPair.right;

		IntPair temp = up;
		HashSet<IntPair> space = GetSpaceContainingPoint(temp, point);
		bool spaceExists = space != null;
		if (spaceExists)
		{
			bool spaceContainsEntryPoint = space.Contains(EntryPoint);
			bool spaceContainsExitPoint = space.Contains(ExitPoint);
			bool spaceContainsOccupants = SetContainsOccupiedCoordinates(space);
			//a space must have both exit/entry or neither
			if (spaceContainsEntryPoint != spaceContainsExitPoint) return false;
			//can't block off exit from people
			if ((!spaceContainsExitPoint || !spaceContainsEntryPoint) && spaceContainsOccupants) return false;
		}

		temp = down;
		space = GetSpaceContainingPoint(temp, point);
		spaceExists = space != null;
		if (spaceExists)
		{
			bool spaceContainsEntryPoint = space.Contains(EntryPoint);
			bool spaceContainsExitPoint = space.Contains(ExitPoint);
			bool spaceContainsOccupants = SetContainsOccupiedCoordinates(space);
			//a space must have both exit/entry or neither
			if (spaceContainsEntryPoint != spaceContainsExitPoint) return false;
			//can't block off exit from people
			if ((!spaceContainsExitPoint || !spaceContainsEntryPoint) && spaceContainsOccupants) return false;
		}

		temp = left;
		space = GetSpaceContainingPoint(temp, point);
		spaceExists = space != null;
		if (spaceExists)
		{
			bool spaceContainsEntryPoint = space.Contains(EntryPoint);
			bool spaceContainsExitPoint = space.Contains(ExitPoint);
			bool spaceContainsOccupants = SetContainsOccupiedCoordinates(space);
			//a space must have both exit/entry or neither
			if (spaceContainsEntryPoint != spaceContainsExitPoint) return false;
			//can't block off exit from people
			if ((!spaceContainsExitPoint || !spaceContainsEntryPoint) && spaceContainsOccupants) return false;
		}

		temp = right;
		space = GetSpaceContainingPoint(temp, point);
		spaceExists = space != null;
		if (spaceExists)
		{
			bool spaceContainsEntryPoint = space.Contains(EntryPoint);
			bool spaceContainsExitPoint = space.Contains(ExitPoint);
			bool spaceContainsOccupants = SetContainsOccupiedCoordinates(space);
			//a space must have both exit/entry or neither
			if (spaceContainsEntryPoint != spaceContainsExitPoint) return false;
			//can't block off exit from people
			if ((!spaceContainsExitPoint || !spaceContainsEntryPoint) && spaceContainsOccupants) return false;
		}

		return true;
	}

	private bool SetContainsOccupiedCoordinates(HashSet<IntPair> set)
	{
		foreach (IntPair coord in OccupiedCoordinates.Keys)
		{
			bool isOccupied = IsOccupied(coord);
			if (!isOccupied) continue;
			if (set.Contains(coord)) return true;
		}

		return false;
	}

	private bool IsOccupied(IntPair coord)
	{
		if (!OccupiedCoordinates.ContainsKey(coord)) return false;
		return OccupiedCoordinates[coord].Count > 0;
	}

	private HashSet<IntPair> GetSpaceContainingPoint(IntPair point, IntPair ignore)
	{
		if (!IsOpen(point)) return null;

		HashSet<IntPair> space = new HashSet<IntPair>();
		space.Add(point);
		AddOpenNeighbours(space, point, true, ignore);
		return space;
	}

	private void AddOpenNeighbours(HashSet<IntPair> set, IntPair point, bool recursive, IntPair ignore)
	{
		IntPair up = point + IntPair.up;
		IntPair down = point + IntPair.down;
		IntPair left = point + IntPair.left;
		IntPair right = point + IntPair.right;

		IntPair temp = up;
		if (!set.Contains(temp) && IsOpen(temp) && temp != ignore)
		{
			set.Add(temp);
			if (recursive) AddOpenNeighbours(set, temp, true, ignore);
		}

		temp = down;
		if (!set.Contains(temp) && IsOpen(temp) && temp != ignore)
		{
			set.Add(temp);
			if (recursive) AddOpenNeighbours(set, temp, true, ignore);
		}

		temp = left;
		if (!set.Contains(temp) && IsOpen(temp) && temp != ignore)
		{
			set.Add(temp);
			if (recursive) AddOpenNeighbours(set, temp, true, ignore);
		}

		temp = right;
		if (!set.Contains(temp) && IsOpen(temp) && temp != ignore)
		{
			set.Add(temp);
			if (recursive) AddOpenNeighbours(set, temp, true, ignore);
		}
	}
}
