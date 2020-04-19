using UnityEngine;

public class ResetButton : UpgradeButton
{
	[SerializeField] private PersonSpawner _spawner;

	protected override int CheckCost() => _spawner.ResetCost;
}
