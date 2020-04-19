using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
	[SerializeField] private List<string> _lines;
	[SerializeField] private TextMeshProUGUI _textMesh;
	[SerializeField] private CanvasGroup _canvasGroup;
	[SerializeField] private float _characterRevealDelay;
	private int CurrentLineID { get; set; } = -1;
	private int RevealCount { get; set; }
	private float LastRevealTime { get; set; }
	private bool Fading { get; set; }

	private void Start()
	{
		Reset();
	}

	private void Update()
	{
		if (Fading)
		{
			_canvasGroup.alpha -= Time.unscaledDeltaTime * 0.5f;
			if (_canvasGroup.alpha == 0f)
			{
				_canvasGroup.interactable = false;
				_canvasGroup.blocksRaycasts = false;
				Time.timeScale = 1f;
			}
		}
		else
		{
			RevealCharacters();
		}
	}

	public bool ExitGameWhenDialogueEnds { get; set; }

	public void SetLines(List<string> lines)
	{
		_lines = lines;
	}

	public void Reset()
	{
		CurrentLineID = -1;
		Fading = false;
		RevealCount = 0;
		LastRevealTime = 0f;
		_canvasGroup.interactable = true;
		_canvasGroup.blocksRaycasts = true;
		ShowNextLine();
	}

	public void ShowNextLine()
	{
		if (AllCharactersRevealed)
		{
			CurrentLineID++;
			bool finishedLastLine = !ShowLine(CurrentLineID);
			if (!finishedLastLine)
			{
				Time.timeScale = 0f;
				_canvasGroup.alpha = 1f;
			}
			else if (ExitGameWhenDialogueEnds)
			{
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
			}
			else
			{
				Fading = finishedLastLine;
			}
		}
		else
		{
			RevealCount = CurrentLineLength;
		}
	}

	private bool AllCharactersRevealed => CurrentLineLength <= RevealCount;

	private void RevealCharacters()
	{
		if (Time.unscaledTime - _characterRevealDelay < LastRevealTime) return;
		LastRevealTime = Time.unscaledTime;
		RevealCount++;
		_textMesh.maxVisibleCharacters = RevealCount;
		if (CurrentLineLength >= RevealCount)
		{
			//play sound effect
		}
	}

	private int CurrentLineLength
	{
		get
		{
			if (CurrentLineID < 0 || CurrentLineID >= _lines.Count) return 0;
			return _lines[CurrentLineID].Length;
		}
	}

	private bool ShowLine(int index)
	{
		if (index < 0 || index >= _lines.Count) return false;
		RevealCount = 0;
		_textMesh.maxVisibleCharacters = RevealCount;
		_textMesh.text = _lines[index];
		return true;
	}
}
