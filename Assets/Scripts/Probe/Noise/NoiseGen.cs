using UnityEngine;

public class NoiseGen : MonoBehaviour
{
	public int texWidth = 256;

	public int texHeight = 256;

	public float scale = 20f;

	public float offsetX;

	public float offsetY;

	public GameObject offsetObject;

	private Vector3 curPos;

	private void Start()
	{
		Vector3 position = offsetObject.transform.position;
		float x = position.x;
		Vector3 position2 = offsetObject.transform.position;
		float y = position2.y;
		Vector3 position3 = offsetObject.transform.position;
		curPos = new Vector3(x, y, position3.z);
		offsetX = UnityEngine.Random.Range(0f, 10000f);
		offsetY = UnityEngine.Random.Range(0f, 10000f);
		Renderer component = GetComponent<Renderer>();
		component.material.mainTexture = GenerateTexture();
	}

	private void Update()
	{
		RegenerateTexture();
	}

	private void RegenerateTexture()
	{
		Resources.UnloadUnusedAssets();
		float x = curPos.x;
		Vector3 position = offsetObject.transform.position;
		if (x != position.x)
		{
			offsetX = UnityEngine.Random.Range(0f, 10000f);
			float y = curPos.y;
			Vector3 position2 = offsetObject.transform.position;
			if (y != position2.y)
			{
				offsetY = UnityEngine.Random.Range(0f, 10000f);
			}
			Renderer component = GetComponent<Renderer>();
			component.material.mainTexture = GenerateTexture();
		}
		Vector3 position3 = offsetObject.transform.position;
		float x2 = position3.x;
		Vector3 position4 = offsetObject.transform.position;
		float y2 = position4.y;
		Vector3 position5 = offsetObject.transform.position;
		curPos = new Vector3(x2, y2, position5.z);
	}

	private Texture2D GenerateTexture()
	{
		Texture2D texture2D = new Texture2D(texWidth, texHeight);
		for (int i = 0; i < texWidth; i++)
		{
			for (int j = 0; j < texHeight; j++)
			{
				Color color = CalculateColor(i, j);
				texture2D.SetPixel(i, j, color);
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	private Color CalculateColor(int x, int y)
	{
		float x2 = (float)x / (float)texWidth * 0.3f * scale + offsetX;
		float y2 = (float)y / (float)texHeight * scale + offsetY;
		float num = Mathf.PerlinNoise(x2, y2);
		return new Color(num, num, num);
	}
}
