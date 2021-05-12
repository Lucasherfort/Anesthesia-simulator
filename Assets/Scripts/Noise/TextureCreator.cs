using UnityEngine;

public class TextureCreator : MonoBehaviour
{
    [SerializeField]
    [Range(2, 512)]
    private int resolution = 256;

    private Texture2D texture;

    private void OnEnable()
    {
        texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, true);
        texture.name = "Procedural Texture";
        texture.wrapMode = TextureWrapMode.Clamp;
        //texture.filterMode = FilterMode.Point;
        texture.filterMode = FilterMode.Bilinear;
        GetComponent<MeshRenderer>().material.mainTexture = texture;
        FillTexture();
    }

    public void FillTexture()
    {
        if (texture.width != resolution)
        {
            texture.Resize(resolution, resolution);
        }

        float stepSize = 1f / resolution;
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                //texture.SetPixel(x, y, new Color((x + 0.5f) * stepSize, (y + 0.5f) * stepSize, 0f));
                texture.SetPixel(x, y, new Color((x + 0.5f) * stepSize % 0.1f, (y + 0.5f) * stepSize % 0.1f, 0f) * 10f);
            }
        }
        texture.Apply();
    }
}
