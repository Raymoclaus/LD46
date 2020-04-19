using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class AnimationAutoPlayer : MonoBehaviour
{
	private Animation _animation;
	private Animation Animation => _animation != null ? _animation : (_animation = GetComponent<Animation>());
	private int CurrentClipID { get; set; }
	private List<string> ClipNames { get; set; } = new List<string>();

	private void Start()
	{
		foreach (AnimationState clip in Animation)
		{
			ClipNames.Add(clip.name);
		}

		int start = Animation.playAutomatically ? 1 : 0;
		for (int i = start; i < ClipNames.Count; i++)
		{
			Animation.PlayQueued(ClipNames[i]);
		}
	}
}
