using System.Net.Sockets;
using UnityEngine;
using Windows.Kinect;

public class ColorSourceManager : MonoBehaviour
{
    private KinectSensor _sensor;
    private ColorFrameReader _reader;
    private Texture2D _texture;
    private byte[] _data;

    public void Shutdown()
    {
        if (_reader != null)
        {
            _reader.Dispose();
            _reader = null;
        }
        if (!DebugManager.Debug)
        {
            this.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        _sensor = KinectSensor.GetDefault();

        if (_sensor != null)
        {
            _reader = _sensor.ColorFrameSource.OpenReader();

            var frameDesc = _sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            _texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.BGRA32, false);
            _data = new byte[frameDesc.BytesPerPixel*frameDesc.LengthInPixels];

            if (!_sensor.IsOpen)
            {
                _sensor.Open();
            }
        }
    }

    void Update()
    {
        if (_reader != null)
        {
            var frame = _reader.AcquireLatestFrame();

            if (frame != null)
            {
                frame.CopyConvertedFrameDataToArray(_data, ColorImageFormat.Bgra);
                _texture.LoadRawTextureData(_data);
                frame.Dispose();
                frame = null;
            }
        }
    }

    void OnApplicationQuit()
    {
        if (_reader != null)
        {
            _reader.Dispose();
            _reader = null;
        }

        if (_sensor != null)
        {
            if (_sensor.IsOpen)
            {
                _sensor.Close();
            }

            _sensor = null;
        }
    }

    public byte[] GetData()
    {
        return _data;
    }

    public Texture2D GetColorTexture()
    {
        return _texture;
    }

    public FrameDescription GetDescriptor()
    {
        return _sensor.ColorFrameSource.FrameDescription;
    }
}