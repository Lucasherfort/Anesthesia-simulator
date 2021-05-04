using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRManager : MonoBehaviour
{
    [SerializeField]
    private bool EnabledVR = true;

    private void Start()
    {
        UnityEngine.XR.XRSettings.enabled = EnabledVR;
    }
}
