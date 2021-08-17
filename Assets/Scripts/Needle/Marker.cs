using UnityEngine;

public class Marker : MonoBehaviour
{
    [SerializeField]
    private Transform marker = null;

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Skin")
        {
            marker.transform.position = TwoHapticsProbeNeedle.instance.NeedleDevice.transform.position;

            var temp = marker.transform.position;
            temp.y = 1.181488f;
            marker.transform.position = temp;

            marker.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if(col.gameObject.tag == "Skin")
        {
            marker.gameObject.SetActive(false);
        }
    }
}
