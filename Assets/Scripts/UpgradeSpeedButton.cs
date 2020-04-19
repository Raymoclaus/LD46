using UnityEngine;

public class UpgradeSpeedButton : UpgradeButton
{
	[SerializeField] private PersonSpawner _spawner;

	protected override int CheckCost() => _spawner.SpeedBonusCost;
}
