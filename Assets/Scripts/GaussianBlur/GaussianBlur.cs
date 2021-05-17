using UnityEngine;
using System.Collections;

public class GaussianBlur : MonoBehaviour
{
    [SerializeField]
    [Range(2, 512)]
    private int resolution = 256;

    [SerializeField]
    private bool mipmap = false;

    [SerializeField]
    private FilterMode filterMode;

    [SerializeField]
    [Range(1, 16)]
    private int anisoLevel = 9;

    private float avgR = 0;
    private float avgG = 0;
    private float avgB = 0;
    private float avgA = 0;
    private float blurPixelCount = 0;

    public int radius = 2;
    public int iterations = 2;

    private Texture2D tex;

    // Use this for initialization
    void OnEnable()
    {
        if (tex == null)
        {
            tex = new Texture2D(resolution, resolution, TextureFormat.RGB24, mipmap);
            tex.name = "Procedural Texture";
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = filterMode;
            tex.anisoLevel = anisoLevel;
            GetComponent<MeshRenderer>().material.mainTexture = tex;
        }

        FastBlur(tex, radius, iterations);
    }

    Texture2D FastBlur(Texture2D image, int radius, int iterations)
    {
        Texture2D tex = image;

        for (var i = 0; i < iterations; i++)
        {

            tex = BlurImage(tex, radius, true);
            tex = BlurImage(tex, radius, false);

        }

        return tex;
    }



    Texture2D BlurImage(Texture2D image, int blurSize, bool horizontal)
    {

        Texture2D blurred = new Texture2D(image.width, image.height);
        int _W = image.width;
        int _H = image.height;
        int xx, yy, x, y;

        if (horizontal)
        {
            for (yy = 0; yy < _H; yy++)
            {
                for (xx = 0; xx < _W; xx++)
                {
                    ResetPixel();

                    //Right side of pixel

                    for (x = xx; (x < xx + blurSize && x < _W); x++)
                    {
                        AddPixel(image.GetPixel(x, yy));
                    }

                    //Left side of pixel

                    for (x = xx; (x > xx - blurSize && x > 0); x--)
                    {
                        AddPixel(image.GetPixel(x, yy));

                    }


                    CalcPixel();

                    for (x = xx; x < xx + blurSize && x < _W; x++)
                    {
                        blurred.SetPixel(x, yy, new Color(avgR, avgG, avgB, 1.0f));

                    }
                }
            }
        }

        else
        {
            for (xx = 0; xx < _W; xx++)
            {
                for (yy = 0; yy < _H; yy++)
                {
                    ResetPixel();

                    //Over pixel

                    for (y = yy; (y < yy + blurSize && y < _H); y++)
                    {
                        AddPixel(image.GetPixel(xx, y));
                    }
                    //Under pixel

                    for (y = yy; (y > yy - blurSize && y > 0); y--)
                    {
                        AddPixel(image.GetPixel(xx, y));
                    }
                    CalcPixel();
                    for (y = yy; y < yy + blurSize && y < _H; y++)
                    {
                        blurred.SetPixel(xx, y, new Color(avgR, avgG, avgB, 1.0f));

                    }
                }
            }
        }

        blurred.Apply();
        return blurred;
    }
    void AddPixel(Color pixel)
    {
        avgR += pixel.r;
        avgG += pixel.g;
        avgB += pixel.b;
        blurPixelCount++;
    }

    void ResetPixel()
    {
        avgR = 0.0f;
        avgG = 0.0f;
        avgB = 0.0f;
        blurPixelCount = 0;
    }

    void CalcPixel()
    {
        avgR = avgR / blurPixelCount;
        avgG = avgG / blurPixelCount;
        avgB = avgB / blurPixelCount;
    }
}
