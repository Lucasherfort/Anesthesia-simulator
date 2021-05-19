using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureDisplay : MonoBehaviour
{
    public Renderer textureRender;

    public void DrawNoiseMap(float[,] noise)
    {
        int width = noise.GetLength(0);
        int height = noise.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorNoise = new Color[width * height];
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                colorNoise[y * width + x] = Color.Lerp(Color.black, Color.white, noise[x, y]);
            }
        }

        texture.SetPixels(colorNoise);
        texture.Apply();

        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(width, 1, height);
    }
}
