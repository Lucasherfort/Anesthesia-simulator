// DecompilerFi decompiler from Assembly-CSharp.dll class: NeedleCollide
using UnityEngine;

public class NeedleCollide : MonoBehaviour
{
	public GameObject moveRef;

	public GameObject tiltRef;

	public GameObject orientRef;

	private bool freeMove;

	private bool freeTilt;

	private void OnTriggerEnter(Collider col)
	{
		if (!freeMove)
		{
			moveRef.GetComponent<NeedleMove>().ChangeMoveSpeedTo0();
		}
		if (!freeTilt)
		{
			tiltRef.GetComponent<NeedleTilt>().ChangeTiltSpeedTo0();
		}
		orientRef.GetComponent<NeedleReorient>().DisableOrient();
	}

	private void OnTriggerStay(Collider col)
	{
		if (!freeMove)
		{
			moveRef.GetComponent<NeedleMove>().ChangeMoveSpeedTo0();
		}
		if (!freeTilt)
		{
			tiltRef.GetComponent<NeedleTilt>().ChangeTiltSpeedTo0();
		}
	}

	private void OnTriggerExit(Collider col)
	{
		moveRef.GetComponent<NeedleMove>().ChangeMoveSpeedTo1();
		tiltRef.GetComponent<NeedleTilt>().ChangeTiltSpeedTo1();
		orientRef.GetComponent<NeedleReorient>().EnableOrient();
	}

	public void FreeMoveToggle()
	{
		freeMove = !freeMove;
	}

	public void FreeTiltToggle()
	{
		freeTilt = !freeTilt;
	}
}
