// DecompilerFi decompiler from Assembly-CSharp.dll class: Alignment
using UnityEngine;

public class Alignment : MonoBehaviour
{
	public Transform theProbe;

	public Camera gameCamera;

	private int layerMask = 512;

	public float maxDist = 200f;

	private void Update()
	{
		Ray ray = gameCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
		if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDist, layerMask))
		{
			if (Input.GetMouseButton(0))
			{
				theProbe.position = theProbe.position;
				return;
			}
			theProbe.position = hitInfo.point;
			theProbe.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
		}
	}
}
