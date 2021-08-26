using UnityEngine;
using System;

public class NeedleCollision : MonoBehaviour
{
    static public NeedleCollision Instance { get; private set; }

    [SerializeField]
    private Transform marker = null;

    public int NbInsertion = 0;

    private TimeSpan firstInsertion = TimeSpan.Zero;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Skin")
        {
            if(!AnestheticManager.Instance.SuccessfulAnesthesia)
            {
                if(firstInsertion == TimeSpan.Zero)
                {
                    firstInsertion = TimeSpan.FromSeconds(CanvasEchographe.Instance.elapsedTime);
                }
                NbInsertion++;
            }

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
