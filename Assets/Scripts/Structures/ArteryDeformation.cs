using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArteryDeformation : MonoBehaviour
{
    [SerializeField]
    private Transform artery = null;

    [SerializeField]
    [Min(0)]
    private int pulseSpeed = 1;

    [SerializeField]
    [Range(0f,1f)]
    private float deformationFactor = 0.9f;

    private float MaxScale;

    private void Start()
    {
        if (artery == null)
        {
            Debug.LogWarning("artery variable is empty !");
        }
        else
        {
            MaxScale = artery.localScale.x;
        }
    }

    private void Update()
    {
        var temp = artery.localScale;
        temp.x = Map(Mathf.Cos(Time.time * pulseSpeed), -1.0f, 1.0f, MaxScale * deformationFactor, MaxScale);
        temp.y = Map(Mathf.Cos(Time.time * pulseSpeed), -1.0f, 1.0f, MaxScale * deformationFactor, MaxScale);
        artery.localScale = temp;
    }

    private float Map(float variable, float x1, float x2, float y1, float y2)
    {
        float a = (y1 - y2) / (x1 - x2);
        float b = y1 - x1 * ((y1 - y2) / (x1 - x2));

        return a * variable + b;
    }
}
