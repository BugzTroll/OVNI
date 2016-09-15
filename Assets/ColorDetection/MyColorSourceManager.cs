using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class MyColorSourceManager : MonoBehaviour
{
    public int ColorWidth { get; private set; }
    public int ColorHeight { get; private set; }

    public bool DetectionActivated = true;

    private KinectSensor _Sensor;
    private ColorFrameReader _Reader;
    private Texture2D _Texture;
    private byte[] _Data;

    private int xMin = 890;
    private int yMin = 470;
    private int xMax = 1030;
    private int yMax = 610;

    public enum Projectile
    {
        red,
        blue,
        yellow,
        green,
        orange
    }

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


            for (int i = 0; i < 140; i++)
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
        var _width = _Texture.width;
        var _height = _Texture.height;

        return Projectile.blue;
    }
}