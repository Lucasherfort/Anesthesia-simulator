#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(HapticConfig))]
public class HapticEditor : Editor
{
    HapticConfig hapticConfig;
    public override void OnInspectorGUI()
    {
        hapticConfig = (HapticConfig)target;
    }

}

#endif