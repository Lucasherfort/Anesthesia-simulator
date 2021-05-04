// DecompilerFi decompiler from Assembly-CSharp.dll class: CameraOrbit
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
	private Transform cameraParent;

	private Vector3 localRotation;

	public float orbitSpeed;

	private void Start()
	{
		cameraParent = base.transform.parent;
	}

	private void Update()
	{
		if ((UnityEngine.Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.Keypad7)) && UnityEngine.Input.GetAxis("Mouse X") != 0f)
		{
			localRotation.y += UnityEngine.Input.GetAxis("Mouse X") * orbitSpeed;
			cameraParent.transform.eulerAngles = new Vector3(localRotation.x, localRotation.y, localRotation.z);
		}
	}
}
