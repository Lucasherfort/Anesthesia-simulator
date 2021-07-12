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

    void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Needle")
        {
            switch(CurrentStructure)
            {
                case Structure.None:
                    break;

                case Structure.Artery:
                    // TODO
                    break;

                case Structure.Veine:
                    // TODO
                    break;

                case Structure.Nerve:  
                    // TODO               
                    break;
            }

            Debug.Log(CurrentStructure + " a été touché");
        }
    }
}
