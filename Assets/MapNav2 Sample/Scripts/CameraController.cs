
using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{
	public float rotateSpeed = 100f;
	private float rot = 0f;
	private float distance = 0f;
	private Vector3 focusPoint = Vector3.zero;

	protected void Awake() 
	{
		rot = transform.eulerAngles.y;
		distance = transform.position.magnitude;
		Refocus(Vector3.zero);
	}
	
	protected void Update()
	{
		if ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) && GUIUtility.hotControl == 0)
		{
			rot += (Input.GetKey(KeyCode.LeftArrow) ? 1 : -1) * rotateSpeed * Time.deltaTime;
			Quaternion q = Quaternion.Euler(transform.eulerAngles.x, rot, transform.eulerAngles.z);
			Vector3 direction = q * Vector3.forward;
			transform.position = focusPoint - direction * distance;
			transform.LookAt(focusPoint);
		}
	}

	public void Refocus(Vector3 focusPoint)
	{
		focusPoint.y = 0f;
		this.focusPoint = focusPoint;
		Quaternion q = Quaternion.Euler(transform.eulerAngles.x, rot, transform.eulerAngles.z);
		Vector3 direction = q * Vector3.forward;
		transform.position = focusPoint - direction * distance;
		transform.LookAt(focusPoint);
	}

}
