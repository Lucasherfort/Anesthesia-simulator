using UnityEngine;
using System;
using HD;

public class TwoHapticsProbeNeedle : MonoBehaviour
{
    public static TwoHapticsProbeNeedle instance { get; private set; }

    //---------------------------------------------------------------------------
    // HAPTIC INFORMATION
    //---------------------------------------------------------------------------

    /// <summary>
    /// PHANTOM instance
    /// </summary>
    private PhantomUnityController Phantoms = null;

    /// <summary>
    /// Left device structure
    /// </summary>
    public PhantomDeviceInfo ProbeDevice;

    /// <summary>
    /// Right device structure
    /// </summary>
    public PhantomDeviceInfo NeedleDevice;

    /// <summary>
    /// Force feedback to apply to device
    /// </summary>
    private Vector3 ProbeForce = Vector3.zero;

    /// <summary>
    /// Force feedback to apply to device
    /// </summary>
    private Vector3 NeedleForce = Vector3.zero;

    /* PROBE PARAMETERS*/

    // Stiffnes, i.e.k value, of the plane.  Higher stiffness results
    // in a harder surface.
    public float FirstPlaneStiffness = 0.25f;
    public float SecondPlaneStiffness = 0.33f;

    public float FirstPlanePosition = 0;
    public float SecondPlanePosition = -1.5f;

    /* NEEDLE PARAMETERS AND STUFF */
    /// <summary>
    /// Mutex for thread safety for simulator state
    /// </summary>
    private readonly object _lockState = new object();

    /// <summary>
    /// Mutex for thread safety for position & rotation variables
    /// </summary>
    private readonly object _lock = new object();

    /// <summary>
    /// For determining X, Z restriction movements inside skin layers
    /// </summary>
    private const float OUTSIDE_POSITION = -999;

    /// <summary>
    /// Dimensions (half) of the tissue [mm] to know if it is inside the lateral boundaries
    /// </summary>
    [SerializeField]
    private Vector3 TISSUE_DIMENSIONS = new Vector3(27, 0, 20.25f);

    /// <summary>
    /// Table position during experimentation
    /// </summary>
    /// <remarks>Modify RigidObstacles position to have correct force feedback</remarks>
    [SerializeField]
    private float GROUND_LEVEL = -0.15f;

    /// <summary>
    /// Top position of first layer (Unity units)
    /// </summary>
    [SerializeField]
    private float FIRST_LAYER_TOP = 0.10f;

    /// <summary>
    /// X position of the tip when entering the skin layers
    /// </summary>
    [SerializeField]
    private float NeedleContactPositionX = OUTSIDE_POSITION;

    /// <summary>
    /// Z position of the tip when entering the skin layers
    /// </summary>
    [SerializeField]
    private float NeedleContactPositionZ = OUTSIDE_POSITION;

    /// <summary>
    /// The gimbal position [mm] stored when contact with first membrane (reseted when transpasing it)
    /// </summary>
    [SerializeField]
    private Vector3 contactPosition = Vector3.zero;

    /// <summary>
    /// Initial position of the starting point
    /// </summary>
    [SerializeField]
    private Vector3 StartPointPosition = new Vector3(0, 70, 0);

    /// <summary>
    /// Position of the tip before reaching table
    /// </summary>
    private Vector3 previousPosition;

    /// <summary>
    /// Stiffness coefficient for Skin Layer [N/m]
    /// </summary>
    [SerializeField]
    private float FirstLayerStiffness = 31.5f; //20

    /// <summary>
    /// Position of the tip when entering the skin layers
    /// </summary>
    private Vector3 lastPosDevice = Vector3.zero;

    /// <summary>
    /// Rotation of the tip when entering the skin layers
    /// </summary>
    private Quaternion lastRotDevice = Quaternion.identity;

    /// <summary>
    /// Rotation matrix of the needle
    /// </summary>
    private double[] RotationMatrix;

    /// <summary>
    /// Current position of needle
    /// </summary>
    private Vector3 NeedleCurrentPosition = Vector3.zero;

    /// <summary>
    /// Current rotation of needle
    /// </summary>
    private Quaternion NeedleCurrentRotation = Quaternion.identity;

    /// <summary>
    /// Stiffness force corresponding to first layer
    /// </summary>
    public float FirstLayerForceStiffness = 0f;

    /// <summary>
    /// Friction force corresponding to first layer
    /// </summary>
    public float FirstLayerForceFriction = 0f;

    /// <summary>
    /// Cutting force corresponding to first layer
    /// </summary>
    public float FirstLayerForceCutting = 0f;

    /// <summary>
    /// Stiffness force corresponding to second layer
    /// </summary>
    public float SecondLayerForceStiffness = 0f;

    /// <summary>
    /// Friction force corresponding to second layer
    /// </summary>
    public float SecondLayerForceFriction = 0f;

    /// <summary>
    /// Cutting force corresponding to second layer
    /// </summary>
    public float SecondLayerForceCutting = 0f;

    /// <summary>
    /// Dumping force corresponding to first & second layers
    /// </summary>
    public float FirstAndSecondForceDumping = 0f;

    /// <summary>
    /// Addition of forces in the Y direction
    /// </summary>
    public float forceTotalY = 0f;

    /// <summary>
    /// Membrane forces before traspasing the membrane
    /// </summary>
    public Vector3 membraneForce;

    /// <summary>
    /// Membrane forces before traspasing the membrane
    /// </summary>
    public Vector3 membraneForce2;

    /// <summary>
    /// Exert force scale
    /// </summary>
    [SerializeField]
    private const float DEVICE_FORCE_SCALE = 0.4f;

    /// <summary>
    /// Damping coefficient for Skin Layer [N/m]
    /// </summary>
    [SerializeField]
    private float FirstLayerDamping = 1.67f; //2.2

    /// <summary>
    /// Cutting coefficient for Skin Layer [N/m]
    /// </summary>
    [SerializeField]
    private float SkinLayerCutting = 1.22f; //1.8

    //---------------------------------------------------------------------------
    // SYSTEM CONSTANTS
    //---------------------------------------------------------------------------

    /// <summary>
    /// Unit conversion from mm to Unity
    /// </summary>
	[SerializeField]
    private float UnitLength = 0.01f;

    public Action ZoomUp;
    public Action ZoomDown;

    public Action InsertAnesthesic;

    // way to set up GetButtonDown function
    private Buttons lastLeftButtonsState;
    private Buttons lastRighttButtonsState;

    /// <summary>
    /// Runs only once when it is first activated
    /// </summary>
    private void Awake()
    {
        if (Phantoms == null)
        {
            try
            {
                Debug.Log("Initializing phantoms");

                // Instantiation of Phantoms
                Phantoms = new PhantomUnityController();

                try
                {
                    ProbeDevice.hHdAPI = Phantoms.InitDevice(ProbeDevice.Name);
                    NeedleDevice.hHdAPI = Phantoms.InitDevice(NeedleDevice.Name);
                }
                catch (UnityException)
                {
                    Phantoms = null;
                }
            }
            catch (UnityException)
            {
                Debug.Log("EXCEPTION >> Error trying to conect to PHANTOM devices.\nVerify connection and try again!");
            }
        }
    }

    /// <summary>
    /// When enabled
    /// </summary>
    private void OnEnable()
    {
        Init();

        Debug.Log("INITIALIZING DEVICE...");
        Phantoms.Start();

        // It specifies the method to be executed repeatedly
        Phantoms.AddSchedule(PhantomUpdate, HdAPI.Priority.HD_DEFAULT_SCHEDULER_PRIORITY);
    }

    /// <summary>
    /// When disabled
    /// </summary>
    private void OnDisable()
    {
        Debug.Log("CLOSING DEVICE...");
        try
        {
            if (Phantoms != null)
            {
                //Phantoms.exitHandler();
                Phantoms.Close();
                Phantoms = null;
                Debug.Log("DEVICES CLOSED");
            }
            else
                Debug.Log("DEVICES NOT CONNECTED");
        }
        catch (Exception e)
        {
            Debug.Log("EXCEPTION ON OnDisable");
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// Process at the start of the simulation
    /// </summary>
    private void Start()
    {

    }

    /// <summary>
    /// Initialization of the manager
    /// </summary>
    private void Init()
    {
        // Save singleton instance
        if (instance == null)
        {
            instance = this;
        }

        else
            Debug.Log("Multiple instances of HapticManager");
    }

    /// <summary>
    /// Process each frame
    /// </summary>
    private void Update()
    {
        // Mise à jour du visuel
        ProbeDevice.tool.transform.localPosition = ProbeDevice.position;
        ProbeDevice.tool.transform.localRotation = ProbeDevice.rotation;
        NeedleDevice.tool.transform.localPosition = NeedleDevice.position;
        NeedleDevice.tool.transform.localRotation = NeedleDevice.rotation;

        // Mise à jour des boutons 
        HdAPI.hdMakeCurrentDevice(ProbeDevice.hHdAPI);
        Buttons bStateLeft = Phantoms.GetButton();

        if (bStateLeft == Buttons.Button1 && lastLeftButtonsState != bStateLeft)
        {
            ZoomDown.Invoke();
        }

        if (bStateLeft == Buttons.Button2 && lastLeftButtonsState != bStateLeft)
        {
            ZoomUp.Invoke();
        }

        HdAPI.hdMakeCurrentDevice(NeedleDevice.hHdAPI);
        Buttons bStateRight = Phantoms.GetButton();
        if (bStateRight == Buttons.Button1 && lastRighttButtonsState != bStateRight)
        {
            InsertAnesthesic.Invoke();
        }

        lastLeftButtonsState = bStateLeft;
        lastRighttButtonsState = bStateRight;
    }

    public float probeDop = 0f;
    public float probeDopStiffness = 0f;

    Vector3 ProbeCurrentPosition;
    /// <summary>
    /// Method that is repeatedly called in PHANTOM's cycle (default rate 1 [kHz])
    /// </summary>
    /// <returns><c>true</c>, if update was phantomed, <c>false</c> otherwise.</returns>
    bool PhantomUpdate()
    {
        /* PROBE */

        HdAPI.hdBeginFrame(ProbeDevice.hHdAPI);

        Vector3 ProbeHandPosition = Phantoms.GetPosition();
        Quaternion ProbeHandRotation = Phantoms.GetRotation();

        // If the user has penetrated the plane, set the device force to 
        // repel the user in the direction of the surface normal of the plane.
        // Penetration occurs if the plane is facing in +Y and the user's Y position
        // is negative, or vice versa.
        if (Mathf.Abs(ProbeHandPosition.x) < TISSUE_DIMENSIONS.x && Mathf.Abs(ProbeHandPosition.z) < TISSUE_DIMENSIONS.z)
        {
            // get position from top 1st layer position
            ProbeCurrentPosition = ProbeHandPosition * UnitLength;
            float ProbeDopStiffness = FirstPlanePosition + 0.075f - ProbeCurrentPosition.y;

            if (ProbeDopStiffness > 0)
            {
                //---------------------------------------------------------------------------
                // MEMBRANE STIFFNESS FORCE (before penetration)
                //---------------------------------------------------------------------------

                Vector3 ProbeHandVelocity = Phantoms.GetVelocity();
                // get velocity and limit it
                float ProbeVelocity = ProbeHandVelocity.y;
                ProbeVelocity = Mathf.Clamp(ProbeVelocity, -0.1f, 0.1f);

                // calculate stiffness force (Y direction)
                float ProbeForceStiffness = (2.5f + FirstLayerStiffness) * ProbeDopStiffness + FirstLayerDamping * (-ProbeVelocity) * ProbeDopStiffness;

                // apply scale factor for forces
                ProbeForceStiffness *= DEVICE_FORCE_SCALE;

                float membraneDamping = 0.003f;
                float membraneStiffness = 0.04f;
                float distanceCoeficient = 0.08f;
                float ClampValue = 0.4f;

                // lateral forces within the membrane: damping force
                membraneForce2 = -membraneDamping * ProbeHandVelocity;
                if (membraneForce2.magnitude > ClampValue)
                {
                    membraneForce2.Normalize();
                    membraneForce2 *= ClampValue;
                }

                Vector3 ForceS = Vector3.zero;
                ForceS.x += membraneForce2.x - ProbeDopStiffness * distanceCoeficient;
                ForceS.y += membraneForce2.y;
                ForceS.z += membraneForce2.z - ProbeDopStiffness * distanceCoeficient; ;

                // lateral forces within the membrane: dynamic stiffness force
                ClampValue = (float)Phantoms.GetContinuousForceLimit();
                membraneForce2 = membraneStiffness * (Vector3.zero - ProbeHandPosition);
                if (membraneForce2.magnitude > ClampValue)
                {
                    membraneForce2.Normalize();
                    membraneForce2 *= ClampValue;
                }
                
                ForceS.x += membraneForce2.x;
                ForceS.y += membraneForce2.y;
                ForceS.z += membraneForce2.z;
                ProbeDevice.force = ForceS;
            }

            if (ProbeHandPosition.y <= FirstPlanePosition) //0 la pos en y du plane
            {
                // Create a force vector repelling the user from the plane proportional
                // to the penetration distance, using F=kx where k is the plane 
                // stiffness and x is the penetration vector.  Since the plane is 
                // oriented at the Y=0, the force direction is always either directly 
                // upward or downward, i.e. either (0,1,0) or (0,-1,0).
                // Hooke's law explicitly
                float penetrationDistance = Mathf.Abs(ProbeHandPosition.y);

                if (ProbeHandPosition.y > SecondPlanePosition)
                {
                    ProbeDevice.force += new Vector3(0, (float)(penetrationDistance * FirstPlaneStiffness), 0);
                    ProbeHandPosition *= UnitLength;
                    ProbeDevice.position = new Vector3(ProbeHandPosition.x, ProbeHandPosition.y, ProbeHandPosition.z);
                    ProbeDevice.rotation = new Quaternion(ProbeHandRotation.x, ProbeHandRotation.y, ProbeHandRotation.z, ProbeHandRotation.w);
                }
                else
                {
                    ProbeDevice.force += new Vector3(0, (float)(penetrationDistance * SecondPlaneStiffness), 0);
                    ProbeHandPosition *= UnitLength;
                    ProbeDevice.position = new Vector3(ProbeHandPosition.x, SecondPlanePosition * UnitLength, ProbeHandPosition.z);
                    ProbeDevice.rotation = new Quaternion(ProbeHandRotation.x, ProbeHandRotation.y, ProbeHandRotation.z, ProbeHandRotation.w);
                }
            }
            else
            {
                ProbeDevice.force += Vector3.zero;
                ProbeHandPosition *= UnitLength;
                ProbeDevice.position = new Vector3(ProbeHandPosition.x, ProbeHandPosition.y, ProbeHandPosition.z);
                ProbeDevice.rotation = new Quaternion(ProbeHandRotation.x, ProbeHandRotation.y, ProbeHandRotation.z, ProbeHandRotation.w);
            }
        }
        else
        {
            ProbeDevice.force = Vector3.zero;
            ProbeHandPosition *= UnitLength;
            ProbeDevice.position = new Vector3(ProbeHandPosition.x, ProbeHandPosition.y, ProbeHandPosition.z);
            ProbeDevice.rotation = new Quaternion(ProbeHandRotation.x, ProbeHandRotation.y, ProbeHandRotation.z, ProbeHandRotation.w);
        }

        HdAPI.hdMakeCurrentDevice(ProbeDevice.hHdAPI);
        Phantoms.SetForce(ProbeDevice.force);

        /***************************************************/

        /* NEEDLE */

        HdAPI.hdBeginFrame(NeedleDevice.hHdAPI);
        HdAPI.hdMakeCurrentDevice(NeedleDevice.hHdAPI);

        // Get the position of the hand (gimbal part) [mm]
        Vector3 NeedleHandPosition = Phantoms.GetPosition();

        // Get the hand posture (orientation)
        Quaternion NeedleHandRotation = NeedleDevice.correctionRotation = Phantoms.GetRotation();

        // Get the speed of the hand [mm/s]
        Vector3 NeedleHandVelocity = Phantoms.GetVelocity();

        // Re-init force feedback to 0
        //Vector3 Force = Vector3.zero;
        NeedleDevice.force = Vector3.zero;

        // Hand position & rotation in the Unity world
        Vector3 currentPosition = NeedleHandPosition * UnitLength;
        Quaternion currentRotation = NeedleHandRotation;

        // init forces to apply to haptic in the Y direction
        FirstLayerForceStiffness = FirstLayerForceFriction = FirstLayerForceCutting = forceTotalY = 0f;

        //---------------------------------------------------------------------------
        // FORCES FROM TISSUE - NEEDLE INTERACTION (1st and 2nd layer)
        //---------------------------------------------------------------------------

        // if within the square of tissue in X, Z coordinates (big cube with all tissue layers inside)
        if (Mathf.Abs(NeedleHandPosition.x) < TISSUE_DIMENSIONS.x && Mathf.Abs(NeedleHandPosition.z) < TISSUE_DIMENSIONS.z)
        {
            // get vertical position of the needle
            float NeedleVerticalPosition = NeedleHandPosition.y * UnitLength;

            // if it has traspased the membrane
            if (NeedleVerticalPosition < FIRST_LAYER_TOP)// - 0.05)
            {
                contactPosition = Vector3.zero;

                // set contact position, store position and rotation of needle at the moment of penetration
                if (NeedleContactPositionX == OUTSIDE_POSITION && NeedleContactPositionZ == OUTSIDE_POSITION)
                {
                    // POINT PIVOT !!!! 
                    NeedleContactPositionX = currentPosition.x;
                    NeedleContactPositionZ = currentPosition.z;
                    lastPosDevice = currentPosition;
                    lastRotDevice = NeedleHandRotation;

                    // get rotation matrix to get direction of the needle when penetrating

                    Phantoms.GetRotationMatrix(out RotationMatrix); // sortir de ce boucle je crois pour le point pivot

                    // needle is inside tissue
                    NeedleDevice.correctionPosition = new Vector3(NeedleContactPositionX, FIRST_LAYER_TOP, NeedleContactPositionZ);
                    NeedleDevice.inside = true;
                }
            }
            else
            {
                NeedleDevice.inside = false;
                NeedleContactPositionX = NeedleContactPositionZ = OUTSIDE_POSITION;
            }

            // init depth variables and velocity
            probeDop = 0f;
            probeDopStiffness = 0f;
            float NeedleVelocity = 0f;

            // limit visual direction if needle inside tissue and calculate lateral forces
            if (NeedleContactPositionX != OUTSIDE_POSITION && NeedleContactPositionZ != OUTSIDE_POSITION)
            {
                //---------------------------------------------------------------------------
                // INSIDE SKIN LAYERS LATERAL FORCES ADDITION
                //---------------------------------------------------------------------------

                currentRotation = lastRotDevice;
                float t = (currentPosition.y - lastPosDevice.y) / -(float)RotationMatrix[9];

                // Temporal variable to calculate the lateral forces to limit position
                Vector3 lateralInsideForce = Vector3.zero;
                lateralInsideForce.x = CalculateLateralForce(currentPosition, NeedleHandVelocity, lastPosDevice, 0).x;
                lateralInsideForce.z = CalculateLateralForce(currentPosition, NeedleHandVelocity, lastPosDevice, 2).z;

                // add lateral forces
                NeedleDevice.force += lateralInsideForce;

                //---------------------------------------------------------------------------

                // update current position with limits in the X and Z position
                currentPosition = new Vector3(t * -(float)RotationMatrix[8] + lastPosDevice.x, currentPosition.y, t * (float)RotationMatrix[10] + lastPosDevice.z);

                // depth in the skin from penetration point
                probeDop = (currentPosition - lastPosDevice).magnitude;
            }

            //---------------------------------------------------------------------------
            // FIRST LAYER FORCE ADDITION
            //---------------------------------------------------------------------------

            // limit depthness
            probeDop = Mathf.Clamp(probeDop, 0f, 0.35f);

            // get position from top 1st layer position
            probeDopStiffness = FIRST_LAYER_TOP + 0.05f - currentPosition.y;

            if (probeDopStiffness > 0 && probeDop == 0)
            {
                //---------------------------------------------------------------------------
                // MEMBRANE STIFFNESS FORCE (before penetration)
                //---------------------------------------------------------------------------

                if (contactPosition == Vector3.zero)
                    contactPosition = NeedleHandPosition;

                // get velocity and limit it
                NeedleVelocity = NeedleHandVelocity.y;
                NeedleVelocity = Mathf.Clamp(NeedleVelocity, -0.1f, 0.1f);

                // calculate stiffness force (Y direction)
                FirstLayerForceStiffness = (2.5f + FirstLayerStiffness) * probeDopStiffness + FirstLayerDamping * (-NeedleVelocity) * probeDopStiffness;

                // apply scale factor for forces
                FirstLayerForceStiffness *= DEVICE_FORCE_SCALE;
                forceTotalY = FirstLayerForceStiffness;

                float membraneDamping = 0.003f;
                float membraneStiffness = 0.04f;
                float distanceCoeficient = 0.08f;
                float ClampValue = 0.4f;

                // lateral forces within the membrane: damping force
                membraneForce = -membraneDamping * NeedleHandVelocity;
                if (membraneForce.magnitude > ClampValue)
                {
                    membraneForce.Normalize();
                    membraneForce *= ClampValue;
                }
                
                NeedleDevice.force.x += membraneForce.x - probeDopStiffness * distanceCoeficient;
                NeedleDevice.force.y += membraneForce.y;
                NeedleDevice.force.z += membraneForce.z - probeDopStiffness * distanceCoeficient; ;

                // lateral forces within the membrane: dynamic stiffness force
                ClampValue = (float)Phantoms.GetContinuousForceLimit();
                membraneForce = membraneStiffness * (contactPosition - NeedleHandPosition);
                if (membraneForce.magnitude > ClampValue)
                {
                    membraneForce.Normalize();
                    membraneForce *= ClampValue;
                }

                NeedleDevice.force += membraneForce;

                if ((contactPosition - NeedleHandPosition).magnitude > 5)
                    contactPosition = NeedleHandPosition;

                //---------------------------------------------------------------------------
            }
            else if (probeDop > 0 && NeedleVerticalPosition < FIRST_LAYER_TOP - 0.05)
            {
                //---------------------------------------------------------------------------
                // TISSUE FRICTION + CUTTING FORCE (after penetration)
                //---------------------------------------------------------------------------

                float f0 = 0.185f;
                float a0 = 0.12f;
                float b0 = -0.097f;

                // get velocity and limit it
                NeedleVelocity = NeedleHandVelocity.y * UnitLength;
                NeedleVelocity = Mathf.Clamp(NeedleVelocity, -1.5f, 1.5f);

                // calculate friction force
                FirstLayerForceFriction = (-NeedleVelocity * 3 + 800 * ((f0 + b0) * Mathf.Exp(a0 * probeDopStiffness) + b0)) / FirstLayerDamping;

                // apply scale factor for forces
                FirstLayerForceFriction *= DEVICE_FORCE_SCALE;

                // add cutting force (= constant)
                FirstLayerForceCutting = SkinLayerCutting;
                
                forceTotalY = FirstLayerForceFriction + FirstLayerForceCutting;
            }
            else
            {
                contactPosition = Vector3.zero;
            }

            // update calculated forces
            NeedleDevice.force.y += forceTotalY;
            //---------------------------------------------------------------------------
        }
        else
        {
            contactPosition = Vector3.zero;
            NeedleDevice.inside = false;

            // reset contact position
            NeedleContactPositionX = NeedleContactPositionZ = OUTSIDE_POSITION;
        }
        //---------------------------------------------------------------------------

        // Force feedback to PHANTOM device [N]
        Phantoms.SetForce(NeedleDevice.force);
        
        // set position and orientation for graphic needle
        NeedleDevice.position = currentPosition;
        NeedleDevice.rotation = currentRotation;
        
        previousPosition = NeedleDevice.position;
        
        HdAPI.hdEndFrame(NeedleDevice.hHdAPI);
        HdAPI.hdEndFrame(ProbeDevice.hHdAPI);

        return true;
    }

    /// <summary>
    /// Auxiliar function to calculate attraction force between devices
    /// </summary>
    /// <param name="pos">position difference between devices</param>
    /// <returns>the force to be applied to the haptic devices</returns>
    private Vector3 ForceField(Vector3 pos)
    {
        float dist = pos.magnitude;

        Vector3 forceVec = Vector3.zero;

        // if two charges overlap...
        if (dist < 24.0f)
        {
            // Attract the charge to the center of the sphere.
            forceVec = new Vector3(-0.1f * pos.x, -0.1f * pos.y, -0.1f * pos.z);
        }
        else
        {
            Vector3 unitPos = pos.normalized;
            forceVec = -1200.0f * unitPos / (dist * dist);
        }

        return forceVec;
    }

    /// <summary>
    /// Seek the forces generated when the operating point is in contact with the lateral membrane
    /// </summary>
    /// <param name="tipPosition">The position of the tip [mm]</param>
    /// <param name="tipVelocity">The speed of the tip [mm/s]</param>
    /// <param name="lateralPosition">The stored lateral position</param>
    /// <param name="axe">To determine if it is X (<code>0</code>) or Z (<code>2</code>)</param>
    /// <returns>The force to apply</returns>
    private Vector3 CalculateLateralForce(Vector3 tipPosition, Vector3 tipVelocity, Vector3 lateralPosition, int axe)
    {
        // local constants
        const float BOUNDARY = 0.0f;
        const float STIFFNESS = 25f;
        const float DUMPING = 0.0f;
        const float FORCE_LIMIT = 3.0f;

        // Calculate the difference from the tip to the object center
        Vector3 differencePositions = tipPosition - lateralPosition;

        // The distance to planar object is assumed in the Y axis
        float distance = differencePositions[axe];

        // No force in the outside of the BOUNDARY with object and no visual feedback
        if (distance == 0) return Vector3.zero;

        // No forces with other planes different from axe
        for (int i = 0; i < 3; i++)
            if (i != axe) differencePositions[i] = 0;

        // Normalisation
        differencePositions /= distance;

        // STIFFNESS force calculation
        float force = STIFFNESS * (BOUNDARY - distance);

        // Restrict to max force
        if (force > FORCE_LIMIT) force = FORCE_LIMIT;

        return (force * differencePositions) - DUMPING * tipVelocity;
    }
}
