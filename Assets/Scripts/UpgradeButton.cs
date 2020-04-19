using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradeButton : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _costMesh;
	[SerializeField] private PointsTracker _pointsTracker;
	[SerializeField] private GameObject _buttonHider;

	private void Update()
	{
		int cost = CheckCost();
		_costMesh.text = $"-{cost}";
		_buttonHider.SetActive(_pointsTracker.SkillPoints < cost);
	}

	protected virtual int CheckCost() => 1;
}
