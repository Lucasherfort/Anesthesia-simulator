// HEADER

// INCLUDES

using System;
using UnityEngine;
using HD;

/// <summary>
/// Namespace for Needle Simulator
/// </summary>
namespace ViRTSA
{
    /// <summary>
    /// 
    /// </summary>
    public class GeomagicTouchHapticInterface : MonoBehaviour
    {
        /// <summary>
        /// PHANTOM instance
        /// </summary>
        protected GeomagicTouchHapticDevice HapticDevice = null;

        /// <summary>
        /// 
        /// </summary>
        protected uint _deviceHandle;

        /// <summary>
        /// 
        /// </summary>
        //protected Simulation SimulationModule;

        /// <summary>
        /// Initializes communication with HapticDevice device
        /// </summary>
        /// <returns><code>true</code> if everything was well instantiated, <code>false</code> if 
        /// it exist already a communication or if it was unsuccessful</returns>
        public bool InitHaptics(string deviceName = "Default Device")
        {
            if (HapticDevice != null)
                return false;

            // Instantiation of HapticDevice
            HapticDevice = new GeomagicTouchHapticDevice();

            // To start the iterative process
            if (HapticDevice != null)
            {
                // create device
                _deviceHandle = HapticDevice.InitDevice(deviceName);
                if (_deviceHandle == 255) return false;

                // It specifies the method to be executed repeatedly
                HapticDevice.AddSchedule(HapticsUpdateCallback, HdAPI.Priority.HD_RENDER_EFFECT_FORCE_PRIORITY);

                // test --FIXME
                while (!HapticDevice.IsAvailable()) ;

                Debug.Log("Initializing device...");
                HapticDevice.Start();

                // Get information about the device
                Debug.Log("Information HapticDevice device :\n" +
                    "Usable Workspace Max    = " + HapticDevice.UsableWorkspaceMaximum + "\n" +
                    "Usable Workspace Min    = " + HapticDevice.UsableWorkspaceMinimum + "\n" +
                    "Workspace Available Max = " + HapticDevice.WorkspaceMaximum + "\n" +
                    "Workspace Available Min = " + HapticDevice.WorkspaceMinimum + "\n" +
                    "Instant. Update Rate    = " + HapticDevice.GetInstantaneousUpdateRate() + "\n" +
                    "Max nominal cont. Force = " + HapticDevice.GetContinuousForceLimit() + "\n" +
                    "Max nominal Force       = " + HapticDevice.GetForceLimit() + "\n" +
                    "Max force clamping enab = " + HapticDevice.IsEnabledMaxForceClamping() + "\n" +
                    "SW force limit enab     = " + HapticDevice.IsEnabledSwForceLimit() + "\n");
            }

            return HapticDevice != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual bool HapticsUpdateCallback()
        {
            HdAPI.hdBeginFrame(_deviceHandle);

            // Use haptics functions here

            HdAPI.hdEndFrame(_deviceHandle);

            return true;
        }

        /// <summary>
        /// Stops device communication
        /// </summary>
        /// <returns><code>true</code> in case of success, <code>false</code> otherwise</returns>
        protected bool StopHaptics()
        {
            if (HapticDevice == null || !HapticDevice.IsRunning)
                return false;

            while (!HapticDevice.IsAvailable()) Debug.Log("...");

            // Exit the use of PHANTOM
            HapticDevice.Close();
            HapticDevice = null;

            return true;
        }

        /// <summary>
        /// Function used to externally set an state for the haptic feedback control
        /// </summary>
        /// <param name="state">State to switch on to</param>
        public virtual void SetState(SimulationState state)
        {

        }

        public virtual void FindTargetInfo()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        //public void SetSimulationManager(Simulation manager)
        //{
        //    SimulationModule = manager;
        //}

        /// <summary>
        /// Function to pause execution because there was a technical problem.
        /// Manually activated by user/experimenter
        /// </summary>
        public virtual void PauseExecution()
        {

        }

        /// <summary>
        /// Seek the forces generated when the operating point is in contact with the lateral membrane
        /// </summary>
        /// <param name="tipPosition">The position of the tip [mm]</param>
        /// <param name="tipVelocity">The speed of the tip [mm/s]</param>
        /// <param name="lateralPosition">The stored lateral position</param>
        /// <param name="axe">To determine if it is X (<code>0</code>) or Z (<code>2</code>)</param>
        /// <returns>The force to apply</returns>
        protected Vector3 CalculateLateralForce(Vector3 tipPosition, Vector3 tipVelocity, Vector3 lateralPosition, int axe)
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
}
