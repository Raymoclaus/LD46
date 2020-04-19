using UnityEngine;

public class ErrorMessage : MonoBehaviour
{
	[SerializeField] private CanvasGroup _canvasGroup;

	private void Start()
	{
		_canvasGroup.alpha = 0f;
	}

	private void Update()
	{
		_canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, 0f, Time.deltaTime * 0.3f);
	}

	public void Reveal() => _canvasGroup.alpha = 1f;
}
