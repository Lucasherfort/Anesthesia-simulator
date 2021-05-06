// DecompilerFi decompiler from Assembly-CSharp.dll class: NeedleReorient
using UnityEngine;

public class NeedleReorient : MonoBehaviour
{
	public Transform refObject;

	private bool enableReorient = true;

	private void Update()
	{
		if (enableReorient)
		{
			ChangeAngle();
		}
	}

	private void ChangeAngle()
	{
		Transform transform = base.transform;
		Vector3 eulerAngles = refObject.eulerAngles;
		transform.localEulerAngles = new Vector3(0f, eulerAngles.y, 0f);
	}

	public void DisableOrient()
	{
		enableReorient = false;
	}

	public void EnableOrient()
	{
		enableReorient = true;
	}
}
