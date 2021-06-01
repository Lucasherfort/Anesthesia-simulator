using UnityEngine;

public class HapticManager : MonoBehaviour
{
    [SerializeField]
    private HapticConfig hapticConfig = null;

    private HapticConfig.HLTOUCH_MODEL hlTouchModel = HapticConfig.HLTOUCH_MODEL.HL_CONTACT;

    private HapticConfig.HLFACING hlTouchable = HapticConfig.HLFACING.HL_FRONT;

    private HapticConfig.EFFECT_TYPE effectType = HapticConfig.EFFECT_TYPE.VISCOUS;

    private bool Flip_Normals = false;  

    private float hlStiffness;   
    private float hlDamping;
    private float hlStaticFriction;
    private float hlDynamicFriction;
    private float hlPopThrough;
    private float snapDistance; 

    private bool oldFlipNormals = false;
    private float oldStiffness = -1;
    private float oldDamping = -1;
    private float oldStaticFriction = -1;
    private float oldDynamicFriction = -1;
    private float oldSnapDistance = -1;
    private float oldPopThrough = -1;
    private HapticConfig.HLTOUCH_MODEL oldTouchModel = HapticConfig.HLTOUCH_MODEL.HL_CONTACT;
    private HapticConfig.HLFACING oldFacing = HapticConfig.HLFACING.HL_FRONT;

    private double Gain = 0.333f;
    private double Magnitude = 0.333f;
    private double Frequency = 200.0f;
    private Vector3 Position = Vector3.zero;
    private Vector3 Direction = Vector3.up;

    private HapticPlugin[] devices;
    private HapticPlugin ProbeDevice;
    private HapticPlugin NeedleDevice;

    private bool inTheZone;       
    private Vector3 devicePoint;  
    private float delta;          
    private int FXID;             

    private Vector3 focusPointWorld = Vector3.zero;
    private Vector3 directionWorld = Vector3.up;

    private bool inContactwithNeedle = false;

    private void Start()
    {
        SetupHapticConfig();

        devices = (HapticPlugin[])FindObjectsOfType(typeof(HapticPlugin));

        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].transform.tag == "Probe")
            {
                ProbeDevice = devices[i];
            }
            else if (devices[i].transform.tag == "Needle")
            {
                NeedleDevice = devices[i];
            }
        }

        ///////////// PROBE /////////////////
        if (GetComponent<MeshCollider>() == null && GetComponent<MeshFilter>() == null)
        {
            Debug.LogError("HapticSurface has been assigned to object without mesh.");
        }

        if (gameObject.tag == "Untagged")
        {
            gameObject.tag = "Touchable";
        }

        ///////////// NEEDLE /////////////////


        /*
        inTheZone = new bool[devices.Length];
        devicePoint = new Vector3[devices.Length];
        delta = new float[devices.Length];
        FXID = new int[devices.Length];

        for (int i = 0; i < devices.Length; i++)
        {
            inTheZone[i] = false;
            devicePoint[i] = Vector3.zero;
            delta[i] = 0.0f;
            FXID[i] = HapticPlugin.effects_assignEffect(devices[i].configName);               
        }

        ProbeDevice.shapesEnabled = true;
        NeedleDevice.shapesEnabled = true;

        */
    }

    private void Update()
    {
        SetupHapticConfig();
        ApplyHapticConfig();
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

        effectType = hapticConfig.effectType;
        Gain = hapticConfig.Gain;
        Magnitude = hapticConfig.Magnitude;
        Frequency = hapticConfig.Frequency;
        Position = hapticConfig.Position;
        Direction = hapticConfig.Direction;
    }

    private void ApplyHapticConfig()
    {
        ////////////////////////// PROBE ////////////////////////

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
            HapticPlugin.shape_settings(gameObject.GetInstanceID(), hlStiffness, hlDamping, hlStaticFriction, hlDynamicFriction, hlPopThrough);

            int M = 0;
            if (hlTouchModel == HapticConfig.HLTOUCH_MODEL.HL_CONSTRAINT)
                M = 1;

            HapticPlugin.shape_constraintSettings(gameObject.GetInstanceID(), M, snapDistance);
            HapticPlugin.shape_flipNormals(gameObject.GetInstanceID(), Flip_Normals);

            int T = 1;
            if (hlTouchable == HapticConfig.HLFACING.HL_BACK) T = 2;
            if (hlTouchable == HapticConfig.HLFACING.HL_FRONT_AND_BACK) T = 3;
            HapticPlugin.shape_facing(gameObject.GetInstanceID(), T);

            oldStiffness = hlStiffness;
            oldDamping = hlDamping;
            oldStaticFriction = hlStaticFriction;
            oldDynamicFriction = hlDynamicFriction;
            oldTouchModel = hlTouchModel;
            oldSnapDistance = snapDistance;
            oldPopThrough = hlPopThrough;
            oldFlipNormals = Flip_Normals;
            oldFacing = hlTouchable;

            ////////////////////////// NEEDLE ////////////////////////

            /*
            Collider collider = gameObject.GetComponent<Collider>();
            if (collider == null)
            {
                Debug.LogError("This Haptic Effect Zone requires a collider");
                return;
            }

            focusPointWorld = transform.TransformPoint(Position);
            directionWorld = transform.TransformDirection(Direction);

            for (int ii = 0; ii < devices.Length; ii++)
            {
                HapticPlugin device = devices[ii];
                bool oldInTheZone = inTheZone[ii];
                int ID = FXID[ii];

                if (ID == -1)
                {
                    FXID[ii] = HapticPlugin.effects_assignEffect(devices[ii].configName);
                    ID = FXID[ii];

                    if (ID == -1)
                    {
                        Debug.LogError("Unable to assign Haptic effect.");
                        continue;
                    }
                }

                Vector3 StylusPos = device.stylusPositionWorld;
                Vector3 CP = collider.ClosestPoint(StylusPos);
                devicePoint[ii] = CP;
                delta[ii] = (CP - StylusPos).magnitude;

                if (delta[ii] <= Mathf.Epsilon)
                {
                    inTheZone[ii] = true;

                    Vector3 focalPointDevLocal = device.transform.InverseTransformPoint(focusPointWorld);
                    Vector3 rotationDevLocal = device.transform.InverseTransformDirection(directionWorld);
                    double[] pos = { focalPointDevLocal.x, focalPointDevLocal.y, focalPointDevLocal.z };
                    double[] dir = { rotationDevLocal.x, rotationDevLocal.y, rotationDevLocal.z };

                    double Mag = Magnitude;

                    if (device.isInSafetyMode())
                        Mag = 0;

                    HapticPlugin.effects_settings(
                        device.configName,
                        ID,
                        Gain,
                        Mag,
                        Frequency,
                        pos,
                        dir);
                    HapticPlugin.effects_type(
                        device.configName,
                        ID,
                        (int)effectType);

                }
                else
                {
                    inTheZone[ii] = false;
                }

                if (oldInTheZone != inTheZone[ii])
                {
                    if (inTheZone[ii])
                    {
                        HapticPlugin.effects_startEffect(device.configName, ID);
                    }
                    else
                    {
                        HapticPlugin.effects_stopEffect(device.configName, ID);
                    }
                }

            }
             */

            if (inContactwithNeedle)
            {
               NeedleDevice.shapesEnabled = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        inContactwithNeedle = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        inContactwithNeedle = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        //if (collision.collider.gameObject.tag != "Probe")
        //{
            //Debug.LogError("This Haptic Effect Zone requires a collider");
            //return;
        //}

        Debug.Log(collision.gameObject.tag);

        focusPointWorld = transform.TransformPoint(Position);
        directionWorld = transform.TransformDirection(Direction);

        bool oldInTheZone = inTheZone;
        int ID = FXID;

        if (ID == -1)
        {
            FXID = HapticPlugin.effects_assignEffect(NeedleDevice.configName);
            ID = FXID;

            if (ID == -1)
            {
                Debug.LogError("Unable to assign Haptic effect.");
                return;
            }
        }

        Vector3 StylusPos = NeedleDevice.stylusPositionWorld;
        Vector3 CP = GetComponent<Collider>().ClosestPoint(StylusPos);
        devicePoint = CP;
        delta = (CP - StylusPos).magnitude;

        if (delta <= Mathf.Epsilon)
        {
            inTheZone = true;

            Vector3 focalPointDevLocal = NeedleDevice.transform.InverseTransformPoint(focusPointWorld);
            Vector3 rotationDevLocal = NeedleDevice.transform.InverseTransformDirection(directionWorld);
            double[] pos = { focalPointDevLocal.x, focalPointDevLocal.y, focalPointDevLocal.z };
            double[] dir = { rotationDevLocal.x, rotationDevLocal.y, rotationDevLocal.z };

            double Mag = Magnitude;

            if (NeedleDevice.isInSafetyMode())
               Mag = 0;

            HapticPlugin.effects_settings(NeedleDevice.configName,ID,Gain,Mag,Frequency,pos,dir);
            HapticPlugin.effects_type(NeedleDevice.configName,ID,(int)effectType);
        }
        else
        {
            inTheZone = false;
        }

        if (oldInTheZone != inTheZone)
        {
            if (inTheZone)
            {
                HapticPlugin.effects_startEffect(NeedleDevice.configName, ID);
            }
            else
            {
                HapticPlugin.effects_stopEffect(NeedleDevice.configName, ID);
            }
        }
    }

    private void OnDestroy()
    {
        HapticPlugin.shape_remove(gameObject.GetInstanceID());

        if (NeedleDevice == null)
            return;

        int ID = FXID;
        HapticPlugin.effects_stopEffect(NeedleDevice.configName, ID);
    }

    private void OnDisable()
    {
        if (NeedleDevice == null)
            return;

        int ID = FXID;
        HapticPlugin.effects_stopEffect(NeedleDevice.configName, ID);
        inTheZone = false;
    }
}
