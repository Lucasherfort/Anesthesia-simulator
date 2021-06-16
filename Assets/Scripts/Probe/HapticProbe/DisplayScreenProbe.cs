using UnityEngine;

public class DisplayScreenProbe : MonoBehaviour
{
    [SerializeField]
    private string SkinTag = "Skin";
    [SerializeField]
    private Material composite = null;
    [SerializeField]
    private Material ScreenPlaceHolder = null;
    [SerializeField]
    private GameObject Screenrenderer = null;

    private void Start()
    {
        Screenrenderer.GetComponent<Renderer>().material = ScreenPlaceHolder;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("IN" + collision.collider.name);
        if (collision.collider.gameObject.tag == SkinTag)
        {
            Screenrenderer.GetComponent<Renderer>().material = composite;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("OUT" + collision.collider.name);
        if (collision.collider.gameObject.tag == SkinTag)
        {
            Screenrenderer.GetComponent<Renderer>().material = ScreenPlaceHolder;
        }
    }
}
