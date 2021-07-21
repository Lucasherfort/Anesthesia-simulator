using UnityEngine;

public class AnestheticManager : MonoBehaviour
{
    static public AnestheticManager Instance{get; private set;}

    private bool NeedleInsideArea = false;

    public bool NeedleIsUp = false;
    public bool NeedleIsDown = false;

    public int StateUp = 0;
    public int StateDown = 0;

    private bool SuccessfulAnesthesia;

    [SerializeField]
    private Transform Needle = null;

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

            if(StateUp + StateDown == 200)
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
