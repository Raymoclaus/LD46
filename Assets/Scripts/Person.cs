using System.Collections.Generic;
using CustomDataTypes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Person : MonoBehaviour
{
	private enum State
	{
		WaitingForMazeCreation,
		MovingToEntryPoint,
		Exploring,
		Retracing,
		MovingPastExit
	}

	[SerializeField] private float _baseSpeed = 1f;
	[SerializeField] private float _basePointGainInterval = 5f;
	[SerializeField] private int _basePointGainAmount = 1;
	[SerializeField] private PointsObject _pointsObjectPrefab;

	private Rigidbody _rigidBody;
	private IntPair _targetTile;
	private float _globalSpeedModifier = 1f;

	private void Awake()
	{
		SpawnTime = Time.time;
		PointGainTime = Time.time;
	}

	private void Update()
	{
		IntPair currentPos = CurrentMazeTile;
		IntPair tileAtPreviousPosition = Maze.LocalPositionToMazePoint(PreviousPosition);

		if (!Maze.ContainsPersonAtCoordinates(this, currentPos))
		{
			Maze.RemovePersonFromCoordinates(this, tileAtPreviousPosition);
			Maze.AddPersonToCoordinates(this, currentPos);
		}

		if (currentPos == Maze.ExitPoint)
		{
			CurrentState = State.MovingPastExit;
		}

		switch (CurrentState)
		{
			case State.WaitingForMazeCreation:
				WaitForMazeCreation();
				break;
			case State.MovingToEntryPoint:
				GoToMazeEntry(currentPos);
				break;
			case State.Exploring:
				ExploreFrom(currentPos);
				break;
			case State.Retracing:
				RetraceFrom(currentPos);
				break;
			case State.MovingPastExit:
				ExitMaze(currentPos);
				break;
		}

		PreviousPosition = transform.position;

		CheckPointGain();
	}

	private void OnDrawGizmos()
	{
		//draw target tile
		//Gizmos.color = Color.cyan;
		//Gizmos.DrawSphere(Maze.MazePointToWorldPosition(TargetTile), 0.3f);
		//draw trail
		//Gizmos.color = Color.yellow; ;
		//foreach (IntPair pos in Trail)
		//{
		//	Gizmos.DrawSphere(Maze.MazePointToWorldPosition(pos), 0.5f);
		//}
		//draw secondary trail
		//Gizmos.color = Color.black;
		//foreach (IntPair pos in SecondaryTrail)
		//{
		//	Gizmos.DrawSphere(Maze.MazePointToWorldPosition(pos), 0.5f);
		//}
		//draw previous tile
		//Gizmos.color = Color.magenta;
		//Gizmos.DrawSphere(Maze.MazePointToWorldPosition(PreviousTile), 0.6f);
	}

	public MazeController Maze { get; set; }

	public PersonSpawner Spawner { get; set; }

	public PointsTracker PointsTracker { get; set; }

	private IntPair TargetTile
	{
		get => _targetTile;
		set
		{
			PreviousTile = _targetTile;
			_targetTile = value;
		}
	}

	private Vector3 PreviousPosition { get; set; }

	private IntPair PreviousTile { get; set; }

	private State CurrentState { get; set; } = State.WaitingForMazeCreation;

	private List<IntPair> Trail { get; set; } = new List<IntPair>();

	private List<IntPair> SecondaryTrail { get; set; } = new List<IntPair>();

	private float SpawnTime { get; set; }

	private float PointGainTime { get; set; }

	private void WaitForMazeCreation()
	{
		if (Maze != null)
		{
			TargetTile = Maze.EntryPoint;
			CurrentState = State.MovingToEntryPoint;
			Spawner.AddPersonToEnteringMazeSet(this);
		}
	}

	private void GoToMazeEntry(IntPair currentPos)
	{
		if (currentPos == Maze.EntryPoint)
		{
			ChooseNextTile(false, out IntPair nextTile, false);
			Trail.Add(currentPos);
			TargetTile = nextTile;
			CurrentState = State.Exploring;
			Spawner.RemovePersonFromEnteringMazeSet(this);
		}
		else
		{
			MoveTowards(Maze.EntryPoint);
		}
	}

	private void ExploreFrom(IntPair point)
	{
		bool reachedTarget = point == TargetTile;
		bool targetIsValid = TileIsNewAndValid(TargetTile);

		if (reachedTarget) Trail.Add(point);

		if (reachedTarget || !targetIsValid)
		{
			if (HasNoNewNearbyValidTiles(point))
			{
				if (IsDeadEnd(point))
				{
					SecondaryTrail.Clear();
					SecondaryTrail.Add(TargetTile);
					TargetTile = Trail[Trail.Count - 2];
					CurrentState = State.Retracing;
				}
				else
				{
					Debug.Log("Crossed old intersection, trying new path");
					ChooseNextTile(false, out IntPair nextTile, false);
					TargetTile = nextTile;
					PopTrailUntilPoint(TargetTile, false);
				}
			}
			else
			{
				ChooseNextTile(false, out IntPair nextTile, true);
				TargetTile = nextTile;
			}
		}
		else
		{
			MoveTowards(TargetTile);
		}
	}

	private void RetraceFrom(IntPair point)
	{
		bool reachedTarget = point == TargetTile;
		bool targetIsValid = Maze.IsOpen(TargetTile);

		if (reachedTarget) SecondaryTrail.Add(point);

		if (reachedTarget)
		{
			bool newPathAvailable = !HasNoNewNearbyValidTiles(point);
			if (newPathAvailable)
			{
				ChooseNextTile(false, out IntPair nextTile, true);
				TargetTile = nextTile;
				SecondaryTrail.Clear();
				CurrentState = State.Exploring;
			}
			else
			{
				Trail.Add(point);
				IntPair nextTile = TrailTileBeforeTarget(point);
				if (nextTile == TargetTile || TargetTile == Trail[0])
				{
					Trail.Clear();
					Trail = SecondaryTrail;
					SecondaryTrail = new List<IntPair>();
					TargetTile = Trail[Trail.Count - 1];
					CurrentState = State.Exploring;
				}
				else
				{
					TargetTile = nextTile;
				}
			}
		}
		else
		{
			if (targetIsValid)
			{
				MoveTowards(TargetTile);
			}
			else
			{
				if (SecondaryTrail.Count > 1)
				{
					TargetTile = SecondaryTrail[SecondaryTrail.Count - 2];
				}
				else
				{
					TargetTile = SecondaryTrail[SecondaryTrail.Count - 1];
				}
				Trail.Clear();
				SecondaryTrail.Clear();
				Trail.Add(point);
				CurrentState = State.Exploring;
			}
		}
	}

	private void ExitMaze(IntPair point)
	{
		IntPair finishLocation = FinishLocation;
		if (point == finishLocation)
		{
			Spawner.ExitMaze(this);
			Destroy(gameObject);
		}
		else
		{
			MoveTowards(FinishLocation);
		}
	}

	private IntPair FinishLocation => Maze.ExitPoint + IntPair.down * 3;

	private IntPair TrailTileBeforeTarget(IntPair target)
	{
		for (int i = 1; i < Trail.Count; i++)
		{
			if (Trail[i] == target) return Trail[i - 1];
		}

		return target;
	}

	private void PopTrailUntilPoint(IntPair targetTile, bool includeTarget)
	{
		if (!Trail.Contains(targetTile)) return;

		IntPair trailPoint;

		do
		{
			int index = Trail.Count - 1;
			trailPoint = Trail[index];
			bool foundTarget = trailPoint == targetTile;
			if (!foundTarget || includeTarget)
			{
				Trail.RemoveAt(index);
			}
		} while (targetTile != trailPoint);
	}

	private bool ChooseNextTile(bool checkForDeadEnd, out IntPair nextTile, bool mustBeNew)
	{
		IntPair currentPos = CurrentMazeTile;

		if (IntPair.IsAdjacentTo(currentPos, Maze.ExitPoint))
		{
			nextTile = Maze.ExitPoint;
			return true;
		}

		if (checkForDeadEnd && HasNoNewNearbyValidTiles(CurrentMazeTile))
		{
			nextTile = CurrentMazeTile;
			return false;
		}

		IntPair up = currentPos + IntPair.up;
		IntPair down = currentPos + IntPair.down;
		IntPair left = currentPos + IntPair.left;
		IntPair right = currentPos + IntPair.right;

		do
		{
			int randomDirection = UnityEngine.Random.Range(0, 4);
			switch (randomDirection)
			{
				default:
					nextTile = up;
					break;
				case 1:
					nextTile = down;
					break;
				case 2:
					nextTile = left;
					break;
				case 3:
					nextTile = right;
					break;
			}
		} while (mustBeNew
			? !TileIsNewAndValid(nextTile)
			: (!Maze.IsOpen(nextTile) || nextTile == PreviousTile));

		return true;
	}

	private bool TileIsNewAndValid(IntPair point) =>
		Maze.IsOpen(point) && !HasTravelledToRecently(point) && point != PreviousTile;

	private bool HasNoNewNearbyValidTiles(IntPair point)
	{
		IntPair currentPos = CurrentMazeTile;
		IntPair up = currentPos + IntPair.up;
		IntPair down = currentPos + IntPair.down;
		IntPair left = currentPos + IntPair.left;
		IntPair right = currentPos + IntPair.right;

		if (TileIsNewAndValid(up)) return false;
		if (TileIsNewAndValid(down)) return false;
		if (TileIsNewAndValid(left)) return false;
		if (TileIsNewAndValid(right)) return false;

		return true;
	}

	private bool IsDeadEnd(IntPair point)
	{
		int openCount = 0;
		IntPair currentPos = CurrentMazeTile;
		IntPair up = currentPos + IntPair.up;
		IntPair down = currentPos + IntPair.down;
		IntPair left = currentPos + IntPair.left;
		IntPair right = currentPos + IntPair.right;

		if (Maze.IsOpen(up)) openCount++;
		if (Maze.IsOpen(down)) openCount++;
		if (Maze.IsOpen(left)) openCount++;
		if (Maze.IsOpen(right)) openCount++;

		return openCount <= 1;
	}

	private bool HasTravelledToRecently(IntPair point) => Trail.Contains(point);

	private IntPair CurrentMazeTile => Maze.LocalPositionToMazePoint(transform.localPosition);

	private void MoveTowards(IntPair point)
	{
		Vector3 currentPos = transform.localPosition;
		Vector3 targetPos = Maze.MazePointToLocalPosition(point);
		Vector3 directionVector = targetPos - currentPos;
		float distance = Vector3.SqrMagnitude(directionVector);
		Vector3 direction = directionVector.normalized;
		Vector3 force = direction * ModifiedSpeed;
		Rigidbody.AddForce(force);
		//enforce max speed
		if (Rigidbody.velocity.magnitude > ModifiedSpeed)
		{
			Rigidbody.velocity = Rigidbody.velocity.normalized * ModifiedSpeed;
		}
		//try to stay up right
		Quaternion rot = Quaternion.FromToRotation(transform.up, Vector3.up);
		Rigidbody.AddTorque(new Vector3(rot.x, rot.y, rot.z) * Time.deltaTime * 100f);
		Rigidbody.centerOfMass = Vector3.zero;
	}

	private float ModifiedSpeed => _baseSpeed * Spawner.SpeedBonusModifier;

	private Rigidbody Rigidbody => _rigidBody != null ? _rigidBody : (_rigidBody = GetComponent<Rigidbody>());

	private void CheckPointGain()
	{
		if (Time.time - _basePointGainInterval < PointGainTime) return;
		PointGainTime = Time.time;
		int pointsEarned = CalculatePointGainAmount();
		PointsTracker.EarnPoints(pointsEarned);
		PointsObject obj = Instantiate(_pointsObjectPrefab);
		obj.SetAmount(pointsEarned);
		Vector3 pos = obj.transform.position;
		pos.x = transform.position.x;
		pos.z = transform.position.z;
		obj.transform.position = pos;
	}

	private int CalculatePointGainAmount()
	{
		float aliveTime = Time.time - SpawnTime;
		int gainCounter = (int)(aliveTime / _basePointGainInterval);
		return (int)(_basePointGainAmount * Mathf.Pow(1.05f, gainCounter) * Spawner.SkillPointBonusModifier);
	}
}
