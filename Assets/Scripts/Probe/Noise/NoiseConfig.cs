using UnityEngine;


[CreateAssetMenu(fileName = "NoiseConfig", menuName = "Noise/NoiseConfig", order = 0)]
public class NoiseConfig : ScriptableObject
{
    [Header("General")]
    [SerializeField]
    [Range(2, 256)]
    public int resolutionX = 256;

    [SerializeField]
    [Range(2, 256)]
    public int resolutionY = 256;

    [SerializeField]
    public float frequency = 20f;

    [Header("Gradient")]
    public Gradient coloring;
}
