// HEADER

// INCLUDES
using UnityEngine;
using HD;
using Utils.Haptics;

/// <summary>
/// Namespace for Needle Simulator
/// </summary>
namespace ViRTSA
{
    /// <summary>
    /// Haptic Manager class
    /// </summary>
    public class HapticExperimentManager : GeomagicTouchHapticInterface
    {
        //---------------------------------------------------------------------------
        // HAPTIC INFORMATION
        //---------------------------------------------------------------------------

        /// <summary>
        /// The gimbal position [mm]
        /// </summary>
        public Vector3 HandPosition = Vector3.zero;

        /// <summary>
        /// The gimbal position [mm]
        /// </summary>
        public Vector3 LastHapticPosition = Vector3.zero;

        /// <summary>
        /// The gimbal linear speed [mm/s]
        /// </summary>
        public Vector3 HandVelocity = Vector3.zero;

        /// <summary>
        /// The gimbal rotation
        /// </summary>
        private Quaternion HandRotation = Quaternion.identity;

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
        private Vector3 TISSUE_DIMENSIONS = new Vector3(80, 0, 60);

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
        private float kStiffness1stLayerHaptic = 20f;

        /// <summary>
        /// Damping coefficient for Skin Layer [N/m]
        /// </summary>
        private float kDamping1stLayerHaptic = 2.2f;

        /// <summary>
        /// Cutting coefficient for Skin Layer [N/m]
        /// </summary>
        private float kCutting1stLayerHaptic = 1.8f;

        /// <summary>
        /// Stiffness coefficient for Skin Layer [N/m]
        /// </summary>
        private float kStiffness2ndLayerHaptic = 35f;

        /// <summary>
        /// Damping coefficient for Skin Layer [N/m]
        /// </summary>
        private float kDamping2ndLayerHaptic = 2.2f;

        /// <summary>
        /// Cutting coefficient for Skin Layer [N/m]
        /// </summary>
        private float kCutting2ndLayerHaptic = 1.8f;

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

        /// <summary>
        /// Last frame rotation of needle for rotation filter
        /// </summary>
        private Quaternion PreviousMyRotation = Quaternion.identity;

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
        private float planeGuidePenetrated = OUTSIDE_POSITION;

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
        /// Trial information about total elapsed time
        /// </summary>
        //////private float _TrialTotalTime;

        /// <summary>
        /// Trial information about final position
        /// </summary>
        //////private Vector3 _TrialFinalPosition;

        /// <summary>
        /// Initial position of the starting point
        /// </summary>
        private Vector3 StartPointPosition = new Vector3(0, 70, 0);

        /// <summary>
        /// 
        /// </summary>
        private bool _begin = true;

        /// <summary>
        /// 
        /// </summary>
        private bool buttonState = false;

        /// <summary>
        /// 
        /// </summary>
        private bool changeButton = false;

        /// <summary>
        /// 
        /// </summary>
        public Transform quad;

        /// <summary>
        /// 
        /// </summary>
        public Transform planRemove;

        /// <summary>
        /// 
        /// </summary>
        public Transform planSet;


        public Transform target;
        public float distToCenter_Gameobjects, forceTarget, sc, debug, pos;
        public Vector3 target_position, currentPosition;

        /// ----------------------------------------------------------------------------------
        /// --FIXME ===>>>> FOR DEBUG
        /// ----------------------------------------------------------------------------------
        public Vector3 FUERZA = Vector3.zero;
        /// ----------------------------------------------------------------------------------

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

        }

        /// <summary>
        /// Process each frame => Updates graphic representation
        /// </summary>
        private void Update()
        {
            // save previous rotation
            PreviousMyRotation = transform.localRotation;

            lock (_lock)
            {
                // Set device position (unity length) and orientation
                transform.localPosition = MyPosition;
                if (Quaternion.Angle(PreviousMyRotation, MyRotation) > ROTATION_LOW_LIMIT)
                    transform.localRotation = MyRotation;

                if (changeButton)
                {
                    if (buttonState)
                    {
                        switch (option)
                        {
                            case 1:
                                planSet.position = target.position;
                                break;
                            case 2:
                                if (planeGuidePenetrated != OUTSIDE_POSITION)
                                    planSet.position = new Vector3(0, -20, planeGuidePenetrated);
                                else
                                    planSet.position = target.position;
                                break;
                            case 3:
                                planSet.position = target.position;
                                break;
                        }

                        quad.parent = planSet;
                        quad.localPosition = new Vector3(0, -20, 0);
                    }
                    else
                    {
                        quad.parent = planRemove;
                        quad.localPosition = new Vector3(0, -20, 0);
                    }
                    changeButton = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                option = 1;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                option = 2;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                option = 3;
            }
        }

        int option = 1;

        /// <summary>
        /// Process when the script becomes disabled or inactive
        /// </summary>
        private void OnDisable()
        {
            // get actual simulation state
            lock (_lockState)
                _hapticState = SimulationState.TURN_OFF;
            Debug.Log("Haptic go out on disable");
            StopHaptics();
        }

        /// <summary>
        /// Initialization of the manager
        /// </summary>
        private void Init()
        {
            // Initialization of hand position and orientation
            HandPosition = Vector3.zero;
            HandRotation = Quaternion.identity;

            // Initialization of speed and direction
            //HandDirection = Vector3.zero;
            HandVelocity = Vector3.zero;

            // Initialization of forces to apply
            Force = Vector3.zero;

            // Make a list of the rigid obstacles in the environment
            Obstacles = GameObject.FindObjectsOfType<Obstacles>();

            // Initialize object attributes;
            MyPosition = previousPosition = contactPosition = transform.position;
            MyRotation = transform.rotation;

            // The rest of the initializations
            contactPositionX = contactPositionZ = planeGuidePenetrated = OUTSIDE_POSITION;
            lastRotDevice = Quaternion.identity;
            lastPosDevice = Vector3.zero;

            // init measure variables
            //////_TrialFinalPosition = Vector3.zero;
            //////_TrialTotalTime = 0;

            // state variables
            _hapticState = _state = SimulationState.SIMULATION_OFF;
            _begin = true;
        }

        /// <summary>
        /// Function to pause execution because there was a technical problem.
        /// Manually activated by user/experimenter
        /// </summary>
        public override void PauseExecution()
        {
            // update haptic state to technical problem
            lock (_lockState)
                _hapticState = SimulationState.TECHNICAL_PROBLEM;
        }

        /// <summary>
        /// Function used to externally set an state for the haptic feedback control
        /// </summary>
        /// <param name="state">State to switch on to</param>
        public override void SetState(SimulationState state)
        {
            if (state == SimulationState.SIMULATION_ON)
            {
                // clean all the previous measures (data is already with simualtor manager)
                Debug.Log("Cleaning previous trial data...");

                //////_TrialTotalTime = 0;
                //////_TrialFinalPosition = Vector3.zero;

                if (_begin)
                {
                    _begin = false;
                    state = SimulationState.WAIT_TO_START_BUTTON;
                }
            }

            // update haptic state
            lock (_lockState)
                _hapticState = state;
        }

        public override void FindTargetInfo()
        {
            target = GameObject.FindGameObjectWithTag("Cyst").transform;
            target_position = transform.InverseTransformPoint(target.position);
            sc = target.localScale.x / 2;
        }

        /// <summary>
        /// Collects the trial time and needle final position when target reached
        /// </summary>
        /// <param name="data">Contains the total elapsed time for the trial and final position of the needle</param>
        //////public void GetTrialMeasures(out LogInfo data)
        //////{
        //////    // Set device final position
        //////    transform.localPosition = MyPosition;

        //////    lock (_lock)
        //////        _TrialFinalPosition = transform.position;

        //////    data.TotalTime = -1;
        //////    data.FinalPosition = _TrialFinalPosition;
        //////}

        /// <summary>
        /// Function to be executed asynchronously from the haptic device
        /// Responsable of all the haptic force feedback during simulation
        /// </summary>
        /// <returns><code>true</code> if everything was successfully applied</returns>
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

            if (_state == SimulationState.TURN_OFF)
            {
                // reset forces (= Vector3.zero)
                HapticDevice.SetForce(Force);

                lock (_lockState)
                    _hapticState = SimulationState.SIMULATION_OFF;

                HdAPI.hdEndFrame(_deviceHandle);
                return false;
            }

            if (_state == SimulationState.TECHNICAL_PROBLEM)
            {
                // reset contact position
                contactPositionX = contactPositionZ = planeGuidePenetrated = OUTSIDE_POSITION;

                // update position & orientation
                lock (_lock)
                {
                    // set position and orientation for graphic needle
                    MyPosition = Vector3.zero;
                    MyRotation = Quaternion.identity;
                }

                Debug.Log("Technical problem... Abort trial");

                lock (_lockState)
                    _hapticState = SimulationState.SIMULATION_OFF;

                HdAPI.hdEndFrame(_deviceHandle);
                return true;
            }

            // SIMULATION OFF - do nothing
            if (_state == SimulationState.SIMULATION_OFF)
            {
                // reset forces (= Vector3.zero)
                HapticDevice.SetForce(Force);

                HdAPI.hdEndFrame(_deviceHandle);
                return true;
            }

            if (_state == SimulationState.WAIT_TO_START_BUTTON)
            {
                // USER indicates that target is reached + force feedback 0
                if (HapticDevice.GetButton() == Buttons.Button2)
                {
                    lock (_lockState)
                        _hapticState = SimulationState.SIMULATION_ON;

                    // notify simulator manager
                    //SimulationModule.MeetStartPointPhase();
                }

                // reset forces
                HapticDevice.SetForce(Force);

                HdAPI.hdEndFrame(_deviceHandle);
                return true;
            }

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

            // TARGET REACHED - needle old position is the final position + force feedback 0
            if (_state == SimulationState.TARGET_REACHED)
            {
                // reset contact position
                contactPositionX = contactPositionZ = planeGuidePenetrated = OUTSIDE_POSITION;

                Debug.Log("Target reached notifying simulator manager");

                lock (_lockState)
                    _hapticState = SimulationState.SIMULATION_OFF;

                //SimulationModule.Temp();
                // notify simulator manager
                //////SimulationManager.instance.TargetReached();

                HdAPI.hdEndFrame(_deviceHandle);
                return true;
            }////////////////////////////////////////////////

            // EXPLORATION MODE
            if (_state == SimulationState.NEEDLE_FEEDBACK_OFF)
            {
                // USER indicates that target is reached + force feedback 0
                if (HapticDevice.GetButton() != Buttons.Button1)
                {
                    lock (_lockState)
                        _hapticState = SimulationState.NEEDLE_FEEDBACK_ON;

                    lock (_lock)
                    {
                        changeButton = true;
                        buttonState = false;
                    }

                    HdAPI.hdEndFrame(_deviceHandle);
                    return true;
                }

                // calculate a hard force to stick haptic device
                Force = (HandPosition - LastHapticPosition) * hardStickForce;
                HapticDevice.SetForce(Force);

                HdAPI.hdEndFrame(_deviceHandle);
                return true;
            }

            // get pressed buttons
            Buttons pressed_button = HapticDevice.GetButton();

            // USER indicates that target is reached + force feedback 0
            if (pressed_button == Buttons.Button2)
            {
                lock (_lockState)
                    _hapticState = SimulationState.TARGET_REACHED;

                // reset forces
                HapticDevice.SetForce(Force);

                HdAPI.hdEndFrame(_deviceHandle);
                return true;
            }

            // User indicates a change of mode to EXPLORATION
            if (pressed_button == Buttons.Button1)
            {
                LastHapticPosition = new Vector3(HandPosition.x, HandPosition.y, HandPosition.z);

                lock (_lockState)
                    _hapticState = SimulationState.NEEDLE_FEEDBACK_OFF;

                lock (_lockState)
                    _hapticState = SimulationState.NEEDLE_FEEDBACK_OFF;

                Force = (HandPosition - LastHapticPosition) * -0.1f;
                // calculate a hard force to stick haptic device
                HapticDevice.SetForce(Force);

                lock (_lock)
                {
                    changeButton = true;
                    buttonState = true;
                }

                HdAPI.hdEndFrame(_deviceHandle);
                return true;
            }

            //---------------------------------------------------------------------------
            // OBSTACLES FORCE ADDITION
            //---------------------------------------------------------------------------

            // Calculate the force received from the obstacles
            if (Obstacles != null)
            {
                // Temporal variable to calculate the obstacles force
                Vector3 force = Vector3.zero;

                foreach (Obstacles obj in Obstacles)
                {
                    try
                    {
                        // calculate rigid obstacles forces (ex. Table)
                        force += ((RigidObstacles)obj).CalculateForce(HandPosition, HandVelocity);
                    }
                    catch
                    {
                        // calculate other obstacles forces (such as cysts) // FIXME
                        force += ((Obstacles)obj).CalculateForce(HandPosition, HandVelocity);
                    }
                }

                // add obstacles force
                Force += force;
            }
            //---------------------------------------------------------------------------

            // Hand position & rotation in the Unity world
            //Vector3 
            currentPosition = HandPosition * UNIT_LENGTH;
            Quaternion currentRotation = HandRotation;

            // init forces to apply to haptic in the Y direction
            forceStiffness1 = forceFriction1 = forceCutting1 = forceStiffness2 = forceFriction2 = forceCutting2 = forceDumping12 = forceTotalY = 0f;

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

                        planeGuidePenetrated = contactPositionZ;

                        // get rotation matrix to get direction of the needle when penetrating
                        HapticDevice.GetRotationMatrix(out RotationMatrix);
                        //SimulationModule.NeedleToSkinPositionChanged(true);
                    }
                }
                else
                {
                    // reset contact position
                    //if (contactPositionX != OUTSIDE_POSITION)
                    //    SimulationModule.NeedleToSkinPositionChanged(false);
                    contactPositionX = contactPositionZ = planeGuidePenetrated = OUTSIDE_POSITION;
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
                //probeDopStiffness = Mathf.Clamp(probeDopStiffness, 0f, 0.35f);

                // Damping force
                if (verticalPosition < SECOND_LAYER_TOP - 0.05)
                {
                    // get velocity and limit it
                    velocity = HandVelocity.y;
                    velocity = Mathf.Clamp(velocity, -0.8f, 0.8f);

                    // Force of first layer present when inside second layer --not documented the source of this force (present when is in second layer)
                    forceDumping12 = (kDamping1stLayerHaptic) * (-velocity) * (SECOND_LAYER_TOP + GROUND_LEVEL);
                    forceTotalY += forceDumping12 * DEVICE_FORCE_SCALE;
                }
                else if (probeDopStiffness > 0 && probeDop == 0)
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
                    //float f1 = -3.39f;
                    //float a1 = -0.031f;
                    //float b1 = 1.7f;

                    // get velocity and limit it
                    velocity = HandVelocity.y * UNIT_LENGTH;
                    velocity = Mathf.Clamp(velocity, -1.5f, 1.5f);

                    // calculate friction force
                    forceFriction1 = (-velocity * 3 + 800 * ((f0 + b0) * Mathf.Exp(a0 * probeDopStiffness) + b0)) / kDamping1stLayerHaptic;
                    //forceFriction1 = (-velocity * 30 + 400 * ((f1 + b1) * Mathf.Exp(a1 * probeDopStiffness) + b1)) / kDamping1stLayerHaptic;

                    // apply scale factor for forces
                    forceFriction1 = forceFriction1 * DEVICE_FORCE_SCALE;

                    // add cutting force (= constant)
                    forceCutting1 = kCutting1stLayerHaptic;
                    forceTotalY = forceFriction1 + forceCutting1;

                    //---------------------------------------------------------------------------
                }
                else
                {
                    contactPosition = Vector3.zero;
                }

                //---------------------------------------------------------------------------

                //---------------------------------------------------------------------------
                // SECOND LAYER FORCE ADDITION
                //---------------------------------------------------------------------------

                if (currentPosition.y < SECOND_LAYER_TOP + 0.05f)
                {
                    float t2 = (SECOND_LAYER_TOP - lastPosDevice.y) / -(float)RotationMatrix[9];
                    Vector3 tempPosition = new Vector3(t2 * -(float)RotationMatrix[8] + lastPosDevice.x, SECOND_LAYER_TOP, t2 * (float)RotationMatrix[10] + lastPosDevice.z);

                    // depth in the skin from penetration point
                    probeDop = (currentPosition - tempPosition).magnitude;

                    // limit depthness
                    probeDop = Mathf.Clamp(probeDop, 0f, 0.35f);

                    // get position from top 2nd layer position
                    probeDopStiffness = SECOND_LAYER_TOP + 0.05f - currentPosition.y;
                    //probeDopStiffness = Mathf.Clamp(probeDopStiffness, 0f, 0.35f);

                    if (probeDopStiffness > 0 && verticalPosition > SECOND_LAYER_TOP - 0.05)
                    {
                        //---------------------------------------------------------------------------
                        // MEMBRANE STIFFNESS FORCE (before penetration)
                        //---------------------------------------------------------------------------

                        forceStiffness2 = kStiffness2ndLayerHaptic * probeDopStiffness + kDamping2ndLayerHaptic * (-velocity) * probeDopStiffness;

                        // apply scale factor for forces
                        forceStiffness2 = forceStiffness2 * DEVICE_FORCE_SCALE;
                        forceTotalY += forceStiffness2;

                        //---------------------------------------------------------------------------
                    }
                    else if (probeDop > 0 && verticalPosition < SECOND_LAYER_TOP - 0.05)
                    {
                        //---------------------------------------------------------------------------
                        // TISSUE FRICTION + CUTTING FORCE (after penetration)
                        //---------------------------------------------------------------------------

                        float f0 = 0.185f;
                        float a0 = 0.12f;
                        float b0 = -0.097f;
                        //float f1 = -3.39f;
                        //float a1 = -0.031f;
                        //float b1 = 1.7f;

                        // get velocity and limit it
                        velocity = HandVelocity.y * UNIT_LENGTH;
                        velocity = Mathf.Clamp(velocity, -1.5f, 1.5f);

                        forceFriction2 = (-velocity * 3 + 900 * ((f0 + b0) * Mathf.Exp(a0 * probeDopStiffness) + b0)) / kDamping2ndLayerHaptic;

                        // apply scale factor for forces
                        forceFriction2 = forceFriction2 * DEVICE_FORCE_SCALE;

                        forceCutting2 = kCutting2ndLayerHaptic;
                        forceTotalY += forceFriction2 + forceCutting2;

                        //---------------------------------------------------------------------------

                        //---------------------------------------------------------------------------
                        // CYST TARGET MEMBRANE STIFFNESS FORCE
                        //---------------------------------------------------------------------------
                        distToCenter_Gameobjects = (currentPosition - target_position).magnitude;

                        if (distToCenter_Gameobjects < (sc + 0.025))
                        {
                            if (distToCenter_Gameobjects < (sc - 0.025))
                            {
                                forceTarget = 0;
                            }
                            else
                            {
                                if (currentPosition.y < target_position.y)
                                {
                                    debug = 1 / (100 * Mathf.Abs(distToCenter_Gameobjects - (sc + 0.015f)));
                                    forceTarget = debug > 1.0f ? 1.0f : debug;
                                }
                                else
                                {
                                    debug = 1 / (100 * Mathf.Abs(distToCenter_Gameobjects - (sc - 0.015f)));
                                    forceTarget = debug > 1.0f ? 1.0f : debug;
                                }
                                //forceTarget = 1f / ((distToCenter_Gameobjects - 0.5f) + 0.01f);
                            }
                            forceTotalY += forceTarget;
                        }
                        else
                        {
                            forceTarget = 0;
                        }
                    }
                }
                //---------------------------------------------------------------------------

                // update calculated forces
                Force[1] += forceTotalY;
                //---------------------------------------------------------------------------
            }
            else
            {
                contactPosition = Vector3.zero;

                //if (contactPositionX != OUTSIDE_POSITION)
                //    SimulationModule.NeedleToSkinPositionChanged(true);

                // reset contact position
                contactPositionX = contactPositionZ = planeGuidePenetrated = OUTSIDE_POSITION;
            }
            //---------------------------------------------------------------------------

            //---------------------------------------------------------------------------
            // CLAMP FORCE AND SEND TO DEVICE
            //---------------------------------------------------------------------------

            // So as not to exceed the upper limit of the force
            //if (Force.sqrMagnitude > (MAX_FORCE * MAX_FORCE))
            //{
            //    Force.Normalize();
            //    Force *= MAX_FORCE;
            //}

            //---------------------------------------------------------------------------

            //---------------------------------------------------------------------------
            // for debug
            FUERZA = Force;
            //---------------------------------------------------------------------------

            // Force feedback to PHANTOM device [N]
            HapticDevice.SetForce(Force);

            bool outside = false;
            lock (_lock)
            {
                // set position and orientation for graphic needle
                MyPosition = currentPosition;
                MyRotation = currentRotation;

                // If it is below table position -> set it back to ground level
                if (outside = (HandPosition.y * UNIT_LENGTH < GROUND_LEVEL && previousPosition != Vector3.zero))
                    MyPosition = previousPosition;
                else
                    previousPosition = MyPosition;
            }

            currentPosition = outside ? previousPosition : currentPosition;

            // Log state
            //////SimulationManager.instance.LogEntry(new Vector3(Force.x, Force.y, Force.z), new Vector3(currentPosition.x, currentPosition.y, currentPosition.z));

            HdAPI.hdEndFrame(_deviceHandle);
            return true;
        }
    }
}