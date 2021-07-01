using UnityEngine;

public enum Structure
{
    Artery,
    Veine,
    Nerve
}

public class StructureManager : MonoBehaviour
{
    [SerializeField]
    private Structure CurrentStructure;

    void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Needle")
        {
            Debug.Log(CurrentStructure+" a été touché");
        }
    }
}
