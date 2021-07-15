#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(HapticConfig))]
public class HapticEditor : Editor
{
    HapticConfig hapticConfig;
    public override void OnInspectorGUI()
    {
        hapticConfig = (HapticConfig)target;

        hapticConfig.FirstPlanePosition = EditorGUILayout.FloatField("First Plane Position", hapticConfig.FirstPlanePosition);
        hapticConfig.SecondPlanePosition = EditorGUILayout.FloatField("Second Plane Position", hapticConfig.SecondPlanePosition);

        hapticConfig.FirstPlaneStiffness = EditorGUILayout.FloatField("First Plane Stiffness", hapticConfig.FirstPlaneStiffness);
        hapticConfig.SecondPlaneStiffness = EditorGUILayout.FloatField("Second Plane Stiffness", hapticConfig.SecondPlaneStiffness);

        hapticConfig.TISSUE_DIMENSIONS = EditorGUILayout.Vector3Field("TISSUE DIMENSIONS", hapticConfig.TISSUE_DIMENSIONS);
        hapticConfig.GROUND_LEVEL = EditorGUILayout.FloatField("GROUND LEVEL", hapticConfig.GROUND_LEVEL);
        hapticConfig.FIRST_LAYER_TOP = EditorGUILayout.FloatField("FIRST LAYER TOP", hapticConfig.FIRST_LAYER_TOP);

        hapticConfig.contactPosition = EditorGUILayout.Vector3Field("contactPosition", hapticConfig.contactPosition);
        hapticConfig.StartPointPosition = EditorGUILayout.Vector3Field("StartPointPosition", hapticConfig.StartPointPosition);

        hapticConfig.FirstLayerStiffness = EditorGUILayout.FloatField("FirstLayerStiffness", hapticConfig.FirstLayerStiffness);

        hapticConfig.DEVICE_FORCE_SCALE = EditorGUILayout.FloatField("DEVICE FORCE SCALE", hapticConfig.DEVICE_FORCE_SCALE);
        hapticConfig.FirstLayerDamping = EditorGUILayout.FloatField("FirstLayerDamping", hapticConfig.FirstLayerDamping);
        hapticConfig.SkinLayerCutting = EditorGUILayout.FloatField("SkinLayerCutting", hapticConfig.SkinLayerCutting);
    }
}

#endif