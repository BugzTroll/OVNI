using System;
using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class MyDepthSourceManager : MonoBehaviour
{
    private KinectSensor _Sensor;
    private DepthFrameReader _Reader;
    private Texture2D _Texture;
    private ushort[] _Data;

    public byte[] GetData()
    {
        return ShortArray2ByteArray(_Data);
    }

    public Texture2D GetDepthTexture()
    {
        return _Texture;
    }


    void Start()
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.DepthFrameSource.OpenReader();

            var frameDesc = _Sensor.DepthFrameSource.FrameDescription;
            _Texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.R16, false);
            _Data = new ushort[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];

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
                frame.CopyFrameDataToArray(_Data);
                _Texture.LoadRawTextureData(ShortArray2ByteArray(_Data));
                var yeah = _Texture.GetPixel(10, 10);
                frame.Dispose();
                frame = null;
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

    byte[] ShortArray2ByteArray(ushort[] shortArray)
    {
        byte[] result = new byte[shortArray.Length * sizeof(short)];
        Buffer.BlockCopy(shortArray, 0, result, 0, result.Length);
        return result;
    }
}