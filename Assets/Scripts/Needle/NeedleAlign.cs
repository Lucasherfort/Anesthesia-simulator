// DecompilerFi decompiler from Assembly-CSharp.dll class: NeedleAlign
using UnityEngine;

public class NeedleAlign : MonoBehaviour
{
	public Transform targetMarker;

	private int layermask = 16386;

	private void Update()
	{
		if (Physics.Raycast(base.transform.position, -base.transform.up, out RaycastHit hitInfo, layermask))
		{
			targetMarker.position = hitInfo.point;
			targetMarker.rotation = Quaternion.FromToRotation(Vector3.forward, hitInfo.normal);
		}
	}
}
