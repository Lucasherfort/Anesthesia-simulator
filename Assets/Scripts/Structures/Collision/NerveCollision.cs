using UnityEngine;

public class NerveCollision : MonoBehaviour
{
    static public NerveCollision Instance { get; private set; }

    public bool NerveIsTouch = false;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Needle")
        {
            NerveIsTouch = true;
            CanvasEchographe.Instance.UpdateTouchNerve();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Needle")
        {
            NerveIsTouch = false;
        }
    }
}
