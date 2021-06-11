#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(NoiseConfig))]
public class NoiseEditor : Editor
{
    NoiseConfig noiseConfig;

    public override void OnInspectorGUI()
    {
        noiseConfig = (NoiseConfig)target;
    }
}
#endif
