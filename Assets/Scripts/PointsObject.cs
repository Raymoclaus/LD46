using TMPro;
using UnityEngine;

public class PointsObject : MonoBehaviour
{
	[SerializeField] private TextMeshPro _textMesh;
	[SerializeField] private Rigidbody _rigidbody;
	[SerializeField] private BoxCollider _collider;
	[SerializeField] private PhysicMaterial _physicsMaterial;
	private bool fading;

	private void Awake()
	{
		_rigidbody.velocity = Vector3.up * 10f;
		Vector3 randomTorque = new Vector3(Random.value, Random.value, Random.value);
		_rigidbody.AddTorque(randomTorque);
	}

	private void Update()
	{
		if (!fading) return;
		Color colour = _textMesh.color;
		colour.a -= Time.deltaTime * 0.3f;
		if (colour.a <= 0f)
		{
			Destroy(gameObject);
		}
		else
		{
			_textMesh.color = colour;
		}
	}

	public void SetAmount(int amount)
	{
		_textMesh.text = $"+{amount}";

		if (_collider != null)
		{
			Destroy(_collider);
		}

		_collider = gameObject.AddComponent<BoxCollider>();
		_collider.material = _physicsMaterial;
	}

	private void OnCollisionEnter(Collision col)
	{
		fading = true;
	}
}
