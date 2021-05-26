using UnityEngine;

public class HapticProbe : MonoBehaviour
{
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
        if(collision.collider.gameObject.tag == "Skin")
        {
            Screenrenderer.GetComponent<Renderer>().material = composite;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Skin")
        {
            Screenrenderer.GetComponent<Renderer>().material = ScreenPlaceHolder;
        }
    }
}
