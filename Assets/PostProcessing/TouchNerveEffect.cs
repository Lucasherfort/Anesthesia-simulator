using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class TouchNerveEffect : MonoBehaviour
{
    private Volume Volume;

    private void Start()
    {
        Volume = GetComponent<Volume>();
    }
}
