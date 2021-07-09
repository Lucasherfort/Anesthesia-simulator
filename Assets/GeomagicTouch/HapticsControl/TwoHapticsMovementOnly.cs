
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

/// <summary>
/// Needle Insertion Prototype
/// </summary>
public class TwoHapticsMovementOnly : MonoBehaviour
{
    //---------------------------------------------------------------------------
    // CLASS INSTANCE
    //---------------------------------------------------------------------------

    /// <summary>
    /// class instance object -singleton-
    /// </summary>
	public static TwoHapticsMovementOnly instance;

    //---------------------------------------------------------------------------
    // HAPTIC INFORMATION
    //---------------------------------------------------------------------------

    /// <summary>
    /// PHANTOM instance
    /// </summary>
    private PhantomUnityController Phantoms = null;

    /// <summary>
    /// Struct containing information attached to one device
    /// </summary>
    
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
    private Vector3 HandPosition_Left = Vector3.zero;

    /// <summary>
    /// The gimbal position [mm]
    /// </summary>
    private Vector3 HandPosition_Right = Vector3.zero;

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
                LeftPhantomDevice.Name = "PHANToM 1";
                RightPhantomDevice.Name = "PHANToM 2";

                Debug.Log("Initializing phantoms");

                // Instantiation of Phantoms
                Phantoms = new PhantomUnityController();

                try
                {
                    LeftPhantomDevice.hHdAPI = Phantoms.InitDevice(LeftPhantomDevice.Name);
                    RightPhantomDevice.hHdAPI = Phantoms.InitDevice(RightPhantomDevice.Name);
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

        if (Phantoms == null)
        {
            //TODO this is awful! :S
            Debug.Log("ERROR INITIALIZING DEVICE...");
            return;
        }

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
            instance = this;
        else
            Debug.Log("Multiple instances of HapticManager");

        // Attach gameobjects to devices
        LeftPhantomDevice.tool = GameObject.Find("Device_1");
        RightPhantomDevice.tool = GameObject.Find("Device_2");

        // Initialization of hand position and orientation
        LeftPhantomDevice.position = Vector3.zero;
        RightPhantomDevice.position = Vector3.zero;
        LeftPhantomDevice.rotation = Quaternion.identity;
        RightPhantomDevice.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Process each frame
    /// </summary>
    private void Update()
    {
        Phantoms.Do(PhantomUpdatePositions);

        LeftPhantomDevice.tool.transform.localPosition = LeftPhantomDevice.position * UnitLength;
        RightPhantomDevice.tool.transform.localPosition = RightPhantomDevice.position * UnitLength;
        LeftPhantomDevice.tool.transform.localRotation = LeftPhantomDevice.rotation;
        RightPhantomDevice.tool.transform.localRotation = RightPhantomDevice.rotation;
    }

    bool PhantomUpdatePositions()
    {
        HdAPI.hdMakeCurrentDevice(LeftPhantomDevice.hHdAPI);
        LeftPhantomDevice.position = Phantoms.GetPosition();
        LeftPhantomDevice.rotation = Phantoms.GetRotation();

        HdAPI.hdMakeCurrentDevice(RightPhantomDevice.hHdAPI);
        RightPhantomDevice.position = Phantoms.GetPosition();
        RightPhantomDevice.rotation = Phantoms.GetRotation();

        return false;
    }


    /// <summary>
    /// Method that is repeatedly called in PHANTOM's cycle (default rate 1 [kHz])
    /// </summary>
    /// <returns><c>true</c>, if update was phantomed, <c>false</c> otherwise.</returns>
    bool PhantomUpdate()
    {

        HdAPI.hdBeginFrame(LeftPhantomDevice.hHdAPI);
        HandPosition_Left = Phantoms.GetPosition();

        HdAPI.hdBeginFrame(RightPhantomDevice.hHdAPI);
        HandPosition_Right = Phantoms.GetPosition();

        Vector3 pos_diff = new Vector3(HandPosition_Left.x - HandPosition_Right.x, HandPosition_Left.y - HandPosition_Right.y, HandPosition_Left.z - HandPosition_Right.z);
        LeftPhantomDevice.force = ForceField(pos_diff);

        HdAPI.hdMakeCurrentDevice(LeftPhantomDevice.hHdAPI);
        Phantoms.SetForce(LeftPhantomDevice.force);
        HdAPI.hdEndFrame(LeftPhantomDevice.hHdAPI);

        RightPhantomDevice.force = -1.0f * LeftPhantomDevice.force;
        HdAPI.hdMakeCurrentDevice(RightPhantomDevice.hHdAPI);
        Phantoms.SetForce(RightPhantomDevice.force);
        HdAPI.hdEndFrame(RightPhantomDevice.hHdAPI);

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
