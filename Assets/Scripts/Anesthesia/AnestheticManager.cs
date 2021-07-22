using UnityEngine;

public class AnestheticManager : MonoBehaviour
{
    static public AnestheticManager Instance{get; private set;}

    private bool NeedleInsideArea = false;

    private bool NeedleIsUp = false;
    private bool NeedleIsDown = false;

    private int StateUp = 0;
    private int StateDown = 0;

    private bool SuccessfulAnesthesia;

    [SerializeField]
    private Transform Needle = null;

    [SerializeField]
    private Transform AnesthesiaFeedback = null;

    public float minScale;
    public float maxScale;

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
            if(Needle.position.y > transform.position.y)
            {
                StateUp += 20;

                if(StateUp >= 100)
                {
                    StateUp = 100;
                }

                CanvasEchographe.Instance.UpdateUIUpAnesthesia(StateUp);
            }
            else
            {
                StateDown += 20;
                if (StateDown >= 100)
                {
                    StateDown = 100;
                }

                CanvasEchographe.Instance.UpdateUIDownAnesthesia(StateDown);
            }

            Vector3 temp = transform.localScale;
            var temp2 = Map(StateDown + StateUp, 0, 200, minScale, maxScale);
            temp.x = temp2;
            temp.y = temp2;

            AnesthesiaFeedback.transform.localScale = temp;

            if (StateUp + StateDown == 200)
            {
                SuccessfulAnesthesia = true;
                CanvasEchographe.Instance.StopTimer();
                DataRecorder.Instance.SaveData(CanvasEchographe.Instance.timePlaying,CanvasEchographe.Instance.NbNerveTouch,CanvasEchographe.Instance.NbVeinTouch,CanvasEchographe.Instance.NbArteryTouch);
            }
        }
    }

    public void RemoveAnesthesic(int amount)
    {
        if(!SuccessfulAnesthesia)
        {
            if (Needle.position.y > transform.position.y)
            {
                StateUp -= amount;

                if (StateUp < 0)
                {
                    StateUp = 0;
                }

                CanvasEchographe.Instance.UpdateUIUpAnesthesia(StateUp);
            }
            else
            {
                StateDown -= amount;
                if (StateDown < 0)
                {
                    StateDown = 0;
                }

                CanvasEchographe.Instance.UpdateUIDownAnesthesia(StateDown);
            }

            Vector3 temp = transform.localScale;
            var temp2 = Map(StateDown + StateUp, 0, 200, minScale, maxScale);
            temp.x = temp2;
            temp.y = temp2;

            AnesthesiaFeedback.transform.localScale = temp;
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

    private float Map(float value, float FromLow, float ToLow, float FromHigh, float ToHigh)
    {
        return (ToHigh - FromHigh) * ((value - FromLow) / (ToLow - FromLow)) + FromHigh;
    }
}
