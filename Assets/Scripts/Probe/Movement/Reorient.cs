// DecompilerFi decompiler from Assembly-CSharp.dll class: Reorient
using UnityEngine;

public class Reorient : MonoBehaviour
{
	public Transform refObject;

	private void Update()
	{
		ChangeAngle();
	}

	private void ChangeAngle()
	{
		Transform transform = base.transform;
		Vector3 eulerAngles = refObject.eulerAngles;
		transform.localEulerAngles = new Vector3(0f, eulerAngles.y, 0f);
	}
}
