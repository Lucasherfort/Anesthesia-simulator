using UnityEngine;

public class AnestheticManager : MonoBehaviour
{
    private bool NeedleInsideArea = false;

    private void Start()
    {
        TwoHapticsProbeNeedle.instance.InsertAnesthesic += ApplyAnesthesic;
    }

    public void ApplyAnesthesic()
    {
        if(NeedleInsideArea && !NerveCollision.Instance.NerveIsTouch)
        {
            Debug.Log("INJECTION !!!!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Needle")
        {
            NeedleInsideArea = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Needle")
        {
            NeedleInsideArea = false;
        }
    }
}
