
//---------------------------------------------------------------------------
// INCLUDES
//---------------------------------------------------------------------------
using UnityEngine;
using System;
using HD;
using System.Collections.Generic;

//---------------------------------------------------------------------------
// HAPTIC MANAGER
//---------------------------------------------------------------------------


public enum SimulationState : int
{
    SIMULATION_START = -1,
    SIMULATION_END = -2,
    SIMULATION_OFF = 0,
    SIMULATION_ON = 1,
    SIMULATION_START_POINT = 2,
    SIMULATION_IN_PROGRESS = 6,
    TARGET_REACHED = 3,
    SAVING_DATA = 4,
    DATA_SAVED = 5,
    TRAINING_OFF = 0,
    TRAINING_BUTTON = 3,
    TRAINING_FAMILIARIZATION = 2,
    TRAINING_START = 1,
    TRAINING_ON = -1,
    FREE_MANIPULATION = 7,
    SEARCHING_TARGET = 6,
    WAIT_TO_START_BUTTON = 5,
    TECHNICAL_PROBLEM = 4,
    NEEDLE_FEEDBACK_ON = 2,
    NEEDLE_FEEDBACK_OFF = 8,
    TURN_OFF = -10,
    FREEZE_POSITION = 16,
    SWITCH_TO_VERIFICATION_MODE = 17,
    MODE_NEW = 18
}


/// <summary>
/// Needle Insertion Prototype
/// </summary>
public class TwoHapticsProbeNeedle : MonoBehaviour
{
    //---------------------------------------------------------------------------
    // CLASS INSTANCE
    //---------------------------------------------------------------------------

    /// <summary>
    /// class instance object -singleton-
    /// </summary>
	public static TwoHapticsProbeNeedle instance;

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
    public Vector3 Force_Left = Vector3.zero;

    /// <summary>
    /// Force feedback to apply to device
    /// </summary>
    public Vector3 Force_Right = Vector3.zero;

    /* PROBE PARAMETERS*/

    // Stiffnes, i.e.k value, of the plane.  Higher stiffness results
    // in a harder surface.
    public double firstPlaneStiffness = .25;
    public double secondPlaneStiffness = .33;

    public float positionFirstPlane = 0, positionSecondPlane = -5;

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
    /// Simulator state: haptic state
    /// </summary>
    public SimulationState _hapticState = SimulationState.SIMULATION_OFF;

    /// <summary>
    /// State variable for haptic thread
    /// </summary>
    public SimulationState _state = SimulationState.SIMULATION_OFF;

    /// <summary>
    /// X position of the tip when entering the skin layers
    /// </summary>
    private float contactPositionX = -999;

    /// <summary>
    /// Z position of the tip when entering the skin layers
    /// </summary>
    private float contactPositionZ = -999;

    /// <summary>
    /// The gimbal position [mm] stored when contact with first membrane (reseted when transpasing it)
    /// </summary>
    public Vector3 contactPosition = Vector3.zero;

    /// <summary>
    /// Initial position of the starting point
    /// </summary>
    private Vector3 StartPointPosition = new Vector3(0, 70, 0);

    /// <summary>
    /// Position of the tip before reaching table
    /// </summary>
    private Vector3 previousPosition;

    /// <summary>
    /// Stiffness coefficient for Skin Layer [N/m]
    /// </summary>
    [SerializeField]
    private float kStiffness1stLayerHaptic = 31.5f; //20

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
    private Vector3 MyPosition = Vector3.zero;

    /// <summary>
    /// Current rotation of needle
    /// </summary>
    private Quaternion MyRotation = Quaternion.identity;

    /// <summary>
    /// Stiffness force corresponding to first layer
    /// </summary>
    public float forceStiffness1 = 0f;

    /// <summary>
    /// Friction force corresponding to first layer
    /// </summary>
    public float forceFriction1 = 0f;

    /// <summary>
    /// Cutting force corresponding to first layer
    /// </summary>
    public float forceCutting1 = 0f;

    /// <summary>
    /// Stiffness force corresponding to second layer
    /// </summary>
    public float forceStiffness2 = 0f;

    /// <summary>
    /// Friction force corresponding to second layer
    /// </summary>
    public float forceFriction2 = 0f;

    /// <summary>
    /// Cutting force corresponding to second layer
    /// </summary>
    public float forceCutting2 = 0f;

    /// <summary>
    /// Dumping force corresponding to first & second layers
    /// </summary>
    public float forceDumping12 = 0f;

    /// <summary>
    /// Addition of forces in the Y direction
    /// </summary>
    public float forceTotalY = 0f;

    /// <summary>
    /// Membrane forces before traspasing the membrane
    /// </summary>
    private Vector3 membraneForce;

    /// <summary>
    /// Top position of first layer (Unity units)
    /// </summary>
    private const float FIRST_LAYER_TOP = 0.30f;

    /// <summary>
    /// Exert force scale
    /// </summary>
    private const float DEVICE_FORCE_SCALE = 0.4f;

    /// <summary>
    /// Damping coefficient for Skin Layer [N/m]
    /// </summary>
    [SerializeField]
    private float kDamping1stLayerHaptic = 1.67f; //2.2

    /// <summary>
    /// Cutting coefficient for Skin Layer [N/m]
    /// </summary>
    [SerializeField]
    private float kCutting1stLayerHaptic = 1.22f; //1.8

    //---------------------------------------------------------------------------
    // SYSTEM CONSTANTS
    //---------------------------------------------------------------------------

    /// <summary>
    /// Unit conversion from mm to Unity
    /// </summary>
	public float UnitLength = 0.01f;

    public Action ZoomUp;
    public Action ZoomDown;

    // way to set up GetButtonDown function
    private Buttons lastLeftButtonsState;

    //---------------------------------------------------------------------------
    // OBJECT ATTRIBUTES
    //---------------------------------------------------------------------------

    //---------------------------------------------------------------------------
    // FUNCTIONS
    //---------------------------------------------------------------------------

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
        //Phantoms.Do(PhantomUpdatePositions);

        // MAJ du visuel
        ProbeDevice.tool.transform.localPosition = ProbeDevice.position;
        ProbeDevice.tool.transform.localRotation = ProbeDevice.rotation;
        NeedleDevice.tool.transform.localPosition = NeedleDevice.position;
        NeedleDevice.tool.transform.localRotation = NeedleDevice.rotation;

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

        lastLeftButtonsState = bStateLeft;
    }

    bool PhantomUpdatePositions()
    {
        HdAPI.hdMakeCurrentDevice(ProbeDevice.hHdAPI);
        ProbeDevice.position = Phantoms.GetPosition();
        ProbeDevice.rotation = Phantoms.GetRotation();

        HdAPI.hdMakeCurrentDevice(NeedleDevice.hHdAPI);
        NeedleDevice.position = Phantoms.GetPosition();
        NeedleDevice.rotation = Phantoms.GetRotation();

        return false;
    }
    
    /// <summary>
    /// Method that is repeatedly called in PHANTOM's cycle (default rate 1 [kHz])
    /// </summary>
    /// <returns><c>true</c>, if update was phantomed, <c>false</c> otherwise.</returns>
    bool PhantomUpdate()
    {

        //HdAPI.hdBeginFrame(ProbeDevice.hHdAPI);
        //HandPosition_Left = Phantoms.GetPosition();

        //HdAPI.hdBeginFrame(NeedleDevice.hHdAPI);
        //HandPosition_Right = Phantoms.GetPosition();

        //Vector3 pos_diff = new Vector3(HandPosition_Left.x - HandPosition_Right.x, HandPosition_Left.y - HandPosition_Right.y, HandPosition_Left.z - HandPosition_Right.z);
        //ProbeDevice.force = ForceField(pos_diff);

        //HdAPI.hdMakeCurrentDevice(ProbeDevice.hHdAPI);
        //Phantoms.SetForce(ProbeDevice.force);
        //HdAPI.hdEndFrame(ProbeDevice.hHdAPI);

        //NeedleDevice.force = -1.0f * ProbeDevice.force;
        //HdAPI.hdMakeCurrentDevice(NeedleDevice.hHdAPI);
        //Phantoms.SetForce(NeedleDevice.force);
        //HdAPI.hdEndFrame(NeedleDevice.hHdAPI);

        /* PROBE */

        HdAPI.hdBeginFrame(ProbeDevice.hHdAPI);

        Vector3 HandPosition_Left = Phantoms.GetPosition();
        Quaternion HandRotation_Left = Phantoms.GetRotation();

        // If the user has penetrated the plane, set the device force to 
        // repel the user in the direction of the surface normal of the plane.
        // Penetration occurs if the plane is facing in +Y and the user's Y position
        // is negative, or vice versa.
        if (HandPosition_Left.y <= positionFirstPlane) //0 la pos en y du plane
        {
            // Create a force vector repelling the user from the plane proportional
            // to the penetration distance, using F=kx where k is the plane 
            // stiffness and x is the penetration vector.  Since the plane is 
            // oriented at the Y=0, the force direction is always either directly 
            // upward or downward, i.e. either (0,1,0) or (0,-1,0).
            // Hooke's law explicitly
            float penetrationDistance = Mathf.Abs(HandPosition_Left[1]);

            if (HandPosition_Left.y > positionSecondPlane)
            {
                ProbeDevice.force = new Vector3(0, (float)(penetrationDistance * firstPlaneStiffness), 0);
                HandPosition_Left = HandPosition_Left * UnitLength;
                ProbeDevice.position = new Vector3(HandPosition_Left.x, HandPosition_Left.y, HandPosition_Left.z);
                ProbeDevice.rotation = new Quaternion(HandRotation_Left.x, HandRotation_Left.y, HandRotation_Left.z, HandRotation_Left.w);
            }
            else
            {
                ProbeDevice.force = new Vector3(0, (float)(penetrationDistance * secondPlaneStiffness), 0);
                HandPosition_Left = HandPosition_Left * UnitLength;
                ProbeDevice.position = new Vector3(HandPosition_Left.x, positionSecondPlane * UnitLength, HandPosition_Left.z);
                ProbeDevice.rotation = new Quaternion(HandRotation_Left.x, HandRotation_Left.y, HandRotation_Left.z, HandRotation_Left.w);
            }
            HdAPI.hdMakeCurrentDevice(ProbeDevice.hHdAPI);
            Phantoms.SetForce(ProbeDevice.force);
        }
        else
        {
            ProbeDevice.force = Vector3.zero;
            HandPosition_Left = HandPosition_Left * UnitLength;
            ProbeDevice.position = new Vector3(HandPosition_Left.x, HandPosition_Left.y, HandPosition_Left.z);
            ProbeDevice.rotation = new Quaternion(HandRotation_Left.x, HandRotation_Left.y, HandRotation_Left.z, HandRotation_Left.w);
        }

        /***************************************************/

        /* NEEDLE */

        HdAPI.hdBeginFrame(NeedleDevice.hHdAPI);
        HdAPI.hdMakeCurrentDevice(NeedleDevice.hHdAPI);

        Vector3 HandPosition_Right = Phantoms.GetPosition();

        // Get the position of the hand (gimbal part) [mm]
        Vector3 HandPosition = Phantoms.GetPosition();

        // Get the hand posture (orientation)
        Quaternion HandRotation = Phantoms.GetRotation();

        // Get the speed of the hand [mm/s]
        Vector3 HandVelocity = Phantoms.GetVelocity();

        // Re-init force feedback to 0
        Vector3 Force = Vector3.zero;

        // get actual simulation state
        lock (_lockState)
            _state = _hapticState;

        // SIMULATION ON - waiting for starting point
        if (_state == SimulationState.SIMULATION_ON)
        {
            // distance to the starting point
            float distanceToStartPoint = Mathf.Abs(StartPointPosition.y + 10 - HandPosition.y);

            // Needle at starting point
            if (distanceToStartPoint < 2f)
            {
                Debug.Log("Starting point met");

                // notify simulator
                //SimulationModule.StartTrial();

                // update simulation state
                lock (_lockState)
                    _hapticState = SimulationState.NEEDLE_FEEDBACK_ON;

                // reset forces
                Phantoms.SetForce(Force);

                HdAPI.hdEndFrame(NeedleDevice.hHdAPI);
                HdAPI.hdEndFrame(ProbeDevice.hHdAPI);
                return true;
            }

            // update force feedback around starting ball
            float ballStiffness = 1.5f;
            float clampValue = 1.2f;

            Vector3 ballForces = ballStiffness * (StartPointPosition - HandPosition);
            if (ballForces.magnitude > clampValue)
            {
                ballForces.Normalize();
                ballForces *= clampValue;
            }

            // update force
            Force += ballForces;

            // update position & orientation
            lock (_lock)
            {
                // set position and orientation for graphic needle
                MyPosition = HandPosition * UnitLength;
                MyRotation = HandRotation;

                // If it is below table position -> set it back to ground level
                if (HandPosition.y * UnitLength < -.30f && previousPosition != Vector3.zero)
                    MyPosition = previousPosition;
                else
                    previousPosition = MyPosition;
            }

            // set ball resistance forces
            Phantoms.SetForce(Force);

            HdAPI.hdEndFrame(NeedleDevice.hHdAPI);
            HdAPI.hdEndFrame(ProbeDevice.hHdAPI);
            return true;
        }

        // Hand position & rotation in the Unity world
        //Vector3 
        Vector3 currentPosition = HandPosition * UnitLength;
        Quaternion currentRotation = HandRotation;

        // init forces to apply to haptic in the Y direction
        float forceStiffness1, forceFriction1, forceCutting1, forceTotalY = 0f;

        //---------------------------------------------------------------------------
        // FORCES FROM TISSUE - NEEDLE INTERACTION (1st and 2nd layer)
        //---------------------------------------------------------------------------

        // TODO: 200 is tissue x dimension !!!!!!
        // if within the square of tissue in X, Z coordinates (big cube with all tissue layers inside)
        if (Mathf.Abs(HandPosition.x) < 200 && Mathf.Abs(HandPosition.z) < 200)
        {
            // get vertical position of the needle
            float verticalPosition = HandPosition.y * UnitLength;

            // if it has traspased the membrane
            if (verticalPosition < 0.30f - 0.05)
            {
                contactPosition = Vector3.zero;

                // set contact position, store position and rotation of needle at the moment of penetration
                if (contactPositionX == -999 && contactPositionZ == -999)
                {
                    contactPositionX = currentPosition.x;
                    contactPositionZ = currentPosition.z;
                    lastPosDevice = currentPosition;
                    lastRotDevice = HandRotation;

                    // get rotation matrix to get direction of the needle when penetrating

                    Phantoms.GetRotationMatrix(out RotationMatrix);

                    // needle is inside tissue
                    //SimulationModule.NeedleToSkinPositionChanged(true);
                }
            }
            else
            {
                // reset contact position
                //if (contactPositionX != OUTSIDE_POSITION)
                //{
                //    SimulationModule.NeedleToSkinPositionChanged(false);
                //}
                contactPositionX = contactPositionZ = -999;
            }

            // init depth variables and velocity
            float probeDop = 0f;
            float probeDopStiffness = 0f;
            float velocity = 0f;

            // limit visual direction if needle inside tissue and calculate lateral forces
            if (contactPositionX != -999 && contactPositionZ != -999)
            {
                //---------------------------------------------------------------------------
                // INSIDE SKIN LAYERS LATERAL FORCES ADDITION
                //---------------------------------------------------------------------------

                currentRotation = lastRotDevice;
                float t = (currentPosition.y - lastPosDevice.y) / -(float)RotationMatrix[9];

                // Temporal variable to calculate the lateral forces to limit position
                Vector3 lateralInsideForce = Vector3.zero;
                lateralInsideForce.x = CalculateLateralForce(currentPosition, HandVelocity, lastPosDevice, 0).x;
                lateralInsideForce.z = CalculateLateralForce(currentPosition, HandVelocity, lastPosDevice, 2).z;

                // add lateral forces
                Force += lateralInsideForce;

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
            probeDopStiffness = 0.30f + 0.125f - currentPosition.y;

            if (probeDopStiffness > 0 && probeDop == 0)
            {
                //---------------------------------------------------------------------------
                // MEMBRANE STIFFNESS FORCE (before penetration)
                //---------------------------------------------------------------------------

                if (contactPosition == Vector3.zero)
                    contactPosition = HandPosition;

                // get velocity and limit it
                velocity = HandVelocity.y;
                velocity = Mathf.Clamp(velocity, -0.1f, 0.1f);

                // calculate stiffness force (Y direction)
                forceStiffness1 = (2.5f + kStiffness1stLayerHaptic) * probeDopStiffness + kDamping1stLayerHaptic * (-velocity) * probeDopStiffness;

                // apply scale factor for forces
                forceStiffness1 = forceStiffness1 * DEVICE_FORCE_SCALE;
                forceTotalY = forceStiffness1;

                float membraneDamping = 0.003f;
                float membraneStiffness = 0.04f;
                float distanceCoeficient = 0.08f;
                float ClampValue = 0.4f;

                // lateral forces within the membrane: damping force
                membraneForce = -membraneDamping * HandVelocity;
                if (membraneForce.magnitude > ClampValue)
                {
                    membraneForce.Normalize();
                    membraneForce *= ClampValue;
                }

                //Force += membraneForce;
                Force.x += membraneForce.x - probeDopStiffness * distanceCoeficient;
                Force.y += membraneForce.y;
                Force.z += membraneForce.z - probeDopStiffness * distanceCoeficient; ;

                // lateral forces within the membrane: dynamic stiffness force
                ClampValue = (float)Phantoms.GetContinuousForceLimit();
                membraneForce = membraneStiffness * (contactPosition - HandPosition);
                if (membraneForce.magnitude > ClampValue)
                {
                    membraneForce.Normalize();
                    membraneForce *= ClampValue;
                }

                //Force += membraneForce;
                Force.x += membraneForce.x;
                Force.y += membraneForce.y;
                Force.z += membraneForce.z;

                if ((contactPosition - HandPosition).magnitude > 5)
                    contactPosition = HandPosition;

                //---------------------------------------------------------------------------
            }
            else if (probeDop > 0 && verticalPosition < FIRST_LAYER_TOP - 0.05)
            {
                //---------------------------------------------------------------------------
                // TISSUE FRICTION + CUTTING FORCE (after penetration)
                //---------------------------------------------------------------------------

                float f0 = 0.185f;
                float a0 = 0.12f;
                float b0 = -0.097f;

                // get velocity and limit it
                velocity = HandVelocity.y * UnitLength;
                velocity = Mathf.Clamp(velocity, -1.5f, 1.5f);

                // calculate friction force
                forceFriction1 = (-velocity * 3 + 800 * ((f0 + b0) * Mathf.Exp(a0 * probeDopStiffness) + b0)) / kDamping1stLayerHaptic;

                // apply scale factor for forces
                forceFriction1 = forceFriction1 * DEVICE_FORCE_SCALE;

                // add cutting force (= constant)
                forceCutting1 = kCutting1stLayerHaptic;

                //---------------------------------------------------------------------------

                //---------------------------------------------------------------------------
                // CYST TARGET MEMBRANE STIFFNESS FORCE
                //---------------------------------------------------------------------------
                //distToCenter_Gameobjects = (currentPosition - target_position).magnitude;

                //if (distToCenter_Gameobjects < (sc + 0.025))
                //{
                //    if (distToCenter_Gameobjects < (sc - 0.025))
                //    {
                //        forceTarget = 0;
                //    }
                //    else
                //    {
                //        if (currentPosition.y < target_position.y)
                //        {
                //            debug = 1 / (100 * Mathf.Abs(distToCenter_Gameobjects - (sc + 0.015f)));
                //            forceTarget = debug > 1.0f ? 1.0f : debug;
                //        }
                //        else
                //        {
                //            debug = 1 / (100 * Mathf.Abs(distToCenter_Gameobjects - (sc - 0.015f)));
                //            forceTarget = debug > 1.0f ? 1.0f : debug;
                //        }
                //        //forceTarget = 1f / ((distToCenter_Gameobjects - 0.5f) + 0.01f);
                //    }
                //    forceTotalY += forceTarget;
                //}
                //else
                //{
                //    forceTarget = 0;
                //}

                forceTotalY = forceFriction1 + forceCutting1;// + forceTarget;
            }
            else
            {
                contactPosition = Vector3.zero;
            }

            // update calculated forces
            Force[1] += forceTotalY;
            //---------------------------------------------------------------------------
        }
        else
        {
            contactPosition = Vector3.zero;


            //if (contactPositionX != OUTSIDE_POSITION)
            //{
            //    SimulationModule.NeedleToSkinPositionChanged(true);
            //}

            // reset contact position
            contactPositionX = contactPositionZ = -999;
        }
        //---------------------------------------------------------------------------

        // Force feedback to PHANTOM device [N]
        Phantoms.SetForce(Force);

        lock (_lock)
        {
            // set position and orientation for graphic needle
            MyPosition = currentPosition;
            MyRotation = currentRotation;

            // If it is below table position -> set it back to ground level
            //if (HandPosition.y * UNIT_LENGTH < GROUND_LEVEL && previousPosition != Vector3.zero)
            //    MyPosition = previousPosition;
            //else
            //    previousPosition = MyPosition;
        }

        // Log state
        //SimulationModule.LogEntry(Force.x + ";" + Force.y + ";" + Force.z + "," + currentPosition.x + ";" + currentPosition.y + ";" + currentPosition.z + "," + currentRotation.x + ";" + currentRotation.y + ";" + currentRotation.z + ";" + currentRotation.w);

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
        if (dist < 12 * 2.0)
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
