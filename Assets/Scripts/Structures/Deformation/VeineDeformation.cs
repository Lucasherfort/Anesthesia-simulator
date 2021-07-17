using UnityEngine;

public class VeineDeformation : MonoBehaviour
{
    [SerializeField]
    private Transform veine = null;

    private float NormalPositionY;
    private float NormalScaleY;

    public float NewPositionY = -5f;
    public float NewScaleY; 

    [Range(0f,1f)]
    public float force;

    private void Start()
    {
        if(veine != null)
        {
            NormalPositionY = veine.localPosition.y;
            NormalScaleY = veine.localScale.y;
        }
    }

    private void Update()
    {
        ApplyDeformation(force);
    }

    private void ApplyDeformation(float force)
    {
        float ResizePosY = Map(force, 0,1,NormalPositionY,NewPositionY);
        float ResizeScaleY = Map(force, 0,1,NormalScaleY,NewScaleY);

        var tempPos = veine.localPosition;
        tempPos.y = ResizePosY;
        veine.localPosition = tempPos;

        var tempScale = veine.localScale;
        tempScale.y = ResizeScaleY;
        veine.localScale = tempScale;
    }

    private float Map(float value, float FromLow, float ToLow, float FromHigh, float ToHigh)
    {
        return (ToHigh - FromHigh) * ((value - FromLow) / (ToLow - FromLow)) + FromHigh;
    }
}
