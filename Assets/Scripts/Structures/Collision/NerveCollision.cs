using UnityEngine;

public class NerveCollision : MonoBehaviour
{
    static public NerveCollision Instance { get; private set; }

    [HideInInspector]
    public bool NerveIsTouch = false;

    private AudioBox audioBox;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        audioBox = GetComponent<AudioBox>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Needle")
        {
            NerveIsTouch = true;
            audioBox.StopAll();
            audioBox.PlayOneShot(SoundOneShot.CryPain);
            CanvasEchographe.Instance.UpdateTouchNerve();
            AnestheticManager.Instance.RemoveAnesthesic(10);
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
