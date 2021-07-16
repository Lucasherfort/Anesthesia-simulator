using UnityEngine;

public enum Structure
{
    None,
    Artery,
    Veine,
    Nerve
}

public class StructureManager : MonoBehaviour
{
    public Structure CurrentStructure;

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Needle")
        {
            switch(CurrentStructure)
            {
                case Structure.None:
                    break;

                case Structure.Artery:
                    CanvasEchographe.Instance.UpdateTouchArtery();
                    break;

                case Structure.Veine:
                    CanvasEchographe.Instance.UpdateTouchVein();
                    break;

                case Structure.Nerve:
                    CanvasEchographe.Instance.UpdateTouchNerve();               
                    break;
            }
        }
    }
}
