using UnityEngine;

public class AnestheticManager : MonoBehaviour
{
    static public AnestheticManager Instance{get; private set;}

    private bool NeedleInsideArea = false;

    private int StateAnesthesia = 0;
    private bool SuccessfulAnesthesia;

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
        TwoHapticsProbeNeedle.instance.InsertAnesthesic += ApplyAnesthesic;
    }

    public void ApplyAnesthesic(int amount)
    {
        if(NeedleInsideArea && !NerveCollision.Instance.NerveIsTouch && !SuccessfulAnesthesia)
        {
            StateAnesthesia += amount;

            if(StateAnesthesia < 0)
            {
                StateAnesthesia = 0;
            }

            if(StateAnesthesia == 100)
            {
                SuccessfulAnesthesia = true;
                StopCoroutine(CanvasEchographe.Instance.UpdateTimer());
                DataRecorder.Instance.SaveData(CanvasEchographe.Instance.timePlaying,CanvasEchographe.Instance.NbNerveTouch,CanvasEchographe.Instance.NbVeinTouch,CanvasEchographe.Instance.NbArteryTouch);
            }

            CanvasEchographe.Instance.UpdateUIStateAnesthesia(StateAnesthesia);
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
