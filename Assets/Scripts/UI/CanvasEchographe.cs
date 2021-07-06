using UnityEngine;
using System;

public class CanvasEchographe : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text DateAndHourTxt;

    [SerializeField]
    private TMPro.TMP_Text ZoomTxt;

    [SerializeField]
    private ZoomScreen zoomScreen;

    private void Update()
    {
        DateAndHourTxt.text = System.DateTime.Now.ToString();
        ZoomTxt.text = "Zoom : "+zoomScreen.currentZoom.ToString();
    }
}
