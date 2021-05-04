// DecompilerFi decompiler from Assembly-CSharp.dll class: RotateControl
using UnityEngine;

public class RotateControl : MonoBehaviour
{
	public float rotateSpeed = 1f;

	private void Update()
	{
		RotateYAxis();
		RotateResetFunction1();
	}

	private void RotateYAxis()
	{
		if (UnityEngine.Input.GetKey("e"))
		{
			base.transform.Rotate(0f, rotateSpeed, 0f);
		}
		if (UnityEngine.Input.GetKey("q"))
		{
			base.transform.Rotate(0f, 0f - rotateSpeed, 0f);
		}
	}

	private void RotateResetFunction1()
	{
		if (UnityEngine.Input.GetKey("space"))
		{
			base.transform.rotation = base.transform.parent.rotation;
		}
	}
}
