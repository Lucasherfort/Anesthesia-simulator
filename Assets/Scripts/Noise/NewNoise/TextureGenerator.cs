using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGenerator : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public void GenerateNoise()
    {
        float[,] noiseMap = Noise2.GenerateNoise(mapWidth,mapHeight,seed, noiseScale,octaves,persistance,lacunarity, offset);

        TextureDisplay display = FindObjectOfType<TextureDisplay>();
        display.DrawNoiseMap(noiseMap);
    }

    private void Update()
    {
        GenerateNoise();
    }
}
