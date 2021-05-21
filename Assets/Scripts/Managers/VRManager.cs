using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRManager : MonoBehaviour
{
    [SerializeField]
    private bool EnabledVR = false;

    private void Update()
    {
        UnityEngine.XR.XRSettings.enabled = EnabledVR;
    }
}
