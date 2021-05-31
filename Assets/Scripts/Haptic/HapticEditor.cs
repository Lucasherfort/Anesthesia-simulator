#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(HapticConfig))]
public class HapticEditor : Editor
{
    
    HapticConfig hapticConfig;

    public override void OnInspectorGUI()
    {
        hapticConfig = (HapticConfig)target;

        hapticConfig.hlTypeModel = (HapticConfig.TYPE_MODEL)EditorGUILayout.EnumPopup("TYPE_MODEL :", hapticConfig.hlTypeModel);

        switch (hapticConfig.hlTypeModel)
        {
            case HapticConfig.TYPE_MODEL.HLTOUCH_MODEL:
            hapticConfig.hlTouchModel = (HapticConfig.HLTOUCH_MODEL)EditorGUILayout.EnumPopup("HLTOUCH_MODEL :", hapticConfig.hlTouchModel);

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
            break;

            case HapticConfig.TYPE_MODEL.EFFECT_TYPE:
            hapticConfig.effectType = (HapticConfig.EFFECT_TYPE)EditorGUILayout.EnumPopup("EFFECT_TYPE :", hapticConfig.effectType);

            switch (hapticConfig.effectType)
            {
                    case HapticConfig.EFFECT_TYPE.CONSTANT:
                        hapticConfig.Direction = EditorGUILayout.Vector3Field("Direction", hapticConfig.Direction);
                        hapticConfig.Magnitude = EditorGUILayout.Slider("Magnitude", (float)hapticConfig.Magnitude, 0.0f, 1.0f);
                        break;
                    case HapticConfig.EFFECT_TYPE.FRICTION:
                        hapticConfig.Gain = EditorGUILayout.Slider("Gain", (float)hapticConfig.Gain, 0.0f, 1.0f);
                        hapticConfig.Magnitude = EditorGUILayout.Slider("Magnitude", (float)hapticConfig.Magnitude, 0.0f, 1.0f);
                        break;
                    case HapticConfig.EFFECT_TYPE.SPRING:
                        hapticConfig.Gain = EditorGUILayout.Slider("Gain", (float)hapticConfig.Gain, 0.0f, 1.0f);
                        hapticConfig.Magnitude = EditorGUILayout.Slider("Magnitude", (float)hapticConfig.Magnitude, 0.0f, 1.0f);
                        hapticConfig.Position = EditorGUILayout.Vector3Field("Position", hapticConfig.Position);
                        break;
                    case HapticConfig.EFFECT_TYPE.VIBRATE:
                        hapticConfig.Gain = EditorGUILayout.Slider("Gain", (float)hapticConfig.Gain, 0.0f, 1.0f);
                        hapticConfig.Magnitude = EditorGUILayout.Slider("Magnitude", (float)hapticConfig.Magnitude, 0.0f, 1.0f);
                        hapticConfig.Frequency = EditorGUILayout.Slider("Frequency", (float)hapticConfig.Frequency, 1.0f, 1000.0f);
                        hapticConfig.Direction = EditorGUILayout.Vector3Field("Direction", hapticConfig.Direction);
                        break;
                    case HapticConfig.EFFECT_TYPE.VISCOUS:
                        hapticConfig.Gain = EditorGUILayout.Slider("Gain", (float)hapticConfig.Gain, 0.0f, 1.0f);
                        hapticConfig.Magnitude = EditorGUILayout.Slider("Magnitude", (float)hapticConfig.Magnitude, 0.0f, 1.0f);
                        break;

                }

                break;
        }
    }
}
#endif
