using UnityEngine;
using System.Collections;

public class NoiseGenerator : MonoBehaviour
{
    [Header("NoiseConfig")]
    [SerializeField]
    private NoiseConfig NoiseConfig = null;
    [Header("Rendering")]
    public Renderer textureRender;

    ////////////////////////////////////////////////////////
    private TextureFormat textureFormat = TextureFormat.RGB24;
    private int resolutionX;
    private int resolutionY;
    private bool mipChain = false; 
    private TextureWrapMode wrapMode = TextureWrapMode.Clamp;
    private FilterMode filterMode = FilterMode.Point;
    [Range(1, 16)]
    public int anisoLevel = 9;
    private float scaleX;
    private float scaleY;
    private float frequency;
    private float amplitude;
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;
    public Gradient coloring;

    private void Update()
    {
        SetupNoiseConfig();
        GenerateNoise();
    }

    private void SetupNoiseConfig()
    {
        textureFormat = NoiseConfig.textureFormat;
        resolutionX = NoiseConfig.resolutionX;
        resolutionY = NoiseConfig.resolutionY;
        mipChain = NoiseConfig.mipChain;
        wrapMode = NoiseConfig.wrapMode;
        filterMode = NoiseConfig.filterMode;
        anisoLevel = NoiseConfig.anisoLevel;

        scaleX = NoiseConfig.scaleX;
        scaleY = NoiseConfig.scaleY;
        //frequency = NoiseConfig.frequency; 
        //amplitude = NoiseConfig.amplitude;
        octaves = NoiseConfig.octaves;
        persistance = NoiseConfig.persistance;
        lacunarity = NoiseConfig.lacunarity;
        seed = NoiseConfig.seed;
        offset = NoiseConfig.offset;

        coloring = NoiseConfig.coloring;
    }

    private void GenerateNoise()
    {
        float[,] noise = new float[resolutionX, resolutionY];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for(int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000,100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;

            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = resolutionX / 2f;
        float halfHeight = resolutionY / 2f;

        for(int y = 0; y < resolutionY;y++)
        {
            for(int x = 0; x < resolutionX; x++)
            {
                float tempFrequency = 1;
                float amplitude = 1;
                float noiseHeight = 0;

                for(int i = 0; i < octaves; i++)
                {
                    float sampleX = (x-halfWidth) / scaleX * tempFrequency + octaveOffsets[i].x;
                    float sampleY = (y-halfHeight) / scaleY * tempFrequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    tempFrequency *= lacunarity;
                }

                if(noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if(noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noise[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < resolutionY; y++)
        {
            for (int x = 0; x < resolutionX; x++)
            {
                noise[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noise[x, y]);
            }
        }

        int width = noise.GetLength(0);
        int height = noise.GetLength(1);

        Texture2D texture = new Texture2D(width, height, textureFormat,mipChain);
        texture.wrapMode = wrapMode;
        texture.filterMode = filterMode;
        texture.anisoLevel = NoiseConfig.anisoLevel;

        Color[] colorNoise = new Color[width * height];
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                colorNoise[y * width + x] = coloring.Evaluate(noise[x, y]);
            }
        }

        texture.SetPixels(colorNoise);
        texture.Apply();

        textureRender.material.mainTexture = texture;
    }
}
