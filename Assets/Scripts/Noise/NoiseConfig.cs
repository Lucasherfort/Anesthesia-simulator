using UnityEngine;

[CreateAssetMenu(fileName = "NoiseConfig", menuName = "Noise/NoiseConfig", order = 0)]
public class NoiseConfig : ScriptableObject
{
    [Header("Texture parameters")]
    public TextureFormat textureFormat = TextureFormat.RGB24;
    [Range(2, 256)]
    public int resolutionX = 256;

    [Range(2, 256)]
    public int resolutionY = 256;

    public bool mipChain = false;

    public bool linear = false;

    public TextureWrapMode wrapMode = TextureWrapMode.Clamp;

    public FilterMode filterMode = FilterMode.Point;

    [Range(1, 16)]
    public int anisoLevel = 9;

    [Header("Noise parameters")]
    [Min(0.001f)]
    public float scale = 0.3f;

    [Min(0.001f)]
    public float frequency = 20f;

    [Range(0, 1)]
    public float persistance = 20f;

    [Min(0)]
    public float lacunarity = 20f;

    [Min(0.001f)]
    public float amplitude = 1f;

    [Range(1,8)]
    public int octaves = 1;

    [Min(0)]
    public int seed = 1;

    public bool offsetDynamic = false;

    public Vector2 offset;

    [Header("Gradient")]
    public Gradient coloring;
}
