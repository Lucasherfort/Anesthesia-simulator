using UnityEngine;
using HD;
using Utils.Haptics;

namespace ViRTSA
{
    public class HapticTrainingMovements : GeomagicTouchHapticInterface
    {
        /// <summary>
        /// class instance object -singleton-
        /// </summary>
        public static HapticTrainingMovements instance;

        //---------------------------------------------------------------------------
        // HAPTIC INFORMATION
        //---------------------------------------------------------------------------

        /// <summary>
        /// The gimbal position [mm]
        /// </summary>
        private Vector3 HandPosition = Vector3.zero;

        /// <summary>
        /// The gimbal rotation
        /// </summary>
        private Quaternion HandRotation = Quaternion.identity;

        /// <summary>
        /// Force feedback to apply to device
        /// </summary>
        public Vector3 Force = Vector3.zero;

        //---------------------------------------------------------------------------
        // SIMULATOR CONSTANTS
        //---------------------------------------------------------------------------

        /// <summary>
        /// Unit conversion from mm to Unity
        /// </summary>
        private const float UNIT_LENGTH = 0.01f;

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
                _hapticState = SimulationState.SIMULATION_OFF   ;
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

            // Initialization of forces to apply
            Force = Vector3.zero;

            // state variables
            _hapticState = _state = SimulationState.SIMULATION_OFF;
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

            // Re-init force feedback to 0
            Force = Vector3.zero;

            // get actual simulation state
            lock (_lockState)
                _state = _hapticState;

            if (_state == SimulationState.TURN_OFF)
            {
                // reset forces (= Vector3.zero)
                HapticDevice.SetForce(Force);

                HdAPI.hdEndFrame(_deviceHandle);
                return false;
            }

            // SIMULATION OFF - do nothing
            if (_state == SimulationState.SIMULATION_OFF)
            {
                // reset forces (= Vector3.zero)
                HapticDevice.SetForce(Force);

                HdAPI.hdEndFrame(_deviceHandle);
                return true;
            }

            // _state == SimulationState.SIMULATION_ON

            // Force feedback = 0 to PHANTOM device [N]
            HapticDevice.SetForce(Force);

            lock (_lock)
            {
                // set position and orientation for graphic needle
                MyPosition = HandPosition * UNIT_LENGTH;
                MyRotation = HandRotation;
            }

            HdAPI.hdEndFrame(_deviceHandle);
            return true;
        }
    }
}