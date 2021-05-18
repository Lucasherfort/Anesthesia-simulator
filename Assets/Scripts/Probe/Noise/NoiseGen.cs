using UnityEngine;

public class NoiseGen : MonoBehaviour
{
    [Header("NoiseConfig")]
    [SerializeField]
    private NoiseConfig NoiseConfig;

    private int resolutionX = 256;
    private int resolutionY = 256;
	private float frequency = 200f;

	private float offsetX;

	private float offsetY;

    [Header("ReferenceOffset")]
    public GameObject offsetObject;

    private Gradient coloring;

	private Vector3 curPos;

	private void Start()
	{
        resolutionX = NoiseConfig.resolutionX;
        resolutionY = NoiseConfig.resolutionY;
        frequency = NoiseConfig.frequency;
        coloring = NoiseConfig.coloring;

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
        resolutionX = NoiseConfig.resolutionX;
        resolutionY = NoiseConfig.resolutionY;
        frequency = NoiseConfig.frequency;
        coloring = NoiseConfig.coloring;
        RegenerateTexture();
	}

	private void RegenerateTexture()
	{
		Resources.UnloadUnusedAssets();
		float x = curPos.x;
		Vector3 position = offsetObject.transform.position;
		if (x != position.x)
		{
			offsetX = Random.Range(0f, 10000f);
			float y = curPos.y;
			Vector3 position2 = offsetObject.transform.position;
			if (y != position2.y)
			{
				offsetY = Random.Range(0f, 10000f);
			}
			Renderer component = GetComponent<Renderer>();
			component.material.mainTexture = GenerateTexture();
		}
		float x2 = offsetObject.transform.position.x;
		float y2 = offsetObject.transform.position.y;
        float z2 = offsetObject.transform.position.z;
		curPos = new Vector3(x2, y2, z2);
	}

	private Texture2D GenerateTexture()
	{
		Texture2D texture2D = new Texture2D(resolutionX, resolutionY);
		for (int i = 0; i < resolutionX; i++)
		{
			for (int j = 0; j < resolutionY; j++)
			{
                float sample = CalculateSample(i, j);
                texture2D.SetPixel(i, j, coloring.Evaluate(sample));
            }
		}
		texture2D.Apply();
		return texture2D;
	}

    private float CalculateSample(int x, int y)
    {
        float x2 = (float)x / (float)resolutionX * 0.3f + offsetX;
        float y2 = (float)y / (float)resolutionY + offsetY;
        float num = Mathf.PerlinNoise(x2 * frequency, y2 * frequency);
        return num;
    }
}
