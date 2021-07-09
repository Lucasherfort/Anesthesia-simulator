using UnityEngine;
using HD;
using Utils.Haptics;

namespace ViRTSA
{

    public class HapticExperimentNeedle : GeomagicTouchHapticInterface
    {
        /// <summary>
        /// class instance object -singleton-
        /// </summary>
        public static HapticExperimentNeedle instance;

        //---------------------------------------------------------------------------
        // HAPTIC INFORMATION
        //---------------------------------------------------------------------------

        /// <summary>
        /// The gimbal position [mm]
        /// </summary>
        private Vector3 HandPosition = Vector3.zero;

        /// <summary>
        /// The gimbal position [mm]
        /// </summary>
        public Vector3 LastHapticPosition = Vector3.zero;

        /// <summary>
        /// The gimbal rotation
        /// </summary>
        private Quaternion HandRotation = Quaternion.identity;

        /// <summary>
        /// The gimbal linear speed [mm/s]
        /// </summary>
        public Vector3 HandVelocity = Vector3.zero;

        /// <summary>
        /// Rotation matrix of the needle
        /// </summary>
        private double[] RotationMatrix;

        /// <summary>
        /// Force feedback to apply to device
        /// </summary>
        public Vector3 Force = Vector3.zero;

        //---------------------------------------------------------------------------
        // SIMULATOR CONSTANTS
        //---------------------------------------------------------------------------

        /// <summary>
        /// Exert force upper limit [N]
        /// </summary>
        private const float MAX_FORCE = 3.0f;

        /// <summary>
        /// Exert force scale
        /// </summary>
        private const float DEVICE_FORCE_SCALE = 0.4f;

        /// <summary>
        /// Unit conversion from mm to Unity
        /// </summary>
        private const float UNIT_LENGTH = 0.01f;

        /// <summary>
        /// Low value for rotation angle elimination
        /// </summary>
        private const float ROTATION_LOW_LIMIT = 1.0f;

        /// <summary>
        /// For determining X, Z restriction movements inside skin layers
        /// </summary>
        private const float OUTSIDE_POSITION = -999;

        /// <summary>
        /// Dimensions (half) of the tissue [mm] to know if it is inside the lateral boundaries
        /// </summary>
        private Vector3 TISSUE_DIMENSIONS = new Vector3(200, 0, 200);

        /// <summary>
        /// Table position during experimentation
        /// </summary>
        /// <remarks>Modify RigidObstacles position to have correct force feedback</remarks>
        public const float GROUND_LEVEL = -0.30f;

        /// <summary>
        /// Top position of first layer (Unity units)
        /// </summary>
        private const float FIRST_LAYER_TOP = 0.30f;

        /// <summary>
        /// Top position of second layer (Unity units)
        /// </summary>
        private const float SECOND_LAYER_TOP = 0.0f;

        //---------------------------------------------------------------------------
        // TISSUE ATTRIBUTES
        //---------------------------------------------------------------------------

        /// <summary>
        /// List of present obstacles
        /// </summary>
        private Obstacles[] Obstacles = null;

        /// <summary>
        /// Stiffness coefficient for Skin Layer [N/m]
        /// </summary>
        [SerializeField]
        private float kStiffness1stLayerHaptic = 31.5f; //20

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

        /// <summary>
        /// 
        /// </summary>
        private float hardStickForce = -0.99f;

        //---------------------------------------------------------------------------
        // NEEDLE ATTRIBUTES
        //---------------------------------------------------------------------------

        /// <summary>
        /// Current position of needle
        /// </summary>
        private Vector3 MyPosition = Vector3.zero;

        /// <summary>
        /// Current rotation of needle
        /// </summary>
        private Quaternion MyRotation = Quaternion.identity;

        //---------------------------------------------------------------------------
        // NEEDLE STATE VARIABLES
        //---------------------------------------------------------------------------

        /// <summary>
        /// X position of the tip when entering the skin layers
        /// </summary>
        private float contactPositionX = OUTSIDE_POSITION;

        /// <summary>
        /// Z position of the tip when entering the skin layers
        /// </summary>
        private float contactPositionZ = OUTSIDE_POSITION;

        /// <summary>
        /// The gimbal position [mm] stored when contact with first membrane (reseted when transpasing it)
        /// </summary>
        public Vector3 contactPosition = Vector3.zero;

        /// <summary>
        /// Position of the tip when entering the skin layers
        /// </summary>
        private Vector3 lastPosDevice = Vector3.zero;

        /// <summary>
        /// Rotation of the tip when entering the skin layers
        /// </summary>
        private Quaternion lastRotDevice = Quaternion.identity;

        /// <summary>
        /// Position of the tip before reaching table
        /// </summary>
        private Vector3 previousPosition;

        //---------------------------------------------------------------------------
        // SIMULATOR VARIABLES
        //---------------------------------------------------------------------------

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

        //---------------------------------------------------------------------------
        // SIMULATOR VARIABLES
        //---------------------------------------------------------------------------

        /// <summary>
        /// Mutex for thread safety for position & rotation variables
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// Mutex for thread safety for simulator state
        /// </summary>
        private readonly object _lockState = new object();

        /// <summary>
        /// Simulator state: haptic state
        /// </summary>
        public SimulationState _hapticState = SimulationState.SIMULATION_OFF;

        /// <summary>
        /// State variable for haptic thread
        /// </summary>
        public SimulationState _state = SimulationState.SIMULATION_OFF;

        /// <summary>
        /// Initial position of the starting point
        /// </summary>
        private Vector3 StartPointPosition = new Vector3(0, 70, 0);

        /// <summary>
        /// 
        /// </summary>
        private Vector3 currentPosition;

        public string hapticStringId;

        //---------------------------------------------------------------------------
        // FUNCTIONS
        //---------------------------------------------------------------------------

        /// <summary>
        /// Process when the script becomes enabled and active
        /// </summary>
        private void OnEnable()
        {
            Init();
        }

        /// <summary>
        /// Process when starting script
        /// </summary>
        private void Start()
        {
            InitHaptics(hapticStringId);
        }

        /// <summary>
        /// Process each frame
        /// </summary>
        private void Update()
        {
            lock (_lock)
            {
                // Set device position (unity length) and orientation
                transform.localPosition = MyPosition;
                transform.localRotation = MyRotation;
            }
        }

        /// <summary>
        /// Process when the script becomes disabled or inactive
        /// </summary>
        protected void OnDisable()
        {
            // get actual simulation state
            lock (_lockState)
                _hapticState = SimulationState.TURN_OFF;
            Debug.Log("Haptic go out on disable");
            StopHaptics();
        }

        /// <summary>
        /// Function used to externally set an state for the haptic feedback control
        /// </summary>
        /// <param name="state">State to switch on to</param>
        public override void SetState(SimulationState state)
        {
            // update haptic state
            lock (_lockState)
                _hapticState = state;
        }

        /// <summary>
        /// Initialization of the manager
        /// </summary>
        private void Init()
        {
            // Initialization of hand position and orientation
            HandPosition = Vector3.zero;
            HandRotation = Quaternion.identity;

            // Initialize object attributes;
            MyPosition = transform.position;
            MyRotation = transform.rotation;

            // Initialization of speed and direction
            HandVelocity = Vector3.zero;

            // Make a list of the rigid obstacles in the environment
            //Obstacles = GameObject.FindObjectsOfType<Obstacles>();previousPosition

            // Initialize object attributes;
            previousPosition = contactPosition = transform.position;

            // The rest of the initializations
            contactPositionX = contactPositionZ = OUTSIDE_POSITION;
            lastRotDevice = Quaternion.identity;
            lastPosDevice = Vector3.zero;

            // state variables
            _hapticState = _state = SimulationState.SIMULATION_ON;
        }

        /// <summary>
        /// Method that is repeatedly called in PHANTOM's cycle (default rate 1 [kHz])
        /// </summary>
        /// <returns><c>true</c>, if update was phantomed, <c>false</c> otherwise.</returns>
        protected override bool HapticsUpdateCallback()
        {
            HdAPI.hdBeginFrame(_deviceHandle);

            // Get the position of the hand (gimbal part) [mm]
            HandPosition = HapticDevice.GetPosition();

            // Get the hand posture (orientation)
            HandRotation = HapticDevice.GetRotation();

            // Get the speed of the hand [mm/s]
            HandVelocity = HapticDevice.GetVelocity();

            // Re-init force feedback to 0
            Force = Vector3.zero;

            // get actual simulation state
            lock (_lockState)
                _state = _hapticState;

            //// SIMULATION OFF - do nothing
            //if (_state == SimulationState.SIMULATION_OFF)
            //{
            //    // reset forces (= Vector3.zero)
            //    HapticDevice.SetForce(Force);

            //    HdAPI.hdEndFrame(_deviceHandle);
            //    return true;
            //}

            //if (_state == SimulationState.TURN_OFF)
            //{
            //    // reset forces (= Vector3.zero)
            //    HapticDevice.SetForce(Force);

            //    HdAPI.hdEndFrame(_deviceHandle);
            //    return false;
            //}

            //if (_state == SimulationState.MODE_NEW)
            //{
            //    Buttons bState = HapticDevice.GetButton();

            //    if (bState == Buttons.Button2)// || bState == Buttons.Button1)
            //    {
            //        lock (_lockState)
            //            _hapticState = SimulationState.WAIT_TO_START_BUTTON;

            //        // notify simulator manager
            //        //SimulationModule.ModeChange();
            //    }
            //}

            //if (_state == SimulationState.WAIT_TO_START_BUTTON)
            //{
            //    Buttons bState = HapticDevice.GetButton();

            //    if (bState == Buttons.Button2)// || bState == Buttons.Button1)
            //    {
            //        lock (_lockState)
            //            _hapticState = SimulationState.SIMULATION_ON;

            //        // notify simulator manager
            //        //SimulationModule.MeetStartPointPhase();
            //    }

            //    // reset forces
            //    HapticDevice.SetForce(Force);

            //    HdAPI.hdEndFrame(_deviceHandle);
            //    return true;
            //}

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
                    HapticDevice.SetForce(Force);

                    HdAPI.hdEndFrame(_deviceHandle);
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
                    MyPosition = HandPosition * UNIT_LENGTH;
                    MyRotation = HandRotation;

                    // If it is below table position -> set it back to ground level
                    if (HandPosition.y * UNIT_LENGTH < GROUND_LEVEL && previousPosition != Vector3.zero)
                        MyPosition = previousPosition;
                    else
                        previousPosition = MyPosition;
                }

                // set ball resistance forces
                HapticDevice.SetForce(Force);

                HdAPI.hdEndFrame(_deviceHandle);
                return true;
            }

            //if (_state == SimulationState.TARGET_REACHED)
            //{
            //    // reset contact position
            //    contactPositionX = contactPositionZ = OUTSIDE_POSITION;

            //    Debug.Log("Target reached notifying simulator manager");

            //    lock (_lockState)
            //        _hapticState = SimulationState.SIMULATION_OFF;

            //    // Save info before
            //    SetState(SimulationState.TURN_OFF); //SimulationModule.EndTrial();

            //    HdAPI.hdEndFrame(_deviceHandle);
            //    return true;
            //}

            // Verification Mode
            //if (_state == SimulationState.FREEZE_POSITION)
            //{
            //    Buttons bState = HapticDevice.GetButton();

            //    if (bState == Buttons.Button2 || bState == Buttons.Button1)
            //    {
            //        lock (_lockState)
            //            _hapticState = SimulationState.TARGET_REACHED;

            //        // notify simulator manager
            //        HapticDevice.SetForce(Force);

            //        HdAPI.hdEndFrame(_deviceHandle);
            //        return true;
            //    }

            //    // calculate a hard force to stick haptic device
            //    Force = (HandPosition - LastHapticPosition) * hardStickForce;
            //    HapticDevice.SetForce(Force);

            //    HdAPI.hdEndFrame(_deviceHandle);
            //    return true;
            //}

            //if (_state == SimulationState.SWITCH_TO_VERIFICATION_MODE)
            //{
            //    LastHapticPosition = new Vector3(HandPosition.x, HandPosition.y, HandPosition.z);

            //    lock (_lockState)
            //        _hapticState = SimulationState.FREEZE_POSITION;

            //    Force = (HandPosition - LastHapticPosition) * -0.1f;
            //    // calculate a hard force to stick haptic device
            //    HapticDevice.SetForce(Force);

            //    HdAPI.hdEndFrame(_deviceHandle);
            //    return true;
            //}

            // _state == SimulationState.NEEDLE_FEEDBACK_ON

            //Buttons bState2 = HapticDevice.GetButton();

            //if (bState2 == Buttons.Button1)// || bState == Buttons.Button1)
            //{
            //    lock (_lockState)
            //        _hapticState = SimulationState.MODE_NEW;
            //    // notify simulator manager
            //    //SimulationModule.ModeChange();
            //}

            // ---------------------------------------------------------------------------
            // OBSTACLES FORCE ADDITION
            //---------------------------------------------------------------------------

            // Calculate the force received from the obstacles
            //if (Obstacles != null)
            //{
            //    // Temporal variable to calculate the obstacles force
            //    Vector3 force = Vector3.zero;

            //    foreach (Obstacles obj in Obstacles)
            //    {
            //        try
            //        {
            //            // calculate rigid obstacles forces (ex. Table)
            //            force += ((RigidObstacles)obj).CalculateForce(HandPosition, HandVelocity);
            //        }
            //        catch
            //        {
            //            // calculate other obstacles forces (such as cysts) // FIXME
            //            force += ((Obstacles)obj).CalculateForce(HandPosition, HandVelocity);
            //        }
            //    }

            //    // add obstacles force
            //    Force += force;
            //}
            //---------------------------------------------------------------------------

            // Hand position & rotation in the Unity world
            //Vector3 
            currentPosition = HandPosition * UNIT_LENGTH;
            Quaternion currentRotation = HandRotation;

            // init forces to apply to haptic in the Y direction
            forceStiffness1 = forceFriction1 = forceCutting1 = forceTotalY = 0f;

            //---------------------------------------------------------------------------
            // FORCES FROM TISSUE - NEEDLE INTERACTION (1st and 2nd layer)
            //---------------------------------------------------------------------------

            // if within the square of tissue in X, Z coordinates (big cube with all tissue layers inside)
            if (Mathf.Abs(HandPosition.x) < TISSUE_DIMENSIONS.x && Mathf.Abs(HandPosition.z) < TISSUE_DIMENSIONS.z)
            {
                // get vertical position of the needle
                float verticalPosition = HandPosition.y * UNIT_LENGTH;

                // if it has traspased the membrane
                if (verticalPosition < FIRST_LAYER_TOP - 0.05)
                {
                    contactPosition = Vector3.zero;

                    // set contact position, store position and rotation of needle at the moment of penetration
                    if (contactPositionX == OUTSIDE_POSITION && contactPositionZ == OUTSIDE_POSITION)
                    {
                        contactPositionX = currentPosition.x;
                        contactPositionZ = currentPosition.z;
                        lastPosDevice = currentPosition;
                        lastRotDevice = HandRotation;

                        // get rotation matrix to get direction of the needle when penetrating
                        HapticDevice.GetRotationMatrix(out RotationMatrix);

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
                    contactPositionX = contactPositionZ = OUTSIDE_POSITION;
                }

                // init depth variables and velocity
                float probeDop = 0f;
                float probeDopStiffness = 0f;
                float velocity = 0f;

                // limit visual direction if needle inside tissue and calculate lateral forces
                if (contactPositionX != OUTSIDE_POSITION && contactPositionZ != OUTSIDE_POSITION)
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
                probeDopStiffness = FIRST_LAYER_TOP + 0.125f - currentPosition.y;

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
                    ClampValue = (float)HapticDevice.GetContinuousForceLimit();
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
                    velocity = HandVelocity.y * UNIT_LENGTH;
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
                contactPositionX = contactPositionZ = OUTSIDE_POSITION;
            }
            //---------------------------------------------------------------------------

            // Force feedback to PHANTOM device [N]
            HapticDevice.SetForce(Force);

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

            HdAPI.hdEndFrame(_deviceHandle);
            return true;
        }

        /*public void GetLogInfo(out Vector3 pos, out Quaternion rot, out Vector3 force)
        {

        }*/

        //public Transform target;
        //public float distToCenter_Gameobjects, forceTarget, sc, debug;
        //public Vector3 target_position;

        //public override void FindTargetInfo()
        //{
        //    target = GameObject.FindGameObjectWithTag("Cyst").transform;
        //    target_position = transform.InverseTransformPoint(target.position);
        //    sc = target.localScale.x / 2;
        //}
    }
}