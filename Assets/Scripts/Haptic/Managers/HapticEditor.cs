#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(HapticConfig))]
public class HapticEditor : Editor
{
    
    HapticConfig hapticConfig;

    public override void OnInspectorGUI()
    {
        hapticConfig = (HapticConfig)target;

        EditorGUILayout.LabelField("HAPTICAL PROBE", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        hapticConfig.hlTouchModel = (HapticConfig.HLTOUCH_MODEL)EditorGUILayout.EnumPopup("HL_Touch_Model", hapticConfig.hlTouchModel);
        hapticConfig.hlTouchable = (HapticConfig.HLFACING)EditorGUILayout.EnumPopup("HL_Facing", hapticConfig.hlTouchable);

        switch (hapticConfig.hlTouchModel)
        {
            case HapticConfig.HLTOUCH_MODEL.HL_CONTACT:
                hapticConfig.hlStiffness = EditorGUILayout.Slider("Stiffness", hapticConfig.hlStiffness, 0.0f, 1.0f);
                hapticConfig.hlDamping = EditorGUILayout.Slider("Damping", hapticConfig.hlDamping, 0.0f, 1.0f);
                hapticConfig.hlStaticFriction = EditorGUILayout.Slider("Static Friction", hapticConfig.hlStaticFriction, 0.0f, 1.0f);
                hapticConfig.hlDynamicFriction = EditorGUILayout.Slider("Dynamic Friction", hapticConfig.hlDynamicFriction, 0.0f, 1.0f);
                hapticConfig.hlPopThrough = EditorGUILayout.Slider("Pop-through", hapticConfig.hlPopThrough, 0.0f, 1.0f);
                break;
            case HapticConfig.HLTOUCH_MODEL.HL_CONSTRAINT:
                hapticConfig.snapDistance = EditorGUILayout.FloatField("Snap Distance", hapticConfig.snapDistance);
                break;
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("HAPTIC NEEDLE", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        hapticConfig.resistance = EditorGUILayout.FloatField("Skin resistance", hapticConfig.resistance);
    }
}
#endif
