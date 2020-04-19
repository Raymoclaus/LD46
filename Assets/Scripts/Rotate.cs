using UnityEngine;

public class Rotate : MonoBehaviour
{
	[SerializeField] private float _xSpeed, _ySpeed, _zSpeed;

	private void Update()
	{
		Vector3 speed = new Vector3(
			_xSpeed * Time.deltaTime,
			_ySpeed * Time.deltaTime,
			_zSpeed * Time.deltaTime);
		transform.eulerAngles = transform.eulerAngles + speed;
	}
}
