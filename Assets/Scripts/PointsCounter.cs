using TMPro;
using UnityEngine;

public class PointsCounter : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _textMesh;
	[SerializeField] private PointsTracker _pointsTracker;

	private void OnEnable()
	{
		UpdateText();
		_pointsTracker.OnSkillPointsUpdated += UpdateText;
	}

	private void OnDisable()
	{
		_pointsTracker.OnSkillPointsUpdated -= UpdateText;
	}

	private void UpdateText()
	{
		_textMesh.text = $"Fear Points: {_pointsTracker.SkillPoints}";
	}
}
