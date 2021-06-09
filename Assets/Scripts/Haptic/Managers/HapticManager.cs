using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
#endif


public class HapticManager : MonoBehaviour
{
    [SerializeField]
    private HapticConfig hapticConfig = null;

    static public HapticManager Instance { get; private set; }



    /// <summary>
    /// //////////////////// PROBE /////////////////////////////
    /// </summary>

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

    private ProbeHapticPlugin probeDevice;
    private NeedleHapticPlugin needleDevice;
    private Vector3 position;

    [HideInInspector]
    public bool NeedleTouchSkin = false;

    private float resistance = 0;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SetupHapticConfig();

        if (GetComponent<MeshCollider>() == null && GetComponent<MeshFilter>() == null)
        {
            Debug.LogError("HapticSurface has been assigned to object without mesh.");
        }

        if (gameObject.tag != "Touchable")
        {
            Debug.LogError(transform.name+" doesn't have the tag Touchable !");
        }

        probeDevice = (ProbeHapticPlugin)FindObjectOfType(typeof(ProbeHapticPlugin));
        needleDevice = (NeedleHapticPlugin)FindObjectOfType(typeof(NeedleHapticPlugin));

        if (probeDevice == null)
            Debug.LogError("probeDevice is missing !");

        if (needleDevice == null)
            Debug.LogError("needleDevice is missing !");

        probeDevice.shapesEnabled = true;
        needleDevice.shapesEnabled = true;
    }

    private void SetupHapticConfig()
    {
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
            ProbeHapticPlugin.shape_settings(gameObject.GetInstanceID(), hlStiffness, hlDamping, hlStaticFriction, hlDynamicFriction, hlPopThrough);

            int M = 0;
            if (hlTouchModel == HapticConfig.HLTOUCH_MODEL.HL_CONSTRAINT)
                M = 1;

            ProbeHapticPlugin.shape_constraintSettings(gameObject.GetInstanceID(), M, snapDistance);
            ProbeHapticPlugin.shape_flipNormals(gameObject.GetInstanceID(), Flip_Normals);

            int T = 1;
            if (hlTouchable == HapticConfig.HLFACING.HL_BACK) T = 2;
            if (hlTouchable == HapticConfig.HLFACING.HL_FRONT_AND_BACK) T = 3;
            ProbeHapticPlugin.shape_facing(gameObject.GetInstanceID(), T);

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

        if (NeedleTouchSkin)
        {
            if (probeDevice != null)
            {
                probeDevice.shapesEnabled = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Needle"))
        {
            // Setup Parent

            // TODO

            needleDevice.PivotManipulator.transform.position = collision.transform.position;
            needleDevice.PivotManipulator.transform.rotation = collision.transform.rotation;

            needleDevice.hapticManipulator.transform.parent = null;
            needleDevice.hapticManipulator.transform.SetParent(needleDevice.PivotManipulator.transform);

        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Needle"))
        {
            position = collision.gameObject.transform.position;

            if (needleDevice.touchingDepth == 0)
            {
                if (probeDevice.touchingDepth > resistance)
                {
                    NeedleTouchSkin = true;
                }               
            }
            else
            {
                if (needleDevice.touchingDepth > resistance)
                {
                    NeedleTouchSkin = true;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Needle"))
        {
            
            // Setup Parent
            needleDevice.hapticManipulator.transform.parent = null;
            needleDevice.hapticManipulator.transform.SetParent(needleDevice.PivotManipulator.transform.parent);


            NeedleTouchSkin = false;
            if (probeDevice != null)
            {
                probeDevice.shapesEnabled = true;
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        ProbeHapticPlugin.shape_remove(gameObject.GetInstanceID());
    }
}
