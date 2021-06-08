using UnityEngine;

[CreateAssetMenu(fileName = "HapticConfig", menuName = "Haptic/HapticConfig", order = 1)]
public class HapticConfig : ScriptableObject
{
    /// <summary>
    /// //////////////////// PROBE /////////////////////////////
    /// </summary>

    public enum HLTOUCH_MODEL { HL_CONTACT, HL_CONSTRAINT };
    public enum HLFACING { HL_FRONT, HL_BACK, HL_FRONT_AND_BACK };

    public HLTOUCH_MODEL hlTouchModel = HLTOUCH_MODEL.HL_CONTACT;  

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

    /// <summary>
    /// //////////////////// NEEDLE /////////////////////////////
    /// </summary>

    public float resistance = 1;
}
