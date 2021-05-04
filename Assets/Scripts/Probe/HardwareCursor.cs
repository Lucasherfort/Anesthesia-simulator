// DecompilerFi decompiler from Assembly-CSharp-firstpass.dll class: HardwareCursor
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UnityEngine;

public class HardwareCursor : MonoBehaviour
{
	public static HardwareCursor Self;

	public static int SavesLength;

	public static Point[] Positions;

	public static float Simulating;

	public static Vector2 Simulation;

	public static Vector2 WindowOffset;

	public static float OffSet;

	public static float ExtraAccurancy;

	public static bool TriggeredLeft;

	[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern void mouse_event(uint dwFlags, uint cButtons, uint dwExtraInfo);

	[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern void mouse_event(int flags, int dX, int dY, int buttons, int extraInfo);

	private void Awake()
	{
		Self = this;
		Positions = new Point[0];
		float num = System.Windows.Forms.Cursor.Position.X;
		Vector3 mousePosition = UnityEngine.Input.mousePosition;
		float x = num - mousePosition.x;
		float num2 = System.Windows.Forms.Cursor.Position.Y;
		float num3 = UnityEngine.Screen.height;
		Vector3 mousePosition2 = UnityEngine.Input.mousePosition;
		WindowOffset = new Vector2(x, num2 - (num3 - mousePosition2.y));
	}

	private void OnGUI()
	{
		float num = System.Windows.Forms.Cursor.Position.X;
		Vector2 mousePosition = Event.current.mousePosition;
		float x = num - mousePosition.x;
		float num2 = System.Windows.Forms.Cursor.Position.Y;
		Vector2 mousePosition2 = Event.current.mousePosition;
		WindowOffset = new Vector2(x, num2 - mousePosition2.y - 1f);
	}

	private void Update()
	{
		if (Simulating == 0f)
		{
			if (OffSet != 0f)
			{
				OffSet = 0f;
			}
			if (Simulation != Vector2.zero)
			{
				Simulation = Vector2.zero;
			}
			if (ExtraAccurancy != 0f)
			{
				ExtraAccurancy = 0f;
			}
		}
		if (SavesLength != Positions.Length)
		{
			SavesLength = Positions.Length;
		}
		if (Simulating != 0f)
		{
			float num = Mathf.Lerp(System.Windows.Forms.Cursor.Position.X, Simulation.x, Simulating + ExtraAccurancy);
			float num2 = Mathf.Lerp(System.Windows.Forms.Cursor.Position.Y, Simulation.y, Simulating + ExtraAccurancy);
			System.Windows.Forms.Cursor.Position = new Point((int)num, (int)num2);
			if (Vector2.Distance(new Vector2(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y), Simulation) <= 10f)
			{
				ExtraAccurancy = 1f;
			}
			if (Vector2.Distance(new Vector2(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y), Simulation) <= 1f)
			{
				Simulating = 0f;
			}
		}
	}

	public static void SetPosition(Vector2 Pos)
	{
		SetPosition((int)Pos.x, (int)Pos.y);
	}

	public static void SetPosition(int x, int y)
	{
		System.Windows.Forms.Cursor.Position = new Point(x, y);
	}

	public static void SetLocalPosition(Vector2 Pos)
	{
		SetLocalPosition((int)Pos.x, (int)Pos.y);
	}

	public static void SetLocalPosition(int x, int y)
	{
		if (!UnityEngine.Screen.fullScreen)
		{
			System.Windows.Forms.Cursor.Position = new Point(x + (int)WindowOffset.x, UnityEngine.Screen.height - y + (int)WindowOffset.y);
			return;
		}
		float num = UnityEngine.Screen.height;
		float num2 = UnityEngine.Screen.resolutions[UnityEngine.Screen.resolutions.Length - 1].height;
		SetPosition((int)(num2 / num * (float)x), (int)(num2 / num * (float)(UnityEngine.Screen.height - y) - num2 / num));
	}

	public static Vector2 GetPosition()
	{
		return new Vector2(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
	}

	public static Vector2 GetLocalPosition()
	{
		return UnityEngine.Input.mousePosition;
	}

	public static void SavePosition()
	{
		SavePosition(0);
	}

	public static void SavePosition(int SaveNumber)
	{
		if (SaveNumber < SavesLength)
		{
			if (SaveNumber >= 0)
			{
				Positions[SaveNumber] = System.Windows.Forms.Cursor.Position;
			}
			else
			{
				UnityEngine.Debug.LogError("The requested Save file cant be below 0 (zero).");
			}
			return;
		}
		Point[] positions = Positions;
		Positions = new Point[SaveNumber + 1];
		for (int i = 0; i < positions.Length; i++)
		{
			Positions[i] = positions[i];
		}
		Positions[SaveNumber] = System.Windows.Forms.Cursor.Position;
		UnityEngine.Debug.Log("The requested Save file exceeded the number of defined saves ! Error Fixed.");
	}

	public static void LoadPosition()
	{
		LoadPosition(0);
	}

	public static void LoadPosition(int LoadNumber)
	{
		if (LoadNumber < SavesLength)
		{
			if (LoadNumber >= 0)
			{
				System.Windows.Forms.Cursor.Position = Positions[LoadNumber];
			}
			else
			{
				UnityEngine.Debug.LogError("The requested Load file cant be below 0 (zero).");
			}
		}
		else
		{
			System.Windows.Forms.Cursor.Position = new Point(0, 0);
			UnityEngine.Debug.Log("The requested Load file exceeds the number of defined saves ! Using default position at (0,0).");
		}
	}

	public static void SetSavesLength(int Saves)
	{
		if (Saves >= SavesLength)
		{
			Point[] positions = Positions;
			Positions = new Point[Saves + 1];
			for (int i = 0; i < positions.Length; i++)
			{
				Positions[i] = positions[i];
			}
		}
		else if (Saves < 0)
		{
			UnityEngine.Debug.LogError("Cursor.SavesLength cant be less than 0 (zero) !");
		}
		else
		{
			UnityEngine.Debug.LogError("There are already more Save slots than " + Saves + ", Current slots: " + SavesLength + ".");
		}
	}

	public static void SimulateAutoMove(int FinalPositionX, int FinalPositionY, float Speed)
	{
		SimulateMove(new Vector2(FinalPositionX, FinalPositionY), Speed, Automaticaly: true);
	}

	public static void SimulateAutoMove(Vector2 FinalPosition, float Speed)
	{
		SimulateMove(FinalPosition, Speed, Automaticaly: true);
	}

	public static void SimulateMove(int FinalPositionX, int FinalPositionY, float Speed)
	{
		SimulateMove(new Vector2(FinalPositionX, FinalPositionY), Speed, Automaticaly: false);
	}

	public static void SimulateMove(Vector2 FinalPosition, float Speed)
	{
		SimulateMove(FinalPosition, Speed, Automaticaly: false);
	}

	public static void SimulateMove(int FinalPositionX, int FinalPositionY, float Speed, bool Automaticaly)
	{
		SimulateMove(new Vector2(FinalPositionX, FinalPositionY), Speed, Automaticaly);
	}

	public static void SimulateMove(Vector2 FinalPosition, float Speed, bool Automaticaly)
	{
		if (!Automaticaly)
		{
			Point position = System.Windows.Forms.Cursor.Position;
			float num = Mathf.Lerp(position.X, FinalPosition.x, Speed);
			float num2 = Mathf.Lerp(position.Y, FinalPosition.y, Speed);
			System.Windows.Forms.Cursor.Position = new Point((int)num, (int)num2);
		}
		else
		{
			OffSet = 0f;
			Simulation = FinalPosition;
			Simulating = Speed;
		}
	}

	public static void SimulateAutoLocalMove(Vector2 FinalPosition, float Speed)
	{
		SimulateLocalMove(FinalPosition, Speed, Automaticaly: true);
	}

	public static void SimulateAutoLocalMove(int FinalPositionX, int FinalPositionY, float Speed)
	{
		SimulateLocalMove(new Vector2(FinalPositionX, FinalPositionY), Speed, Automaticaly: true);
	}

	public static void SimulateLocalMove(int FinalPositionX, int FinalPositionY, float Speed)
	{
		SimulateLocalMove(new Vector2(FinalPositionX, FinalPositionY), Speed, Automaticaly: false);
	}

	public static void SimulateLocalMove(Vector2 FinalPosition, float Speed)
	{
		SimulateLocalMove(FinalPosition, Speed, Automaticaly: false);
	}

	public static void SimulateLocalMove(int FinalPositionX, int FinalPositionY, float Speed, bool Automaticaly)
	{
		SimulateLocalMove(new Vector2(FinalPositionX, FinalPositionY), Speed, Automaticaly);
	}

	public static void SimulateLocalMove(Vector2 FinalPosition, float Speed, bool Automaticaly)
	{
		FinalPosition += WindowOffset;
		if (!Automaticaly)
		{
			Point position = System.Windows.Forms.Cursor.Position;
			float num = Mathf.Lerp(position.X, FinalPosition.x, Speed);
			float num2 = Mathf.Lerp(position.Y, FinalPosition.y, Speed);
			System.Windows.Forms.Cursor.Position = new Point((int)num, (int)num2);
		}
		else
		{
			OffSet = 0f;
			Simulation = FinalPosition;
			Simulating = Speed;
		}
	}

	public static void SimulateController(float Horizontal, float Vertical)
	{
		System.Windows.Forms.Cursor.Position = new Point((int)((float)System.Windows.Forms.Cursor.Position.X + Horizontal), (int)((float)System.Windows.Forms.Cursor.Position.Y - Vertical));
	}

	public static void SimulateController(float Horizontal, float Vertical, float Speed)
	{
		System.Windows.Forms.Cursor.Position = new Point((int)((float)System.Windows.Forms.Cursor.Position.X + Horizontal * Speed), (int)((float)System.Windows.Forms.Cursor.Position.Y + Vertical * (0f - Speed)));
	}

	public static void SimulateSmoothController(float Horizontal, float Vertical, float Speed, float SpeedScale)
	{
		float num = Mathf.Lerp(System.Windows.Forms.Cursor.Position.X, (float)System.Windows.Forms.Cursor.Position.X + Horizontal * Speed, SpeedScale);
		float num2 = Mathf.Lerp(System.Windows.Forms.Cursor.Position.Y, (float)System.Windows.Forms.Cursor.Position.Y + Vertical * (0f - Speed), SpeedScale);
		System.Windows.Forms.Cursor.Position = new Point((int)num, (int)num2);
	}

	public static void ScrollWheel(int Direction)
	{
		mouse_event(2048, 0, 0, Direction * 20, 0);
	}

	public static void LeftClick()
	{
		mouse_event(6u, 0u, 0u);
	}

	public static void LeftClickDown()
	{
		mouse_event(2u, 0u, 0u);
	}

	public static void LeftClickUp()
	{
		mouse_event(4u, 0u, 0u);
	}

	public static void RightClick()
	{
		mouse_event(24u, 0u, 0u);
	}

	public static void RightClickDown()
	{
		mouse_event(8u, 0u, 0u);
	}

	public static void RightClickUp()
	{
		mouse_event(16u, 0u, 0u);
	}

	public static void MiddleClick()
	{
		mouse_event(96u, 0u, 0u);
	}

	public static void MiddleClickDown()
	{
		mouse_event(32u, 0u, 0u);
	}

	public static void MiddleClickUp()
	{
		mouse_event(64u, 0u, 0u);
	}

	public static void LeftClickEquals(KeyCode KeycodeId)
	{
		if (UnityEngine.Input.GetKeyDown(KeycodeId))
		{
			mouse_event(2u, 0u, 0u);
		}
		if (UnityEngine.Input.GetKeyUp(KeycodeId))
		{
			mouse_event(4u, 0u, 0u);
		}
	}

	public static void RightClickEquals(KeyCode KeycodeId)
	{
		if (UnityEngine.Input.GetKeyDown(KeycodeId))
		{
			mouse_event(8u, 0u, 0u);
		}
		if (UnityEngine.Input.GetKeyUp(KeycodeId))
		{
			mouse_event(16u, 0u, 0u);
		}
	}

	public static void MiddleClickEquals(KeyCode KeycodeId)
	{
		if (UnityEngine.Input.GetKeyDown(KeycodeId))
		{
			mouse_event(32u, 0u, 0u);
		}
		if (UnityEngine.Input.GetKeyUp(KeycodeId))
		{
			mouse_event(64u, 0u, 0u);
		}
	}
}
