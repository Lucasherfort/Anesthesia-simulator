// DecompilerFi decompiler from Assembly-CSharp.dll class: NeedleMove
using UnityEngine;
using UnityEngine.UI;

public class NeedleMove : MonoBehaviour
{
	private float needleSpeed = 0.1f;

	private Vector3 oriPos;

	public bool needleMode;

	private void Start()
	{
		oriPos = base.transform.position;
	}

	private void Update()
	{
		MoveNeedle();
		NeedleModeToggle();
		ReturnNeedle();
	}

	public void ChangeMoveSpeedTo1()
	{
		needleSpeed = 0.1f;
	}

	public void ChangeMoveSpeedTo0()
	{
		needleSpeed = 0f;
	}

	private void NeedleModeToggle()
	{
		if (UnityEngine.Input.GetKeyDown("q") || UnityEngine.Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			needleMode = !needleMode;
			if (!needleMode)
			{
				GameObject.Find("NeedleModeButton").GetComponentInChildren<Text>().text = "Needle Mode: Move";
			}
			if (needleMode)
			{
				GameObject.Find("NeedleModeButton").GetComponentInChildren<Text>().text = "Needle Mode: Tilt";
			}
		}
	}

	private void MoveNeedle()
	{
		if (!needleMode)
		{
			if (UnityEngine.Input.GetKey("w") || UnityEngine.Input.GetKey(KeyCode.Keypad8))
			{
				base.transform.Translate(Vector3.forward * needleSpeed);
			}
			if (UnityEngine.Input.GetKey("s") || UnityEngine.Input.GetKey(KeyCode.Keypad5))
			{
				base.transform.Translate(Vector3.back * needleSpeed);
			}
			if (UnityEngine.Input.GetKey("a") || UnityEngine.Input.GetKey(KeyCode.Keypad4))
			{
				base.transform.Translate(Vector3.left * needleSpeed);
			}
			if (UnityEngine.Input.GetKey("d") || UnityEngine.Input.GetKey(KeyCode.Keypad6))
			{
				base.transform.Translate(Vector3.right * needleSpeed);
			}
		}
	}

	private void ReturnNeedle()
	{
		if (UnityEngine.Input.GetKeyDown("3") || UnityEngine.Input.GetKeyDown(KeyCode.Keypad3))
		{
			base.transform.position = oriPos;
		}
	}
}
