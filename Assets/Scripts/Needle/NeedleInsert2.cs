// DecompilerFi decompiler from Assembly-CSharp.dll class: NeedleInsert2
using UnityEngine;

public class NeedleInsert2 : MonoBehaviour
{
	public float insertSpeed = 1f;

	private void FixedUpdate()
	{
		InsertNeedle();
		ResetNeedle();
	}

	private void InsertNeedle()
	{
		Transform transform = base.transform;
		Vector3 localPosition = base.transform.localPosition;
		transform.localPosition = new Vector3(0f, Mathf.Clamp(localPosition.y, -20f, 0f), 0f);
		if (UnityEngine.Input.GetKey("space") || UnityEngine.Input.GetKey(KeyCode.Keypad0))
		{
			base.transform.Translate(Vector3.down * insertSpeed);
		}
		if (UnityEngine.Input.GetKey("left shift") || UnityEngine.Input.GetKey(KeyCode.KeypadPeriod))
		{
			base.transform.Translate(Vector3.up * insertSpeed);
		}
	}

	private void ResetNeedle()
	{
		if (UnityEngine.Input.GetKeyDown("2") || UnityEngine.Input.GetKeyDown(KeyCode.Keypad2))
		{
			base.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
	}
}
