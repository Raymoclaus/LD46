using System.Collections.Generic;
using UnityEngine;

public class PersonSpawner : MonoBehaviour
{
	[SerializeField] private Person _personPrefab;
	[SerializeField] private Transform _holder;
	[SerializeField] private MazeController _mazeController;
	[SerializeField] private float _spawnTime = 2f;
	[SerializeField] private int _baseSpawnLimit = 1;
	[SerializeField] private PointsTracker _pointsTracker;
	[SerializeField] private Dialogue _dialogue;
	[SerializeField] private List<string> _failedLines;

	private void Awake()
	{
		if (CanSpawn)
		{
			SpawnPerson();
		}
	}

	private void Update()
	{
		if (CanSpawn)
		{
			SpawnPerson();
		}

		if (People.Count == 0 && _pointsTracker.SkillPoints < SpawnBonusCost && SpawnCount >= _baseSpawnLimit + SpawnBonus && !Failed)
		{
			_dialogue.SetLines(_failedLines);
			_dialogue.Reset();
			_dialogue.ExitGameWhenDialogueEnds = true;
			Failed = true;
		}
	}

	public void LevelUpSpawnBonus()
	{
		if (!_pointsTracker.SpendPoints(SpawnBonusCost)) return;
		SpawnBonus++;
		SpawnBonusCost *= 2;
	}

	public void ResetPeople()
	{
		if (!_pointsTracker.SpendPoints(ResetCost)) return;
		foreach (Person p in People)
		{
			Destroy(p);
		}
		People.Clear();
		PeopleEnteringMaze.Clear();
		ResetCost *= 2;
	}

	public void UpgradeSpeedBonus()
	{
		if (!_pointsTracker.SpendPoints(SpeedBonusCost)) return;
		SpeedBonusModifier *= 1.2f;
		SkillPointBonusModifier *= 1.5f;
		SpeedBonusCost *= 2;
	}

	public int SpawnBonusCost { get; private set; } = 5;

	public int ResetCost { get; private set; } = 100;

	public int SpeedBonusCost { get; private set; } = 50;

	public void AddPersonToEnteringMazeSet(Person p) => PeopleEnteringMaze.Add(p);

	public void RemovePersonFromEnteringMazeSet(Person p) => PeopleEnteringMaze.Remove(p);

	public int AmountOfPeopleEnteringMaze => PeopleEnteringMaze.Count;

	public int SpawnsLeft => _baseSpawnLimit + SpawnBonus - SpawnCount;

	public void ExitMaze(Person p)
	{
		PeopleFinishedMaze++;
		People.Remove(p);
	}

	private int PeopleFinishedMaze { get; set; }

	private bool Failed { get; set; }

	private List<Person> People { get; set; } = new List<Person>();

	private int SpawnBonus { get; set; }

	private int SpawnCount { get; set; }

	private bool CanSpawn => Time.time - _spawnTime >= LastSpawnTime && SpawnCount < _baseSpawnLimit + SpawnBonus;

	private float LastSpawnTime { get; set; }

	public float SpeedBonusModifier { get; private set; } = 1f;

	public float SkillPointBonusModifier { get; private set; } = 1f;

	private HashSet<Person> PeopleEnteringMaze { get; set; } = new HashSet<Person>();

	private void SpawnPerson()
	{
		Person person = Instantiate(_personPrefab, _holder);
		person.transform.position = transform.position;
		person.Maze = _mazeController;
		person.Spawner = this;
		person.PointsTracker = _pointsTracker;
		LastSpawnTime = Time.time;
		People.Add(person);
		SpawnCount++;
	}
}
