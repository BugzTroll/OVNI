using System;
using UnityEngine;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Windows.Kinect;

public class MyColorSourceManager : MonoBehaviour
{
    private KinectSensor _Sensor;
    private ColorFrameReader _Reader;
    private Texture2D _Texture;
    private byte[] _Data;
    private byte[] _Background;
    private int cpt = 2;

    public byte[] GetData()
    {
        return _Data;
    }

    public Texture2D GetColorTexture()
    {
        return _Texture;
    }
    public int GetRawSize()
    {
        return _Sensor.ColorFrameSource.FrameDescription.Width * _Sensor.ColorFrameSource.FrameDescription.Height;
    }

    public FrameDescription GetDescriptor()
    {
        return _Sensor.ColorFrameSource.FrameDescription;
    }

    void Start()
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.ColorFrameSource.OpenReader();

            var frameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            _Texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.BGRA32, false);
            _Data = new byte[frameDesc.BytesPerPixel*frameDesc.LengthInPixels];
            _Background = new byte[frameDesc.BytesPerPixel * frameDesc.LengthInPixels];

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
                frame.CopyConvertedFrameDataToArray(_Data, ColorImageFormat.Bgra);

                //for (int i = 0; i < _Data.Length; ++i)
                //{
                //    _Data[i] = (byte)Math.Max((_Background[i] - _Data[i]), 0);
                //}
                _Texture.LoadRawTextureData(_Data);

            cpt--;
            if (cpt == 0)
            {
                frame.CopyConvertedFrameDataToArray(_Background, ColorImageFormat.Bgra);
            }

                frame.Dispose();
                frame = null;
            }
        }
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
}