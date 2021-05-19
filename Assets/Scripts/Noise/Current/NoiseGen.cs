using UnityEngine;

public class NoiseGen : MonoBehaviour
{
    [Header("NoiseConfig")]
    [SerializeField]
    private NoiseConfig NoiseConfig = null;

    private TextureFormat textureFormat;
    private int resolutionX = 256;
    private int resolutionY = 256;
	private float frequency = 200f;
	private float amplitude = 1f;
    private bool mipChain = false;
    private TextureWrapMode wrapMode;
    private FilterMode filterMode;
    private int anisoLevel;
    private int octaves;

    private float scaleX = 0f;
    private float scaleY = 0f;

    [Header("ReferenceOffset")]
    public GameObject offsetObject;

    private Gradient coloring;
	private float offsetX;
	private float offsetY;
	private Vector3 curPos;
	private Texture2D texture;
    private bool enabledReferenceOffset = false;

    private void Start()
	{
        SetupNoiseConfig();

        if (offsetObject == null)
        {
            float x = offsetObject.transform.position.x;
            float y = offsetObject.transform.position.y;
            float z = offsetObject.transform.position.z;
            curPos = new Vector3(x, y, z);
        }

        offsetX = Random.Range(0f, 10000f);
		offsetY = Random.Range(0f, 10000f);

		texture = new Texture2D(resolutionX, resolutionY, textureFormat, mipChain);

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
        textureFormat = NoiseConfig.textureFormat;
        resolutionX = NoiseConfig.resolutionX;
        resolutionY = NoiseConfig.resolutionY;
        octaves = NoiseConfig.octaves;
        scaleX = NoiseConfig.scaleX;
        scaleY = NoiseConfig.scaleY;

        frequency = NoiseConfig.frequency;
		amplitude = NoiseConfig.amplitude;
        coloring = NoiseConfig.coloring;
		mipChain = NoiseConfig.mipChain;

        wrapMode = NoiseConfig.wrapMode;
        filterMode = NoiseConfig.filterMode;
		anisoLevel = NoiseConfig.anisoLevel;

        enabledReferenceOffset = NoiseConfig.enabledReferenceOffset;

        if(texture != null)
        {
            texture.wrapMode = wrapMode;
            texture.filterMode = filterMode;
            texture.anisoLevel = anisoLevel;
        }
	}

	private void RegenerateTexture()
	{
		Resources.UnloadUnusedAssets();

        if(enabledReferenceOffset)
        {
            if(offsetObject == null)
            {
                float x = offsetObject.transform.position.x;
                float y = offsetObject.transform.position.y;
                float z = offsetObject.transform.position.z;
                curPos = new Vector3(x, y, z);
            }

            if (curPos.x != offsetObject.transform.position.x)
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
        }
        else
        {
            offsetX = Random.Range(0f, 10000f);
            offsetY = Random.Range(0f, 10000f);
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
        float xCoord = (float)x / (float)resolutionX * scaleX + offsetX;
        float yCoord = (float)y / (float)resolutionY * scaleY + offsetY;

        float sum = Mathf.PerlinNoise(xCoord * frequency, yCoord * frequency) * amplitude;

        return sum;
    }
}
