using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    private float maxY = 0.0468f;

    private float mixScaleY = 0.004f;
    private float maxScaleY = 0.009f;

    [SerializeField]
    [Range(0f,0.0468f)]
    private float localY = 0;

    private float ScalelocalY = 0;

    void Update()
    {
        ScalelocalY = ((mixScaleY - maxScaleY) / maxY)* localY + maxScaleY;

        Vector3 temp = transform.localScale;
        temp.y = ScalelocalY;
        transform.localScale = temp;

        Vector3 temp2 = transform.localPosition;
        temp2.y = localY;
        transform.localPosition = temp2;
    }

    private float Map(float variable, float x1, float x2, float y1, float y2)
    {
        float a = (y1 - y2) / (x1 - x2);
        float b = y1 - x1 * ((y1 - y2) / (x1 - x2));

        return a*variable + b;
    }

}
