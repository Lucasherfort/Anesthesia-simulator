using UnityEngine;


[CreateAssetMenu(fileName = "NoiseConfig", menuName = "Noise/NoiseConfig", order = 0)]
public class NoiseConfig : ScriptableObject
{
    [Header("General")]
    [Range(2, 256)]
    public int resolutionX = 256;
    [Range(2, 256)]
    public int resolutionY = 256;
    public bool mipChain = false;
    public FilterMode filterMode = FilterMode.Point;

    [Header("Noise parameters")]
    [Range(1, 16)]
    public int anisoLevel = 9;
    public float frequency = 20f;
    public float amplitude = 1f;

    [Header("Gradient")]
    public Gradient coloring;
}
