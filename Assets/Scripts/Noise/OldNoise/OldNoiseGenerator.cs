using UnityEngine;

/*
 * https://catlikecoding.com/unity/tutorials/noise/
 * https://forum.unity.com/threads/contribution-texture2d-blur-in-c.185694/
 */

public enum Orienttion
{
    Horizontal,
    Vertical
}

public class OldNoiseGenerator : MonoBehaviour
{
    [Header("General")]
    [SerializeField]
    [Range(2, 512)]
    private int resolution = 256;

    [SerializeField]
    private bool mipmap = false;

    [SerializeField]
    private FilterMode filterMode = FilterMode.Trilinear;

    [Header("Noise parameters")]

    [SerializeField]
    [Range(1, 16)]
    private int anisoLevel = 9;

    [SerializeField]
    private float frequency = 1f;

    [Range(1, 3)]
    public int dimensions = 3;

    public NoiseMethodType type;

    [Range(1, 8)]
    public int octaves = 1;

    [Range(1f, 4f)]
    public float lacunarity = 2f;

    [Range(0f, 1f)]
    public float persistence = 0.5f;

    [SerializeField]
    private Gradient coloring = null;

    private float avgR = 0;
    private float avgG = 0;
    private float avgB = 0;
    //private float avgA = 0;
    private float blurPixelCount = 0;

    [Header("Gaussian blur parameters")]

    public int radius = 2;
    public int iterations = 2;
    public Orienttion orientation;
    private Texture2D texture;

    private void OnEnable()
    {
        if(texture == null)
        {
            texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, mipmap);
            texture.name = "Procedural Texture";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = filterMode;
            texture.anisoLevel = anisoLevel;
            GetComponent<MeshRenderer>().material.mainTexture = texture;
        }
        FillTexture();
        BlurImage();
    }

    private void Update()
    {
        if (transform.hasChanged)
        {
            transform.hasChanged = false;
            FillTexture();
            BlurImage();
        }
    }

    public void FillTexture()
    {
        if (texture.width != resolution)
        {
            texture.Resize(resolution, resolution);
        }

        Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f, -0.5f));
        Vector3 point10 = transform.TransformPoint(new Vector3(0.5f, -0.5f));
        Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f));
        Vector3 point11 = transform.TransformPoint(new Vector3(0.5f, 0.5f));

        NoiseMethod method = Noise.noiseMethods[(int)type][dimensions - 1];
        float stepSize = 1f / resolution;

        for (int y = 0; y < resolution; y++)
        {
            Vector3 point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
            Vector3 point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);

            for (int x = 0; x < resolution; x++)
            {
                Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);

                float sample = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
                if (type != NoiseMethodType.Value)
                {
                    sample = sample * 0.5f + 0.5f;
                }

                texture.SetPixel(x, y, coloring.Evaluate(sample));
            }
        }
        texture.Apply();
    }

    public void BlurImage()
    {
        Texture2D blurred = new Texture2D(texture.width, texture.height);
        int _W = texture.width;
        int _H = texture.height;

        switch(orientation)
        {
            case Orienttion.Horizontal:
                blurred = BlurImageHorizontal(blurred,_H,_W);
            break;

            case Orienttion.Vertical:
                blurred = BlurImageVertical(blurred, _H, _W);
            break;
        }

       

        blurred.Apply();
        GetComponent<MeshRenderer>().material.mainTexture = blurred;
    }

    private Texture2D BlurImageHorizontal(Texture2D blurred, int _H, int _W)
    {
        int xx, yy, x;

        for (yy = 0; yy < _H; yy++)
        {
            for (xx = 0; xx < _W; xx++)
            {
                ResetPixel();

                //Right side of pixel

                for (x = xx; (x < xx + radius && x < _W); x++)
                {
                    AddPixel(texture.GetPixel(x, yy));
                }

                //Left side of pixel

                for (x = xx; (x > xx - radius && x > 0); x--)
                {
                    AddPixel(texture.GetPixel(x, yy));

                }


                CalcPixel();

                for (x = xx; x < xx + radius && x < _W; x++)
                {
                    blurred.SetPixel(x, yy, new Color(avgR, avgG, avgB, 1.0f));

                }
            }
        }

        return blurred;
    }

    private Texture2D BlurImageVertical(Texture2D blurred, int _H, int _W)
    {
        int xx, yy, y;
        for (xx = 0; xx < _W; xx++)
        {
            for (yy = 0; yy < _H; yy++)
            {
                ResetPixel();

                //Over pixel

                for (y = yy; (y < yy + radius && y < _H); y++)
                {
                    AddPixel(texture.GetPixel(xx, y));
                }
                //Under pixel

                for (y = yy; (y > yy - radius && y > 0); y--)
                {
                    AddPixel(texture.GetPixel(xx, y));
                }
                CalcPixel();
                for (y = yy; y < yy + radius && y < _H; y++)
                {
                    blurred.SetPixel(xx, y, new Color(avgR, avgG, avgB, 1.0f));
                }
            }
        }
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
