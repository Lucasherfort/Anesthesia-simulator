using UnityEngine;

public class MeshDeformerInput : MonoBehaviour
{
	
	public float force = 10f;
	public float forceOffset = 0.1f;
	
	void Update ()
    {
		if (Input.GetMouseButton(0))
        {
            Debug.Log("1");
			HandleInput();
		}
	}

	void HandleInput ()
    {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
        Debug.Log("2");

        if (Physics.Raycast(inputRay, out hit))
        {
            Debug.Log("3");
            Debug.Log(hit.transform.name);
            MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
			if (deformer)
            {
                Debug.Log("4");
                Vector3 point = hit.point;
				point += hit.normal * forceOffset;
                deformer.AddDeformingForce(point, force);
            }
		}
	}
}