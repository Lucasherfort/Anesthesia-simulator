using UnityEngine;

public class NoiseGen : MonoBehaviour
{
    [Header("NoiseConfig")]
    [SerializeField]
    private NoiseConfig NoiseConfig = null;

    private int resolutionX = 256;
    private int resolutionY = 256;
	private float frequency = 200f;
	private float amplitude = 1f;
    private bool mipChain = false;
	private FilterMode filterMode = FilterMode.Point;
	private int anisoLevel = 9;

    [Header("ReferenceOffset")]
    public GameObject offsetObject;
    private Gradient coloring;
	private float offsetX;
	private float offsetY;
	private Vector3 curPos;
	private Texture2D texture;

	private void Start()
	{
        Vector3 position = offsetObject.transform.position;
		float x = position.x;
		Vector3 position2 = offsetObject.transform.position;
		float y = position2.y;
		Vector3 position3 = offsetObject.transform.position;
		curPos = new Vector3(x, y, position3.z);
		offsetX = Random.Range(0f, 10000f);
		offsetY = Random.Range(0f, 10000f);

		texture = new Texture2D(resolutionX, resolutionY, TextureFormat.RGB24, mipChain);
		SetupNoiseConfig();
		GetComponent<Renderer>().material.mainTexture = texture;
		GenerateTexture();
	}

	private void Update()
	{
		SetupNoiseConfig();
        RegenerateTexture();
	}

	private void SetupNoiseConfig()
	{
		resolutionX = NoiseConfig.resolutionX;
        resolutionY = NoiseConfig.resolutionY;
        frequency = NoiseConfig.frequency;
		amplitude = NoiseConfig.amplitude;
        coloring = NoiseConfig.coloring;
		mipChain = NoiseConfig.mipChain;
		filterMode = NoiseConfig.filterMode;
		anisoLevel = NoiseConfig.anisoLevel;
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

			GenerateTexture();
		}
		float x2 = offsetObject.transform.position.x;
		float y2 = offsetObject.transform.position.y;
        float z2 = offsetObject.transform.position.z;
		curPos = new Vector3(x2, y2, z2);
	}

	private void GenerateTexture()
	{
		for (int i = 0; i < resolutionX; i++)
		{
			for (int j = 0; j < resolutionY; j++)
			{
                float sample = CalculateNoise(i, j);
                texture.SetPixel(i, j, coloring.Evaluate(sample));

				// TODO
            }
		}
		texture.Apply();
	}

    private float CalculateNoise(int x, int y)
    {
        float x2 = (float)x / (float)resolutionX * 0.3f + offsetX;
        float y2 = (float)y / (float)resolutionY * 1.0f + offsetY;
        float noise = Mathf.PerlinNoise(x2 * frequency, y2 * frequency) * amplitude;
		//float noise = Mathf.PerlinNoise(x2 * frequency * 2f, y2 * frequency * 2f);
        //return (num + num2 * 0.5f) /1.5f;
		return noise;
    }
}
