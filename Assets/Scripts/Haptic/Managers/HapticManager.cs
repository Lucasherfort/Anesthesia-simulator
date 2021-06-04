using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
#endif


public class HapticManager : MonoBehaviour
{
    [SerializeField]
    private HapticConfig hapticConfig = null;

    /// <summary>
    /// //////////////////// PROBE /////////////////////////////
    /// </summary>

    private string ProbeTagName;

    private HapticConfig.HLTOUCH_MODEL hlTouchModel = HapticConfig.HLTOUCH_MODEL.HL_CONTACT; 
    private HapticConfig.HLFACING hlTouchable = HapticConfig.HLFACING.HL_FRONT; 

    private bool Flip_Normals = false;  

    [Range(0.0f, 1.0f)]
    private float hlStiffness = 0.7f;  
    [Range(0.0f, 1.0f)]
    private float hlDamping = 0.1f;     
    [Range(0.0f, 1.0f)]
    private float hlStaticFriction = 0.2f;   
    [Range(0.0f, 1.0f)]
    private float hlDynamicFriction = 0.3f; 
    [Range(0.0f, 1.0f)]
    private float hlPopThrough = 0.0f;

    private float snapDistance = 1.0f; 

    private bool oldFlipNormals = false;
    private float oldStiffness = -1;
    private float oldDamping = -1;
    private float oldStaticFriction = -1;
    private float oldDynamicFriction = -1;
    private float oldSnapDistance = -1;
    private float oldPopThrough = -1;
    private HapticConfig.HLTOUCH_MODEL oldTouchModel = HapticConfig.HLTOUCH_MODEL.HL_CONTACT;
    private HapticConfig.HLFACING oldFacing = HapticConfig.HLFACING.HL_FRONT;

    /// <summary>
    /// //////////////////// NEEDLE /////////////////////////////
    /// </summary>

    private string NeedleTagName;
    private CustomHapticPlugin[] devices;
    private CustomHapticPlugin probeDevice;
    private CustomHapticPlugin needleDevice;
    private Vector3 position;
    private bool inContactwithNeedle = false;

    private float resistance = 0;

    private void Start()
    {
        SetupHapticConfig();

        if (GetComponent<MeshCollider>() == null && GetComponent<MeshFilter>() == null)
        {
            Debug.LogError("HapticSurface has been assigned to object without mesh.");
        }

        if (gameObject.tag == "Untagged")
            gameObject.tag = "Touchable";

        devices = (CustomHapticPlugin[])FindObjectsOfType(typeof(CustomHapticPlugin));

        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].tag == ProbeTagName)
            {
                probeDevice = devices[i];
            }
            else if (devices[i].tag == NeedleTagName)
            {
                needleDevice = devices[i];
            }
            else
            {
                Debug.LogWarning("The GameObject " + devices[i].name + " doesn't have a correct tag");
            }
        }

        probeDevice.shapesEnabled = true;
        needleDevice.shapesEnabled = true;
    }

    private void SetupHapticConfig()
    {
        ProbeTagName = hapticConfig.ProbeTagName;
        NeedleTagName = hapticConfig.NeedleTagProbe;

        hlTouchModel = hapticConfig.hlTouchModel;
        hlTouchable = hapticConfig.hlTouchable;
        hlStiffness = hapticConfig.hlStiffness;
        hlDamping = hapticConfig.hlDamping;
        hlStaticFriction = hapticConfig.hlStaticFriction;
        hlDynamicFriction = hapticConfig.hlDynamicFriction;
        hlPopThrough = hapticConfig.hlPopThrough;
        snapDistance = hapticConfig.snapDistance;

        resistance = hapticConfig.resistance;
    }

    private void Update()
    {
        SetupHapticConfig();

        bool needUpdate = false;

        if (hlStiffness != oldStiffness) needUpdate = true;
        if (hlDamping != oldDamping) needUpdate = true;
        if (hlStaticFriction != oldStaticFriction) needUpdate = true;
        if (hlDynamicFriction != oldDynamicFriction) needUpdate = true;
        if (hlPopThrough != oldPopThrough) needUpdate = true;
        if (snapDistance != oldSnapDistance) needUpdate = true;
        if (hlTouchModel != oldTouchModel) needUpdate = true;
        if (Flip_Normals != oldFlipNormals) needUpdate = true;
        if (hlTouchable != oldFacing) needUpdate = true;

        if (needUpdate)
        {
            CustomHapticPlugin.shape_settings(gameObject.GetInstanceID(), hlStiffness, hlDamping, hlStaticFriction, hlDynamicFriction, hlPopThrough);

            int M = 0;
            if (hlTouchModel == HapticConfig.HLTOUCH_MODEL.HL_CONSTRAINT)
                M = 1;

            CustomHapticPlugin.shape_constraintSettings(gameObject.GetInstanceID(), M, snapDistance);
            CustomHapticPlugin.shape_flipNormals(gameObject.GetInstanceID(), Flip_Normals);

            int T = 1;
            if (hlTouchable == HapticConfig.HLFACING.HL_BACK) T = 2;
            if (hlTouchable == HapticConfig.HLFACING.HL_FRONT_AND_BACK) T = 3;
            CustomHapticPlugin.shape_facing(gameObject.GetInstanceID(), T);

            oldStiffness = hlStiffness;
            oldDamping = hlDamping;
            oldStaticFriction = hlStaticFriction;
            oldDynamicFriction = hlDynamicFriction;
            oldTouchModel = hlTouchModel;
            oldSnapDistance = snapDistance;
            oldPopThrough = hlPopThrough;
            oldFlipNormals = Flip_Normals;
            oldFacing = hlTouchable;
        }

        if (inContactwithNeedle)
        {
            if (probeDevice != null)
            {
                probeDevice.shapesEnabled = false;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag(NeedleTagName))
        {
            position = collision.gameObject.transform.position;

            if (needleDevice.touchingDepth == 0)
            {
                if (probeDevice.touchingDepth > resistance)
                {
                    inContactwithNeedle = true;
                }
               
            }
            else
            {
                if (needleDevice.touchingDepth > resistance)
                {
                    inContactwithNeedle = true;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(NeedleTagName))
        {
            inContactwithNeedle = false;
            if (probeDevice != null)
            {
                probeDevice.shapesEnabled = true;
            }
        }
    }

    private void OnDestroy()
    {
        CustomHapticPlugin.shape_remove(gameObject.GetInstanceID());
    }
}
