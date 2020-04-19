using System;
using UnityEngine;

public class PointsTracker : MonoBehaviour
{
	private int _skillPoints = 10;

	public int SkillPoints
	{
		get => _skillPoints;
		set
		{
			int oldVal = _skillPoints;
			_skillPoints = value;
			if (oldVal != value)
			{
				OnSkillPointsUpdated?.Invoke();
			}
		}
	}

	public event Action OnSkillPointsUpdated;

	public bool SpendPoints(int amount)
	{
		if (amount <= 0) return false;
		if (amount > SkillPoints) return false;
		SkillPoints -= amount;
		return true;
	}

	public void EarnPoints(int amount)
	{
		SkillPoints += amount;
	}
}
