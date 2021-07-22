#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(HapticConfig))]
public class HapticEditor : Editor
{
    HapticConfig hapticConfig;
    public override void OnInspectorGUI()
    {
        hapticConfig = (HapticConfig)target;

        EditorGUILayout.LabelField("PROBE PARAMETERS", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        hapticConfig.FirstPlanePosition = EditorGUILayout.FloatField("First Plane Position", hapticConfig.FirstPlanePosition);
        hapticConfig.SecondPlanePosition = EditorGUILayout.FloatField("Second Plane Position", hapticConfig.SecondPlanePosition);

        hapticConfig.FirstPlaneStiffness = EditorGUILayout.FloatField("First Plane Stiffness", hapticConfig.FirstPlaneStiffness);
        hapticConfig.SecondPlaneStiffness = EditorGUILayout.FloatField("Second Plane Stiffness", hapticConfig.SecondPlaneStiffness);
        hapticConfig.SkinLayerCutting = EditorGUILayout.FloatField("Skin Layer Cutting", hapticConfig.SkinLayerCutting);
        hapticConfig.SkinLayerStiffness = EditorGUILayout.FloatField("Skin Layer Stiffness", hapticConfig.SkinLayerStiffness);

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("NEEDLE PARAMETERS", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        hapticConfig.TISSUE_DIMENSIONS = EditorGUILayout.Vector3Field("TISSUE DIMENSIONS", hapticConfig.TISSUE_DIMENSIONS);
        hapticConfig.FIRST_LAYER_TOP = EditorGUILayout.FloatField("FIRST LAYER TOP", hapticConfig.FIRST_LAYER_TOP);

        hapticConfig.FirstLayerForceStiffness = EditorGUILayout.FloatField("First LayerForce Stiffness", hapticConfig.FirstLayerForceStiffness);
        hapticConfig.FirstLayerForceFriction = EditorGUILayout.FloatField("First Layer Force Friction", hapticConfig.FirstLayerForceFriction);
        hapticConfig.FirstLayerForceCutting = EditorGUILayout.FloatField("First Layer Force Cutting", hapticConfig.FirstLayerForceCutting);
        hapticConfig.FirstLayerDamping = EditorGUILayout.FloatField("First Layer Damping", hapticConfig.FirstLayerDamping);

        hapticConfig.SecondLayerForceStiffness = EditorGUILayout.FloatField("Second Layer Force Stiffness", hapticConfig.SecondLayerForceStiffness);
        hapticConfig.SecondLayerForceFriction = EditorGUILayout.FloatField("Second Layer Force Friction", hapticConfig.SecondLayerForceFriction);
        hapticConfig.SecondLayerForceCutting = EditorGUILayout.FloatField("Second Layer Force Cutting", hapticConfig.SecondLayerForceCutting);

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("OTHER PARAMETERS", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        hapticConfig.DEVICE_FORCE_SCALE = EditorGUILayout.FloatField("DEVICE FORCE SCALE", hapticConfig.DEVICE_FORCE_SCALE);
        hapticConfig.UnitLength = EditorGUILayout.FloatField("UnitLength", hapticConfig.UnitLength);
    }
}

#endif