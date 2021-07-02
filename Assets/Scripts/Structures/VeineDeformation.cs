using UnityEngine;

public class VeineDeformation : MonoBehaviour
{
    [SerializeField]
    private Transform[] objects = null;

    private float maxY = 0f;

    private float minScaleY = 0f;

    private float maxScaleY = 0f;

    [Range(0f, 1f)]
    public float force = 0f;

    private void Start()
    {
        if (objects.Length == 0)
        {
            Debug.LogWarning("The objects list is empty !");
        }
        else
        {
            maxY = objects[0].position.y;
            maxScaleY = objects[0].localScale.y;
        }
    }

    private void Update()
    {
        if(force == 1)
        {
            foreach (Transform obj in objects)
            {
                obj.gameObject.SetActive(false);
            }
        }
        else
        {
            foreach (Transform obj in objects)
            {
                obj.gameObject.SetActive(true);
            }
        }

        ApplyDeformation(force);
    }

    private void ApplyDeformation(float force)
    {
        float localY = Map(force, 0f, 1f, maxY, 0);
        float ScalelocalY = ((maxScaleY - minScaleY) / maxY) * localY;

        foreach (Transform obj in objects)
        {
            Vector3 temp = obj.localScale;
            temp.y = ScalelocalY;
            obj.localScale = temp;

            Vector3 temp2 = obj.localPosition;
            temp2.y = localY;
            obj.localPosition = temp2;
        }
    }

    private float Map(float variable, float x1, float x2, float y1, float y2)
    {
        float a = (y1 - y2) / (x1 - x2);
        float b = y1 - x1 * ((y1 - y2) / (x1 - x2));

        return a * variable + b;
    }
}
