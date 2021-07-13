using UnityEngine;

public class VeineDeformation : MonoBehaviour
{
    [SerializeField]
    private Transform veine = null;

    private float maxY = 0f;

    private float minScaleY = 0f;

    private float maxScaleY = 0f;

    [Range(0f, 1f)]
    public float force = 0f;

    private void Start()
    {
        if (veine == null)
        {
            Debug.LogWarning("veine variable is empty !");
        }
        else
        {
            maxY = veine.position.y;
            maxScaleY = veine.localScale.y;
        }
    }

    private void Update()
    {
        if (force == 1)
        {

                veine.gameObject.SetActive(false);

        }
        else
        {
            veine.gameObject.SetActive(true);
        }

        ApplyDeformation(force);
    }

    private void ApplyDeformation(float force)
    {
        float localY = Map(force, 0f, 1f, maxY, 0);
        float ScalelocalY = ((maxScaleY - minScaleY) / maxY) * localY;


        Vector3 temp = veine.localScale;
        temp.y = ScalelocalY;
        veine.localScale = temp;

        Vector3 temp2 = veine.localPosition;
        temp2.y = localY;
        veine.localPosition = temp2;     
    }

    private float Map(float variable, float x1, float x2, float y1, float y2)
    {
        float a = (y1 - y2) / (x1 - x2);
        float b = y1 - x1 * ((y1 - y2) / (x1 - x2));

        return a * variable + b;
    }
}
