using UnityEngine;

public class Wall : MonoBehaviour
{
	[SerializeField] private Animator _animator;

	public void Shrink()
	{
		_animator.SetTrigger("Shrink");
	}

	public void Destroy()
	{
		Destroy(gameObject);
	}
}
