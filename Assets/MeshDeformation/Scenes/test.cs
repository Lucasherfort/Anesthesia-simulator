using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public float minY = 0f;
    public float maxY = 0f;

    public float mixScaleY = 0f;
    public float maxScaleY = 0f;

    public float localY = 0;
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
