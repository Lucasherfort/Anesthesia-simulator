using System.Collections;
using System.Text;
using UnityEngine;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ProbeHapticPlugin : MonoBehaviour
{
    [Header("Configuration Attributes")]

    public string configName = "Default Device";
    public string touchableTagName = "Touchable";
    public bool connect_On_Start = true;       

    [Range(0.0f, 1.0f)]
    public float PhysicsForceStrength = 0.333f; 
    [Range(0.0f, 1.0f)]
    public float PhysicsForceDamping = 0.33f; 

    public bool shapesEnabled = true; 

    public GameObject hapticManipulator = null; 
    public bool PhysicsManipulationEnabled = true;  


    [Header("ReadOnly Attributes")]

    [DisplayOnly]
    public GameObject touching = null; 
    [DisplayOnly]
    public float touchingDepth = 0; 

    [DisplayOnly]
    public int hHD = -1;
    [DisplayOnly]
    public string device_SerialNumber = "-Not Connected-";  
    [DisplayOnly]
    public string device_Model = "-Not Connected-"; 

    [DisplayOnly]
    public Vector3 stylusPositionRaw;   
    [DisplayOnly]
    public Vector3 stylusVelocityRaw;   
    [DisplayOnly]
    public Matrix4x4 stylusTransformRaw;    
    [DisplayOnly]
    public int[] Buttons;              
    [DisplayOnly]
    public int inkwell;                

    [DisplayOnly]
    public Vector3 proxyPositionRaw;           
    [DisplayOnly]
    public Quaternion proxyOrientationRaw;  
    [DisplayOnly]
    public Matrix4x4 proxyTransformRaw;     
    [DisplayOnly]
    public Vector3 proxyNormalRaw;

    #region DLL_Imports
    [DllImport("OHToUnityBridge")]
    public static extern void getVersionString(StringBuilder dest, int len);  

    [DllImport("OHToUnityBridge")]
    public static extern int initDevice(string deviceName); 
    [DllImport("OHToUnityBridge")]
    public static extern void getDeviceSN(string configName, StringBuilder dest, int len); 
    [DllImport("OHToUnityBridge")]
    public static extern void getDeviceModel(string configName, StringBuilder dest, int len);	
    [DllImport("OHToUnityBridge")]
    public static extern void startSchedulers();   

    [DllImport("OHToUnityBridge")]
    public static extern void getWorkspaceArea(string configName, double[] usable6, double[] max6);

    [DllImport("OHToUnityBridge")]
    public static extern void getPosition(string configName, double[] position3);
    [DllImport("OHToUnityBridge")]
    public static extern void getVelocity(string configName, double[] velocity3);
    [DllImport("OHToUnityBridge")]
    public static extern void getTransform(string configName, double[] matrix16);
    [DllImport("OHToUnityBridge")]
    public static extern void getButtons(string configName, int[] buttons4, ref int inkwell);

    [DllImport("OHToUnityBridge")]
    public static extern void setForce(string configName, double[] lateral3, double[] torque3); 

    [DllImport("OHToUnityBridge")]
    public static extern void setSpringAnchorPosition(string configName, double[] position3, double[] velocity3);
 
    [DllImport("OHToUnityBridge")]
    public static extern void setSpringStiffness(string configName, double stiffness, double damping);

    [DllImport("OHToUnityBridge")]
    public static extern void shape_define(int id, string name, double[] ParticleSystemVertexStreams, int[] triangles, int vertCount, int triCount);
    [DllImport("OHToUnityBridge")]
    public static extern void shape_setTransform(int id, double[] matrix16); 
    [DllImport("OHToUnityBridge")]
    public static extern void shape_remove(int id);  
    [DllImport("OHToUnityBridge")]
    public static extern void shape_removeAll(); 

    [DllImport("OHToUnityBridge")]
    public static extern bool shape_getTouched(string configName, ref int shapeID, ref double depth);

    [DllImport("OHToUnityBridge")]
    public static extern void shape_settings(int id, double hlStiffness, double hlDamping, double hlStaticFriction, double hlDynamicFriction, double hlPopThrough);

    [DllImport("OHToUnityBridge")]
    public static extern void shape_constraintSettings(int id, int model, double snapDist);

    [DllImport("OHToUnityBridge")]
    public static extern void shape_flipNormals(int id, bool flipNormals);

    [DllImport("OHToUnityBridge")]
    public static extern void shape_facing(int id, int facing); 

    [DllImport("OHToUnityBridge")]
    public static extern void shape_render(string configName, double[] matrix16);

    [DllImport("OHToUnityBridge")]
    public static extern void shape_enableShapeRendering();
    [DllImport("OHToUnityBridge")]
    public static extern void shape_disableShapeRendering();

    [DllImport("OHToUnityBridge")]
    public static extern void getProxyPosition(string configName, double[] position3);
    [DllImport("OHToUnityBridge")]
    public static extern void getProxyRotation(string configName, double[] quaternion4);
    [DllImport("OHToUnityBridge")]
    public static extern void getProxyTouchNormal(string configName, double[] normal3);
    [DllImport("OHToUnityBridge")]
    public static extern void getProxyTransform(string configName, double[] matrix16);

    [DllImport("OHToUnityBridge")]
    public static extern void effects_resetAll(); 

    [DllImport("OHToUnityBridge")]
    public static extern int effects_assignEffect(string configName);
    [DllImport("OHToUnityBridge")]
    public static extern void effects_startEffect(string configName, int ID);
    [DllImport("OHToUnityBridge")]
    public static extern void effects_stopEffect(string configName, int ID);

    [DllImport("OHToUnityBridge")]
    public static extern void effects_settings(string configName, int ID, double gain, double magnitude, double frequency, double[] position3, double[] direction3);
    [DllImport("OHToUnityBridge")]
    public static extern void effects_deleteEffect(string configName, int ID);

    [DllImport("OHToUnityBridge")]
    public static extern void effects_type(string configName, int ID, int type);

    [DllImport("OHToUnityBridge")]
    public static extern void disconnectAllDevices();

    [DllImport("OHToUnityBridge")]
    public static extern int getHDError(StringBuilder Info, int len);
    [DllImport("OHToUnityBridge")]
    public static extern int getHLError(StringBuilder Info, int len);

    #endregion

    private Queue hapticErrorQueue;

    [System.NonSerialized]
    public double[] max_extents = new double[6];
    [System.NonSerialized]
    public double[] usable_extents = new double[6];

    Matrix4x4 stylusMatrixWorld;
    public Vector3 stylusPositionWorld;
    Vector3 stylusVelocityWorld;
    public Quaternion stylusRotationWorld;

    private GameObject[] touchableObjects;


    private bool safetyMode = false;

    private bool showNoDevicePopup = false;
    private bool showOldVersionPopup = false;

    private bool isIncorrectVersion = false;

    void OnEnable()
    {
        Buttons = new int[4];

        StringBuilder sb = new StringBuilder(256);
        getVersionString(sb, sb.Capacity);
        Debug.Log("Haptic Plugin Version : " + sb.ToString());

        if (connect_On_Start)
        {
            initializeHapticDevice();
        }

        if (hapticManipulator != null)
        {
            Rigidbody body = hapticManipulator.GetComponent<Rigidbody>();
            if (body != null) // Put the cursor on the device so it doesn't jump.
            {
                safeUpdateManipulator();
            }
        }

        //// FIXME
        touchableObjects = GameObject.FindGameObjectsWithTag(touchableTagName) as GameObject[];  //FIXME  Does this fail gracefully?

        hapticErrorQueue = new Queue();

    }
    void Start()
    {
        if (isIncorrectVersion) return;


        startSchedulers();
        if (this.shapesEnabled) setupShapes();

    }

    void OnDestroy()
    {
        if (isIncorrectVersion) return;

        disconnectAllDevices();
    }
    void OnDisable()
    {
        if (isIncorrectVersion) return;

        double[] zero = { 0.0, 0.0, 0.0 };
        setSpringStiffness(configName, 0.0, 0.0);
        setForce(configName, zero, zero);
        shape_removeAll();
        effects_resetAll();
    }

    void OnApplicationQuit()
    {
        if (isIncorrectVersion) return;
        Debug.Log("OnApplicationQuit: Disconnecting from Haptic");


        double[] zero = { 0.0, 0.0, 0.0 };
        setSpringStiffness(configName, 0.0, 0.0);
        setForce(configName, zero, zero);
        shape_removeAll();
        effects_resetAll();

        double[] M = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        shape_render(configName, M);
    }



    bool initializeHapticDevice()
    {
        bool success = false;
        hHD = initDevice(configName);
        if (hHD < 0)
        {
            //Error.
            device_SerialNumber = "-Not Connected-";
            device_Model = "-Not Connected-";
            success = false;
            showNoDevicePopup = true;

            if (hHD == -1001) // Constant indicating incorrect OH Version
            {
                showOldVersionPopup = true;
                isIncorrectVersion = true;
                hapticErrorQueue.Enqueue(System.DateTime.Now.ToLongTimeString() + " - " + "Incorrect Open Haptic Version.");
            }


        }
        else
        {
            {
                StringBuilder sb = new StringBuilder(256);
                getDeviceSN(configName, sb, sb.Capacity);
                device_SerialNumber = sb.ToString();
            }
            {
                StringBuilder sb = new StringBuilder(256);
                getDeviceModel(configName, sb, sb.Capacity);
                device_Model = sb.ToString();
            }

            getWorkspaceArea(configName, usable_extents, max_extents);

            success = true;
            showNoDevicePopup = false;
            isIncorrectVersion = false;
        }

        return success;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isIncorrectVersion) return;
        checkErrors();

        if (hHD < 0)
            return;

        double[] M16 = MatrixToDoubleArray(this.transform.worldToLocalMatrix);
        shape_render(configName, M16);

        if (shapesEnabled)
        {
            
            updateShapes();

            int shapeID = -1;
            double depth = 0.0;

            if (shape_getTouched(configName, ref shapeID, ref depth))
            {
                
                if (touching == null || touching.GetInstanceID() != shapeID)
                {
                    for (int ii = 0; ii < touchableObjects.Length; ii++)
                    {
                        if (shapeID == touchableObjects[ii].GetInstanceID())
                        {
                            touching = touchableObjects[ii];
                            break;
                        }
                    }
                }
            }
            else
                touching = null;

            touchingDepth = (float)depth;                 
        }

        if (safetyMode)
        {
            safeUpdateManipulator();
            return;
        }

        // Pull the data from the device
        updateDevice();

        // Update the gamestate

        //Update the physics manipulator and whatnot.
        updateManipulator();

        checkErrors();
    }

    public void startSafetyMode()
    {
        safeUpdateManipulator();
        safetyMode = true;
    }

    public void endSafetyMode()
    {
        safetyMode = false;
    }
    public bool isInSafetyMode()
    {
        return safetyMode;
    }


    private static Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
    }



    private void checkErrors()
    {
        StringBuilder sb = new StringBuilder(256);
        int code = getHDError(sb, sb.Capacity);
        if (code != 0)
        {
            Debug.LogError("Haptic(" + configName + ") HD_Error : " + code + ", " + sb.ToString());
            hapticErrorQueue.Enqueue(System.DateTime.Now.ToLongTimeString() + " - " + "Haptic(" + configName + ") HD_Error : " + code + ", " + sb.ToString());
        }

        code = getHLError(sb, sb.Capacity);
        if (code != 0)
        {
            Debug.LogError("Haptic(" + configName + ") HL_Error : " + code + ", " + sb.ToString());
            hapticErrorQueue.Enqueue(System.DateTime.Now.ToLongTimeString() + " - " + "Haptic(" + configName + ") HL_Error : " + code + ", " + sb.ToString());
        }

        while (hapticErrorQueue.Count > 100)
            hapticErrorQueue.Dequeue();
    }

    private void updateDevice()
    {
        if (isIncorrectVersion) return;

        // Retrieve the raw values
        double[] posInput = new double[3];
        double[] velInput = new double[3];
        getPosition(configName, posInput);
        getVelocity(configName, velInput);

        stylusPositionRaw.x = (float)posInput[0];
        stylusPositionRaw.y = (float)posInput[1];
        stylusPositionRaw.z = (float)posInput[2];

        stylusVelocityRaw.x = (float)velInput[0];
        stylusVelocityRaw.y = (float)velInput[1];
        stylusVelocityRaw.z = (float)velInput[2];

        double[] oriInput = new double[4];
        getProxyPosition(configName, posInput);
        proxyPositionRaw.x = (float)posInput[0];
        proxyPositionRaw.y = (float)posInput[1];
        proxyPositionRaw.z = (float)posInput[2];

        getProxyRotation(configName, oriInput);
        proxyOrientationRaw.x = (float)oriInput[0];
        proxyOrientationRaw.y = (float)oriInput[1];
        proxyOrientationRaw.z = (float)oriInput[2];
        proxyOrientationRaw.w = (float)oriInput[3];


        {
            double[] matInput = new double[16];
            getTransform(configName, matInput);
            Matrix4x4 mat;
            mat.m00 = (float)matInput[0];
            mat.m01 = (float)matInput[1];
            mat.m02 = (float)matInput[2];
            mat.m03 = (float)matInput[3];
            mat.m10 = (float)matInput[4];
            mat.m11 = (float)matInput[5];
            mat.m12 = (float)matInput[6];
            mat.m13 = (float)matInput[7];
            mat.m20 = (float)matInput[8];
            mat.m21 = (float)matInput[9];
            mat.m22 = (float)matInput[10];
            mat.m23 = (float)matInput[11];
            mat.m30 = (float)matInput[12];
            mat.m31 = (float)matInput[13];
            mat.m32 = (float)matInput[14];
            mat.m33 = (float)matInput[15];
            stylusTransformRaw = mat.transpose;
        }
        {
            double[] matInput = new double[16];
            getProxyTransform(configName, matInput);
            Matrix4x4 mat;
            mat.m00 = (float)matInput[0];
            mat.m01 = (float)matInput[1];
            mat.m02 = (float)matInput[2];
            mat.m03 = (float)matInput[3];
            mat.m10 = (float)matInput[4];
            mat.m11 = (float)matInput[5];
            mat.m12 = (float)matInput[6];
            mat.m13 = (float)matInput[7];
            mat.m20 = (float)matInput[8];
            mat.m21 = (float)matInput[9];
            mat.m22 = (float)matInput[10];
            mat.m23 = (float)matInput[11];
            mat.m30 = (float)matInput[12];
            mat.m31 = (float)matInput[13];
            mat.m32 = (float)matInput[14];
            mat.m33 = (float)matInput[15];
            proxyTransformRaw = mat.transpose;
        }



        getButtons(configName, Buttons, ref inkwell);


        // Cook the raw values.
        if (this.shapesEnabled)
        {
            stylusMatrixWorld = gameObject.transform.localToWorldMatrix * proxyTransformRaw;
            shape_enableShapeRendering();
        }
        else
        {
            stylusMatrixWorld = gameObject.transform.localToWorldMatrix * stylusTransformRaw;
            shape_disableShapeRendering();
        }
        stylusPositionWorld = stylusMatrixWorld.GetColumn(3);
        stylusRotationWorld = QuaternionFromMatrix(stylusMatrixWorld);

        stylusVelocityWorld = gameObject.transform.InverseTransformVector(stylusVelocityRaw);

    }


    private GameObject previousManipulator = null;

    private void updateManipulator()
    {
        if (this.hapticManipulator == null)
        {
            double[] zero = { 0.0, 0.0, 0.0 };
            setSpringStiffness(configName, 0.0, 0.0);
            return;
        }

        if (PhysicsManipulationEnabled == false || hapticManipulator != previousManipulator)
        { // No physics, just move it.
            hapticManipulator.transform.rotation = stylusRotationWorld;
            hapticManipulator.transform.position = stylusPositionWorld;
            setSpringStiffness(configName, 0.0, 0.0);
            previousManipulator = hapticManipulator;
            return;
        }
        previousManipulator = hapticManipulator;

        Rigidbody body = hapticManipulator.GetComponent<Rigidbody>();

        // FIXME! Why 10? Determine this from bounding sphere.
        if ((body.position - stylusPositionWorld).magnitude > 10)
        {
            body.position = stylusPositionWorld;
            body.velocity = stylusVelocityWorld;
            body.rotation = stylusRotationWorld;
        }
        Vector3 springPos = gameObject.transform.InverseTransformPoint(this.hapticManipulator.transform.position);
        double[] springPosOut = new double[3];
        springPosOut[0] = springPos.x;
        springPosOut[1] = springPos.y;
        springPosOut[2] = springPos.z;

        Vector3 springVel = gameObject.transform.InverseTransformVector(body.velocity);
        double[] springVelOut = new double[3];
        springVelOut[0] = springVel.x;
        springVelOut[1] = springVel.y;
        springVelOut[2] = springVel.z;
        setSpringAnchorPosition(configName, springPosOut, springVelOut);
        setSpringStiffness(configName, this.PhysicsForceStrength, 0);


        {
            // Idea : (Could we take velocity from the device?)

            Vector3 force = new Vector3();
            force = (stylusPositionWorld) - body.position;
            force *= body.mass;
            force *= 1000;
            force *= (0.15f); // Magic number So that manupilator with mass 1.0 feels right.

            Vector3 V = new Vector3();
            V = (body.velocity);
            V *= V.magnitude;

            force -= (body.mass * V * this.PhysicsForceDamping);


            body.AddForce(force);
        }

        //Rotation

        {
            // Apply rotation to this item.
            Vector3 torque = DetermineTorque(body);

            body.AddTorque(torque, ForceMode.VelocityChange);

            // Apply rotation to anything we're grabbing.
            FixedJoint[] joints = hapticManipulator.GetComponentsInChildren<FixedJoint>();
            foreach (FixedJoint J in joints)
            {
                if (J.connectedBody != null)
                {
                    J.connectedBody.AddTorque(torque, ForceMode.VelocityChange);
                }
            }
        }
    }

    private Vector3 DetermineTorque(Rigidbody body)
    {
        if (body == null)
            return Vector3.zero;

        Quaternion AngleDifference = stylusRotationWorld * Quaternion.Inverse(body.rotation);

        float AngleToCorrect = Quaternion.Angle(body.rotation, stylusRotationWorld);
        Vector3 Perpendicular = Vector3.Cross(transform.up, transform.forward);
        if (Vector3.Dot(stylusRotationWorld * Vector3.forward, Perpendicular) < 0)
            AngleToCorrect *= -1;
        Quaternion Correction = Quaternion.AngleAxis(AngleToCorrect, transform.up);

        Vector3 MainRotation = RectifyAngleDifference((AngleDifference).eulerAngles);
        Vector3 CorrectiveRotation = RectifyAngleDifference((Correction).eulerAngles);

        Vector3 torque = ((MainRotation - CorrectiveRotation / 2) - body.angularVelocity);
        return torque;
    }
    private Vector3 RectifyAngleDifference(Vector3 angdiff)
    {
        if (angdiff.x > 180) angdiff.x -= 360;
        if (angdiff.y > 180) angdiff.y -= 360;
        if (angdiff.z > 180) angdiff.z -= 360;
        return angdiff;
    }


    // Put the cursor on the device so it doesn't jump.
    public void safeUpdateManipulator()
    {
        if (isIncorrectVersion) return;

        //Debug.unityLogger.Log("Safing the Manipulator.");

        if (hapticManipulator == null)
            return;
        Rigidbody body = hapticManipulator.GetComponent<Rigidbody>();
        if (body == null)
            return;
        updateDevice();

        {
            body.position = stylusPositionWorld;
            body.rotation = stylusRotationWorld;
            body.velocity = Vector3.zero;
        }

        double[] zero = { 0.0, 0.0, 0.0 };
        setSpringStiffness(configName, 0.0, 0.0);
        setForce(configName, zero, zero);
    }



    void setupShapes()
    {
        if (isIncorrectVersion) return;

        touchableObjects = GameObject.FindGameObjectsWithTag(touchableTagName) as GameObject[];

        for (int ii = 0; ii < touchableObjects.Length; ii++)
        {
            int shapeID = touchableObjects[ii].GetInstanceID();
            string name = touchableObjects[ii].name;

            GameObject go = touchableObjects[ii];
            Mesh mesh = null;

            // If the object has a collision mesh, use that.
            MeshCollider collider = go.GetComponent<MeshCollider>();
            if (collider != null)
            {
                mesh = collider.sharedMesh;
            }
            if (mesh == null)
            {
                MeshFilter filter = go.GetComponent<MeshFilter>();
                if (filter != null)
                    mesh = filter.mesh;
            }

            // Vectors need to be converted to array of primatives. 
            // Triangles already are an array of ints.
            if (mesh != null)
            {
                double[] vertices = Vector3ArrayToDoubleArray(mesh.vertices);
                int[] triangles = mesh.triangles;

                shape_define(shapeID, name, vertices, triangles, vertices.Length, triangles.Length);
            }

        } //for(ii)

        // TODO Update the transforms of all shapes.

    }

    void updateShapes()
    {
        if (isIncorrectVersion) return;

        GameObject[] myObjects = GameObject.FindGameObjectsWithTag(touchableTagName) as GameObject[];

        for (int ii = 0; ii < myObjects.Length; ii++)
        {
            int shapeID = myObjects[ii].GetInstanceID();

            Matrix4x4 M = myObjects[ii].transform.localToWorldMatrix;

            M = this.transform.worldToLocalMatrix * M;

            double[] M16 = MatrixToDoubleArray(M);

            shape_setTransform(shapeID, M16);
        }
    }

    //Convert Vector3[] to double[]  
    //Used by setupShapes to cook the vertex array of a mesh.
    private static double[] Vector3ArrayToDoubleArray(Vector3[] inVectors)
    {
        double[] outDoubles = new double[inVectors.Length * 3];

        for (int ii = 0; ii < inVectors.Length; ii++)
        {
            outDoubles[3 * ii + 0] = inVectors[ii].x;
            outDoubles[3 * ii + 1] = inVectors[ii].y;
            outDoubles[3 * ii + 2] = inVectors[ii].z;
        }

        return outDoubles;
    }
    //Convert Matrix4x4 to double[]  
    private static double[] MatrixToDoubleArray(Matrix4x4 M)
    {
        double[] out16 = new double[16];

        out16[0] = M.m00;
        out16[1] = M.m10;
        out16[2] = M.m20;
        out16[3] = M.m30;

        out16[4] = M.m01;
        out16[5] = M.m11;
        out16[6] = M.m21;
        out16[7] = M.m31;

        out16[8] = M.m02;
        out16[9] = M.m12;
        out16[10] = M.m22;
        out16[11] = M.m32;

        out16[12] = M.m03;
        out16[13] = M.m13;
        out16[14] = M.m23;
        out16[15] = M.m33;
        /*
		out16 [0] = M.m00;
		out16 [1] = M.m01;
		out16 [2] = M.m02;
		out16 [3] = M.m03;

		out16 [4] = M.m10;
		out16 [5] = M.m11;
		out16 [6] = M.m12;
		out16 [7] = M.m13;

		out16 [8] = M.m20;
		out16 [9] = M.m21;
		out16 [10] =M.m22;
		out16 [11] =M.m23;

		out16 [12] =M.m30;
		out16 [13] =M.m31;
		out16 [14] =M.m32;
		out16 [15] =M.m33;*/

        return out16;
    }


    public string[] retrieveErrorList()
    {
        string[] output = new string[hapticErrorQueue.Count];
        hapticErrorQueue.CopyTo(output, 0);
        return output;
    }
    public void clearErrorQueue()
    {
        hapticErrorQueue.Clear();
    }


    private Rect windowRect = new Rect(20, 20, 350, 125);
    void OnGUI()
    {
        if (showOldVersionPopup)
            windowRect = GUI.Window(0, windowRect, OldVersionDialogWindow, "Out-of-date OpenHaptics version.");
        else if (showNoDevicePopup)
            windowRect = GUI.Window(0, windowRect, NoDeviceDialogWindow, "Haptic Device Not Found.");

    }

    // This is the actual window.
    void NoDeviceDialogWindow(int windowID)
    {
        float y = 30;

        GUI.Label(new Rect(5, y, windowRect.width, 20), "Could not find haptic device named \"" + configName + "\""); y += 30;

        if (GUI.Button(new Rect(windowRect.width * 0.33f, y, windowRect.width * 0.33f, 20), "OK"))
        {
            showNoDevicePopup = false;
        }
        y += 30;

        if (GUI.Button(new Rect(windowRect.width * 0.33f, y, windowRect.width * 0.33f, 20), "Exit"))
        {
            Application.Quit();
            showNoDevicePopup = false;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        y += 30;
    }
    void OldVersionDialogWindow(int windowID)
    {
        // Get Version String
        StringBuilder sb = new StringBuilder(256);
        getVersionString(sb, sb.Capacity);

        string ver = sb.ToString().Substring(sb.ToString().IndexOf("OpenHapticsVersion=") + "OpenHapticsVersion=".Length);


        float y = 30;

        GUI.Label(new Rect(5, y, windowRect.width, 20), "Unity Plugin requires OpenHaptics version 3.50.0"); y += 30;
        GUI.Label(new Rect(5, y, windowRect.width, 20), "(Current OH Version : " + ver + ")"); y += 30;

        if (GUI.Button(new Rect(windowRect.width * 0.33f, y, windowRect.width * 0.33f, 20), "Exit"))
        {
            Application.Quit();
            showNoDevicePopup = false;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        y += 30;

    }



#if UNITY_EDITOR
    // In editor Gizmos
    void OnDrawGizmos()
    {
        if (hHD >= 0)
        {
            // Draw Extants
            {
                const int minX = 0;
                const int minY = 1;
                const int minZ = 2;
                const int maxX = 3;
                const int maxY = 4;
                const int maxZ = 5;

                Vector3 usableBox = new Vector3(
                                        (float)(usable_extents[maxX] - usable_extents[minX]),
                                        (float)(usable_extents[maxY] - usable_extents[minY]),
                                        (float)(usable_extents[maxZ] - usable_extents[minZ]));
                Vector3 usableCenter = new Vector3(
                                           0.5f * (float)(usable_extents[maxX] + usable_extents[minX]),
                                           0.5f * (float)(usable_extents[maxY] + usable_extents[minY]),
                                           0.5f * (float)(usable_extents[maxZ] + usable_extents[minZ]));

                Gizmos.color = Color.green;
                Gizmos.matrix = gameObject.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(usableCenter, usableBox);

                Vector3 maxBox = new Vector3(
                                     (float)(max_extents[maxX] - max_extents[minX]),
                                     (float)(max_extents[maxY] - max_extents[minY]),
                                     (float)(max_extents[maxZ] - max_extents[minZ]));
                Vector3 maxCenter = new Vector3(
                                        0.5f * (float)(max_extents[maxX] + max_extents[minX]),
                                        0.5f * (float)(max_extents[maxY] + max_extents[minY]),
                                        0.5f * (float)(max_extents[maxZ] + max_extents[minZ]));


                Gizmos.color = Color.yellow;
                Gizmos.matrix = gameObject.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(maxCenter, maxBox);
            }

            // Draw Stylus!
            Gizmos.color = Color.white;
            Gizmos.matrix = gameObject.transform.localToWorldMatrix * stylusTransformRaw;
            Gizmos.DrawWireSphere(Vector3.zero, 10);
            Gizmos.DrawWireCube(new Vector3(0, 0, 20), new Vector3(5, 5, 40));

            // Draw Buttons

            if (Buttons[0] == 1)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(new Vector3(0, 0, 5), 2f);
            if (Buttons[1] >= 0)
            {
                if (Buttons[1] == 1)
                    Gizmos.color = Color.green;
                else
                    Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(new Vector3(0, 0, 10), 2f);
            }
        }
        else
        {
            // Else no connection.
            Vector3 OmniBox = new Vector3(160, 120, 70);
            Gizmos.color = Color.grey;
            Gizmos.matrix = gameObject.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, OmniBox);


        }
    } //OnDrawGizmos()
#endif

} //class HapticPlugin





public class ProbeDisplayOnlyAttribute : PropertyAttribute
{

}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(DisplayOnlyAttribute))]
public class ProbeReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property,
        GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position,
        SerializedProperty property,
        GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif




