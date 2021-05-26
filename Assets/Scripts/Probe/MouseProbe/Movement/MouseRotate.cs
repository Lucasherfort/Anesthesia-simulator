// DecompilerFi decompiler from Assembly-CSharp.dll class: MouseRotate
using UnityEngine;

public class MouseRotate : MonoBehaviour
{
	public float rotateSpeed = 1f;

	private void Update()
	{
		RotateYAxis();
		RotateResetFunction1();
	}

	private void RotateYAxis()
	{
		if (Input.GetMouseButton(0))
		{
			if (UnityEngine.Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				base.transform.Rotate(0f, 0.1f * rotateSpeed, 0f);
			}
			if (UnityEngine.Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				base.transform.Rotate(0f, -0.1f * rotateSpeed, 0f);
			}
		}
		else
		{
			if (UnityEngine.Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				base.transform.Rotate(0f, rotateSpeed, 0f);
			}
			if (UnityEngine.Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				base.transform.Rotate(0f, 0f - rotateSpeed, 0f);
			}
		}
	}

	private void RotateResetFunction1()
	{
		if (Input.GetMouseButtonDown(2))
		{
			base.transform.rotation = base.transform.parent.rotation;
		}
	}
}
