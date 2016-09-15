using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System.Collections.Generic;
using System.Linq;

public class MyColorSourceManager : MonoBehaviour
{
    public int ColorWidth { get; private set; }
    public int ColorHeight { get; private set; }

    public bool DetectionActivated = true;

    private KinectSensor _Sensor;
    private ColorFrameReader _Reader;
    private Texture2D _Texture;
    private byte[] _Data;

   

    static public int squareSize = 300;

    private int xMin = 1920/2 - squareSize/2;
    private int xMax = 1920/2 + squareSize/2;
    private int yMin = 1080/2 - squareSize/2;
    private int yMax = 1080/2 + squareSize/2;

    public enum Projectile
    {
        red = 0,
        blue = 1,
        green = 2,
        yellow = 3,
    }

    private Color[] colors =
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow
    };

    public Texture2D GetColorTexture()
    {
        return _Texture;
    }

    void Start()
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.ColorFrameSource.OpenReader();

            var frameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            ColorWidth = frameDesc.Width;
            ColorHeight = frameDesc.Height;

            _Texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.RGBA32, false);
            _Data = new byte[frameDesc.BytesPerPixel*frameDesc.LengthInPixels];

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }

    void Update()
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();

            if (frame != null)
            {
                frame.CopyConvertedFrameDataToArray(_Data, ColorImageFormat.Rgba);
                _Texture.LoadRawTextureData(_Data);

                frame.Dispose();
                frame = null;
            }
        }

        if (DetectionActivated)
        {
            for (int i = 0; i < squareSize; i++)
            {
                _Texture.SetPixel(xMin + i, yMin, Color.red);
                _Texture.SetPixel(xMin + i, yMin - 1, Color.red);

                _Texture.SetPixel(xMin + i, yMax, Color.red);
                _Texture.SetPixel(xMin + i, yMax + 1, Color.red);

                _Texture.SetPixel(xMin, yMin + i, Color.red);
                _Texture.SetPixel(xMin - 1, yMin + i, Color.red);

                _Texture.SetPixel(xMax, yMin + i, Color.red);
                _Texture.SetPixel(xMax + 1, yMin + i, Color.red);
            }

            DetectProjectile();
        }

        _Texture.Apply();
    }

    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }

    public Projectile DetectProjectile()
    {
        int[] colorMax = { 0, 0, 0, 0 };

        for (int x = xMin + 1; x < xMax - 1; x++)
        {
            for (int y = yMin + 1; y < yMax - 1; y++)
            {
                Color current = _Texture.GetPixel(x, y);
                short min = -1;
                float distMin = float.MaxValue;

                for (short c = 0; c < colors.Length; c++)
                {
                    float dist = (current.r - colors[c].r)*(current.r - colors[c].r) +
                                 (current.g - colors[c].g)*(current.g - colors[c].g) +
                                 (current.b - colors[c].b)*(current.b - colors[c].b);

                    if (dist < distMin)
                    {
                        min = c;
                        distMin = dist;
                    }
                }

                _Texture.SetPixel(x, y, colors[min]);
                colorMax[min] += 1;

            }
        }

        int maxValue = colorMax.Max();
        int maxIndex = colorMax.ToList().IndexOf(maxValue);
        setZone(colors[maxIndex]);

        return (Projectile)maxIndex;
    }

    public void setZone(Color color)
    {
        for (int x = 1; x < 50; x++)
        {
            for (int y = 1; y < 50; y++)
            {
                _Texture.SetPixel(x, y, color);
            }
        }
    }
}