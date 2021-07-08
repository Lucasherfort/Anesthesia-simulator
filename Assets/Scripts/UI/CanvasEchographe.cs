using UnityEngine;
using System;

public class CanvasEchographe : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text DateAndHourTxt = null;

    [SerializeField]
    private TMPro.TMP_Text ZoomTxt = null;

    [SerializeField]
    private ZoomScreen zoomScreen = null;

    private void Update()
    {
        DateAndHourTxt.text = System.DateTime.Now.ToString();
        ZoomTxt.text = "Zoom : "+zoomScreen.currentZoom.ToString();
    }
}
