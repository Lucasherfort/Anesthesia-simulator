using UnityEngine;

[CreateAssetMenu(fileName = "HapticConfig", menuName = "Haptic/HapticConfig", order = 1)]
public class HapticConfig : ScriptableObject
{
    public enum TYPE_MODEL { HLTOUCH_MODEL, EFFECT_TYPE };

    public TYPE_MODEL hlTypeModel = TYPE_MODEL.HLTOUCH_MODEL;

    //////////////////////////PROBE TOUCH//////////////////////////////////////

    public enum HLTOUCH_MODEL { HL_CONTACT, HL_CONSTRAINT };

    public HLTOUCH_MODEL hlTouchModel = HLTOUCH_MODEL.HL_CONTACT;  

    public enum HLFACING { HL_FRONT, HL_BACK, HL_FRONT_AND_BACK };

    public HLFACING hlTouchable = HLFACING.HL_FRONT; 

    [Range(0.0f, 1.0f)]
    public float hlStiffness = 0.7f;  

    [Range(0.0f, 1.0f)]
    public float hlDamping = 0.1f;    

    [Range(0.0f, 1.0f)]
    public float hlStaticFriction = 0.2f;   

    [Range(0.0f, 1.0f)]
    public float hlDynamicFriction = 0.3f;  

    [Range(0.0f, 1.0f)]
    public float hlPopThrough = 0.0f;   

    public float snapDistance = 1.0f; 


    //////////////////////////NEEDLE EFFECT//////////////////////////////////////

    public enum EFFECT_TYPE { CONSTANT, VISCOUS, SPRING, FRICTION, VIBRATE };

    public EFFECT_TYPE effectType = EFFECT_TYPE.VISCOUS;

    [Range(0.0f, 1.0f)]
    public double Gain = 0.333f;

    [Range(0.0f, 1.0f)]
    public double Magnitude = 0.333f;

    [Range(1.0f, 1000.0f)]
    public double Frequency = 200.0f;

    public Vector3 Position = Vector3.zero;

    public Vector3 Direction = Vector3.up;
}
