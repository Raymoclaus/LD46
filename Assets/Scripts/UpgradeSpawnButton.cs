using UnityEngine;

public class UpgradeSpawnButton : UpgradeButton
{
	[SerializeField] private PersonSpawner _spawner;

	protected override int CheckCost() => _spawner.SpawnBonusCost;
}
