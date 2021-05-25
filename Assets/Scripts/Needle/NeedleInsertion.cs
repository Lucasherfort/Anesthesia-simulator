using System;
using UnityEngine;

public class NeedleInsertion : MonoBehaviour
{
    private HapticPlugin[] devices;
    private HapticPlugin probeDevice;
    private HapticPlugin needleDevice;

    [SerializeField]
    GameObject NeedlePointer = null;
    [SerializeField]
    GameObject probePointer = null;
    bool inContactwithNeedle = false;
    Vector3 position;
    [SerializeField]
    float resistance;

    private void Start()
    { 

        devices = (HapticPlugin[])FindObjectsOfType(typeof(HapticPlugin));

        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].hapticManipulator == NeedlePointer)
            {
                needleDevice = devices[i];         
            }
            else if (devices[i].hapticManipulator == probePointer)
            {
                probeDevice = devices[i];
            }
        }
        probeDevice.shapesEnabled = true;
        needleDevice.shapesEnabled = true;
    }

    void Update()
    {
        if (inContactwithNeedle)
        {
            try
            {
                if (probeDevice != null)
                {
                    probeDevice.shapesEnabled = false;

                }
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

    }
    private void OnCollisionEnter(Collision collision)
    {

    }

    private void OnCollisionStay(Collision collision)
    {
        Debug.Log("CollisionStay");
        if (collision.gameObject.CompareTag("NeedlePointer"))
        {
            Debug.Log("TagNeedlePointer");
            position = collision.gameObject.transform.position;
            Debug.Log("position" + this.transform.position.y);

            // If TouchingDepth doesn't work properly
            if (needleDevice.touchingDepth == 0)
            {
                if (probeDevice.touchingDepth > resistance)
                {
                    inContactwithNeedle = true;
                    Debug.Log("InContactWithNeedle");
                }
            }
            else
            {//if it works properly
                if (needleDevice.touchingDepth > resistance)
                {
                    inContactwithNeedle = true;
                    Debug.Log("InContactWithNeedle");
                }
            }
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("NeedlePointer"))
        {
            inContactwithNeedle = false;
            if (probeDevice != null)
            {
                probeDevice.shapesEnabled = true;
            }

            Debug.Log("NotInContact");
        }
    }
}
