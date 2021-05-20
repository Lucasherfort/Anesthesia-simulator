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
    public TextureWrapMode wrapMode = TextureWrapMode.Clamp;
    public FilterMode filterMode = FilterMode.Point;
    [Range(1, 16)]
    public int anisoLevel = 9;

    [Header("Noise parameters")]
    public float scaleX = 0.3f;
    public float scaleY = 1.0f;
    public float frequency = 20f;
    public float persistance = 20f;
    public float lacunarity = 20f;
    public float amplitude = 1f;
    [Range(1,8)]
    public int octaves = 1;
    public int seed = 1;
    public Vector2 offset;

    [Header("ReferenceOffset")]
    public bool enabledReferenceOffset = false;

    [Header("Gradient")]
    public Gradient coloring;
}
