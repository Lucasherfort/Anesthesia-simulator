// DecompilerFi decompiler from Assembly-CSharp.dll class: MouseTilt
using UnityEngine;

public class MouseTilt : MonoBehaviour
{
	public float tiltSpeed = 1f;

	private float xLimit;

	private float yLimit;

	private void Update()
	{
		TiltAxis();
		MouseLock();
		RotateResetFunction2();
	}

	private void MouseLock()
	{
		if (Input.GetMouseButtonDown(0))
		{
			HardwareCursor.SavePosition();
			Cursor.visible = false;
		}
		if (Input.GetMouseButtonUp(0))
		{
			HardwareCursor.LoadPosition();
			Cursor.visible = true;
		}
	}

	private void TiltAxis()
	{
		if (Input.GetMouseButton(0))
		{
			xLimit += UnityEngine.Input.GetAxis("Mouse X") * tiltSpeed;
			xLimit = Mathf.Clamp(xLimit, -25f, 25f);
			yLimit += UnityEngine.Input.GetAxis("Mouse Y") * tiltSpeed;
			yLimit = Mathf.Clamp(yLimit, -25f, 25f);
			Transform transform = base.transform;
			float x = yLimit;
			Vector3 localEulerAngles = base.transform.localEulerAngles;
			transform.localEulerAngles = new Vector3(x, localEulerAngles.y, 0f - xLimit);
		}
	}

	private void RotateResetFunction2()
	{
		if (Input.GetMouseButtonDown(2))
		{
			base.transform.rotation = base.transform.parent.rotation;
		}
	}
}
