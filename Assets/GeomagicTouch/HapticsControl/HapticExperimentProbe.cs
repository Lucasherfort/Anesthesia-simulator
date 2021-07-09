using UnityEngine;
using HD;
using Utils.Haptics;
using System;

namespace ViRTSA
{

    public class HapticExperimentProbe : GeomagicTouchHapticInterface
    {
        /// <summary>
        /// class instance object -singleton-
        /// </summary>
        public static HapticExperimentProbe instance;

        //---------------------------------------------------------------------------
        // HAPTIC INFORMATION
        //---------------------------------------------------------------------------


        /// <summary>
        /// Struct containing information attached to one device
        /// </summary>
        public struct PhantomDeviceInfo
        {
            /// <summary>
            /// Device configuration name
            /// </summary>
            public string Name;

            /// <summary>
            /// Device handler
            /// </summary>
            public uint hHdAPI;

            /// <summary>
            /// Device position
            /// </summary>
            public Vector3 position;

            /// <summary>
            /// Device rotation
            /// </summary>
            public Quaternion rotation;

            /// <summary>
            /// Force to be applied to the attached device
            /// </summary>
            public Vector3 force;

            /// <summary>
            /// Scene object attached to this device's position and rotation
            /// </summary>
            public GameObject tool;
        }

        /// <summary>
        /// Left device structure
        /// </summary>
        public PhantomDeviceInfo LeftPhantomDevice;

        /// <summary>
        /// Right device structure
        /// </summary>
        public PhantomDeviceInfo RightPhantomDevice;

        /// <summary>
        /// The gimbal position [mm]
        /// </summary>
        [SerializeField]
        private Vector3 HandPosition_Left = Vector3.zero;

        /// <summary>
        /// The gimbal position [mm]
        /// </summary>
        private Vector3 HandPosition_Right = Vector3.zero;

        /// <summary>
        /// The gimbal rotation
        /// </summary>
        private Quaternion HandRotation_Left = Quaternion.identity;

        /// <summary>
        /// The gimbal rotation
        /// </summary>
        private Quaternion HandRotation_Right = Quaternion.identity;

        /// <summary>
        /// Force feedback to apply to device
        /// </summary>
        public Vector3 Force_Left = Vector3.zero;

        /// <summary>
        /// Force feedback to apply to device
        /// </summary>
        public Vector3 Force_Right = Vector3.zero;

        //---------------------------------------------------------------------------
        // SYSTEM CONSTANTS
        //---------------------------------------------------------------------------

        /// <summary>
        /// Unit conversion from mm to Unity
        /// </summary>
        public float UnitLength = 0.01f;

        public string hapticStringId;

        public GameObject visual;

        //---------------------------------------------------------------------------
        // FUNCTIONS
        //---------------------------------------------------------------------------

        /// <summary>
        /// Process when the script becomes enabled and active
        /// </summary>
        private void OnEnable()
        {

            LeftPhantomDevice.Name = "Left Device";
            LeftPhantomDevice.hHdAPI = _deviceHandle;
            LeftPhantomDevice.tool = visual;
            LeftPhantomDevice.position = Vector3.zero;
            LeftPhantomDevice.rotation = Quaternion.identity;
        }

        /// <summary>
        /// Process when starting script
        /// </summary>
        private void Start()
        {
            InitHaptics(hapticStringId);
        }
        
        /// <summary>
        /// Process when the script becomes disabled or inactive
        /// </summary>
        protected void OnDisable()
        {
            // get actual simulation state
            Debug.Log("Haptic go out on disable");
            StopHaptics();
        }
        private void Update()
        {
            LeftPhantomDevice.tool.transform.localPosition = LeftPhantomDevice.position;
            //RightPhantomDevice.tool.transform.localPosition = RightPhantomDevice.position;
            LeftPhantomDevice.tool.transform.localRotation = LeftPhantomDevice.rotation;
            //RightPhantomDevice.tool.transform.localRotation = RightPhantomDevice.rotation;
        }
        // Stiffnes, i.e.k value, of the plane.  Higher stiffness results
        // in a harder surface.
        double planeStiffness = .25;

        // Amount of force the user needs to apply in order to pop through
        // the plane.
        double popthroughForceThreshold = 5.0;

        // Plane direction changes whenever the user applies sufficient
        // force to popthrough it.
        // 1 means the plane is facing +Y.
        // -1 means the plane is facing -Y.
        int directionFlag = 1;

        /// <summary>
        /// Method that is repeatedly called in PHANTOM's cycle (default rate 1 [kHz])
        /// </summary>
        /// <returns><c>true</c>, if update was phantomed, <c>false</c> otherwise.</returns>
        protected override bool HapticsUpdateCallback()
        {

            HdAPI.hdBeginFrame(LeftPhantomDevice.hHdAPI);
            HandPosition_Left = HapticDevice.GetPosition();
            HandRotation_Left = HapticDevice.GetRotation();

            //HdAPI.hdBeginFrame(RightPhantomDevice.hHdAPI);
            //HandPosition_Right = HapticDevice.GetPosition();
            //HandRotation_Right = HapticDevice.GetRotation();

            // If the user has penetrated the plane, set the device force to 
            // repel the user in the direction of the surface normal of the plane.
            // Penetration occurs if the plane is facing in +Y and the user's Y position
            // is negative, or vice versa.

            if (HandPosition_Left[1] <= 0) //0 la pos en y du plane
            {
                // Create a force vector repelling the user from the plane proportional
                // to the penetration distance, using F=kx where k is the plane 
                // stiffness and x is the penetration vector.  Since the plane is 
                // oriented at the Y=0, the force direction is always either directly 
                // upward or downward, i.e. either (0,1,0) or (0,-1,0).
                double penetrationDistance = Mathf.Abs(HandPosition_Left[1]);

                // Hooke's law explicitly:
                double k = planeStiffness;
                Vector3 f = new Vector3(0, (float)(penetrationDistance * k), 0);

                // If the user applies sufficient force, pop through the plane
                // by reversing its direction.  Otherwise, apply the repel
                // force.
                // NOT USED

                //HdAPI.hdMakeCurrentDevice(LeftPhantomDevice.hHdAPI);
                HapticDevice.SetForce(f);
            }

            HdAPI.hdEndFrame(LeftPhantomDevice.hHdAPI);
            //HdAPI.hdEndFrame(RightPhantomDevice.hHdAPI);

            HandPosition_Left = HandPosition_Left * UnitLength;
            LeftPhantomDevice.position = new Vector3(HandPosition_Left.x, HandPosition_Left.y, HandPosition_Left.z);
            LeftPhantomDevice.rotation = new Quaternion(HandRotation_Left.x, HandRotation_Left.y, HandRotation_Left.z, HandRotation_Left.w);

            /*
            HdAPI.hdBeginFrame(LeftPhantomDevice.hHdAPI);
            HandPosition_Left = HapticDevice.GetPosition();
            HandRotation_Left = HapticDevice.GetRotation();

            HdAPI.hdBeginFrame(RightPhantomDevice.hHdAPI);
            HandPosition_Right = HapticDevice.GetPosition();
            HandRotation_Right = HapticDevice.GetRotation();

            Vector3 pos_diff = new Vector3(HandPosition_Left.x - HandPosition_Right.x, HandPosition_Left.y - HandPosition_Right.y, HandPosition_Left.z - HandPosition_Right.z);
            LeftPhantomDevice.force = ForceField(pos_diff);

            HdAPI.hdMakeCurrentDevice(LeftPhantomDevice.hHdAPI);
            HapticDevice.SetForce(LeftPhantomDevice.force);
            HdAPI.hdEndFrame(LeftPhantomDevice.hHdAPI);

            RightPhantomDevice.force = -1.0f * LeftPhantomDevice.force;
            HdAPI.hdMakeCurrentDevice(RightPhantomDevice.hHdAPI);
            HapticDevice.SetForce(RightPhantomDevice.force);
            HdAPI.hdEndFrame(RightPhantomDevice.hHdAPI);

            HandPosition_Left = HandPosition_Left * UnitLength;
            LeftPhantomDevice.position = new Vector3(HandPosition_Left.x, HandPosition_Left.y, HandPosition_Left.z);
            LeftPhantomDevice.rotation = new Quaternion(HandRotation_Left.x, HandRotation_Left.y, HandRotation_Left.z, HandRotation_Left.w);
            HandPosition_Right = HandPosition_Right * UnitLength;
            RightPhantomDevice.position = new Vector3(HandPosition_Right.x, HandPosition_Right.y, HandPosition_Right.z);
            RightPhantomDevice.rotation = new Quaternion(HandRotation_Right.x, HandRotation_Right.y, HandRotation_Right.z, HandRotation_Right.w);
            */
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
    }
}