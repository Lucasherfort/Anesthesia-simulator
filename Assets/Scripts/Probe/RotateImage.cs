// DecompilerFi decompiler from Assembly-CSharp.dll class: RotateImage
using UnityEngine;

public class RotateImage : MonoBehaviour
{
	public GameObject rotRef;

	private void Update()
	{
		Transform transform = base.transform;
		Vector3 localEulerAngles = rotRef.transform.localEulerAngles;
		transform.localEulerAngles = new Vector3(0f, 0f, 0f - localEulerAngles.z);
	}
}
