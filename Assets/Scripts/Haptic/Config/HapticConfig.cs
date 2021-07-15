using UnityEngine;

[CreateAssetMenu(fileName = "HapticConfig", menuName = "Haptic/HapticConfig", order = 1)]
public class HapticConfig : ScriptableObject
{
    public float FirstPlanePosition = 0;
    public float SecondPlanePosition = -1.5f;

    public float FirstPlaneStiffness = 0.25f;
    public float SecondPlaneStiffness = 0.33f;

    public Vector3 TISSUE_DIMENSIONS = new Vector3(27, 0, 20.25f);

    public float GROUND_LEVEL = -0.15f;

    public float FIRST_LAYER_TOP = 0.10f;

    public Vector3 contactPosition = Vector3.zero;

    public Vector3 StartPointPosition = new Vector3(0, 70, 0);

    public float FirstLayerStiffness = 31.5f;


    public float DEVICE_FORCE_SCALE = 0.4f;

    public float FirstLayerDamping = 1.67f; 

    public float SkinLayerCutting = 1.22f; 

}