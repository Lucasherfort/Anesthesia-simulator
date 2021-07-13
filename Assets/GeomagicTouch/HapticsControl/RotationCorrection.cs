using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationCorrection : MonoBehaviour
{
    public PhantomDeviceInfo needleInfo;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (needleInfo.inside)
        {
            transform.localPosition = needleInfo.correctionPosition;
            transform.localRotation = needleInfo.correctionRotation;
        }
    }
}
