using UnityEngine;

public class NeedleInsertion : MonoBehaviour
{
    private HapticPlugin[] devices;
    private HapticPlugin sondeDevice;
    private HapticPlugin needleDevice;

    [SerializeField]
    GameObject NeedlePointer = null;
    [SerializeField]
    GameObject echoPointer = null;

    private void Start()
    {
        devices = (HapticPlugin[])FindObjectsOfType(typeof(HapticPlugin));

        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].hapticManipulator == NeedlePointer)
            {
                needleDevice = devices[i];         
            }
            else if (devices[i].hapticManipulator == echoPointer)
            {
                sondeDevice = devices[i];

            }
        }
        sondeDevice.shapesEnabled = true;
        needleDevice.shapesEnabled = true;
    }
}
