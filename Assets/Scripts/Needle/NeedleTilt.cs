// DecompilerFi decompiler from Assembly-CSharp.dll class: NeedleTilt
using UnityEngine;

public class NeedleTilt : MonoBehaviour
{
	private float tiltSpeed = 1f;

	private void Update()
	{
		TiltNeedle();
		ResetNeedle();
	}

	public void ChangeTiltSpeedTo1()
	{
		tiltSpeed = 1f;
	}

	public void ChangeTiltSpeedTo0()
	{
		tiltSpeed = 0f;
	}

	private void TiltNeedle()
	{
		if (!GameObject.Find("NeedleMove").GetComponentInChildren<NeedleMove>().needleMode)
		{
			return;
		}
		if (UnityEngine.Input.GetKey("a") || UnityEngine.Input.GetKey(KeyCode.Keypad4))
		{
			Quaternion rotation = base.transform.rotation;
			if (rotation.z < 0.6f)
			{
				base.transform.Rotate(0f, 0f, tiltSpeed);
			}
			else
			{
				base.transform.Rotate(0f, 0f, 0f);
			}
		}
		if (UnityEngine.Input.GetKey("d") || UnityEngine.Input.GetKey(KeyCode.Keypad6))
		{
			Quaternion rotation2 = base.transform.rotation;
			if (rotation2.z > -0.6f)
			{
				base.transform.Rotate(0f, 0f, 0f - tiltSpeed);
			}
			else
			{
				base.transform.Rotate(0f, 0f, 0f);
			}
		}
		if (UnityEngine.Input.GetKey("w") || UnityEngine.Input.GetKey(KeyCode.Keypad8))
		{
			Quaternion rotation3 = base.transform.rotation;
			if (rotation3.x < 0.6f)
			{
				base.transform.Rotate(tiltSpeed, 0f, 0f);
			}
			else
			{
				base.transform.Rotate(0f, 0f, 0f);
			}
		}
		if (UnityEngine.Input.GetKey("s") || UnityEngine.Input.GetKey(KeyCode.Keypad5))
		{
			Quaternion rotation4 = base.transform.rotation;
			if (rotation4.x > -0.6f)
			{
				base.transform.Rotate(0f - tiltSpeed, 0f, 0f);
			}
			else
			{
				base.transform.Rotate(0f, 0f, 0f);
			}
		}
	}

	private void ResetNeedle()
	{
		if (UnityEngine.Input.GetKeyDown("1") || UnityEngine.Input.GetKeyDown(KeyCode.Keypad1))
		{
			base.transform.rotation = base.transform.parent.rotation;
		}
	}
}
