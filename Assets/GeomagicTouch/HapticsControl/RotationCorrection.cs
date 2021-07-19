using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationCorrection : MonoBehaviour
{
    public PhantomDeviceInfo needleInfo;
    public bool previousStateInsertion = false;
    public Transform needle;
    public Vector3 oldPosition;

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
        return;


        if (previousStateInsertion != needleInfo.inside)
        {   // state change
            if (needleInfo.inside)
            {
                transform.localPosition = needleInfo.correctionPosition;
                transform.localRotation = needleInfo.correctionRotation;
                needle.parent = transform;
                oldPosition = needle.localPosition;
            }
            else
            {
                needle.parent = transform.parent;
            }
            previousStateInsertion = needleInfo.inside;
        }

        if (needleInfo.inside)
        {
            transform.localPosition = needleInfo.correctionPosition;
            transform.localRotation = needleInfo.correctionRotation;
            //needle.localPosition = needle.position;
        }

        if (Input.GetKey(KeyCode.F))
        {
            needle.localPosition = needleInfo.position - needleInfo.correctionPosition;
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            needle.localPosition = oldPosition;
        }
    }
}
