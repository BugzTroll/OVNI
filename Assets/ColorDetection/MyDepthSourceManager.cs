using System;
using UnityEngine;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Kinect;
using AForge.Imaging;
using AForge.Imaging.Filters;

public class MyDepthSourceManager : MonoBehaviour
{
    public float ScalingFactorZBuffer = 10.0f;
    public ushort SizeOfBackgroundSuppression = 5;

    private KinectSensor _Sensor;
    private DepthFrameReader _Reader;
    private Texture2D _Texture;

    private ushort[] _Data;
    private ushort[,] _Background;
    private ushort[] _BackgroundMean;
    private int FrameCounter = 0;
    private uint BufferSize;

    public byte[] GetData()
    {
        return ShortArray2ByteArray(_Data);
    }

    public ushort GetRawZ(int i, int j)
    {
        return (ushort)((_BackgroundMean[i + j * GetDescriptor().Width] -
                         _Data[i + j * GetDescriptor().Width]) / ScalingFactorZBuffer);
    }

    public Texture2D GetDepthTexture()
    {
        return _Texture;
    }

    public FrameDescription GetDescriptor()
    {
        return _Sensor.DepthFrameSource.FrameDescription;
    }

    void Start()
    {
        _Sensor = KinectSensor.GetDefault();
        
        if (_Sensor != null)
        {
            _Reader = _Sensor.DepthFrameSource.OpenReader();
            var frameDesc = _Sensor.DepthFrameSource.FrameDescription;
            BufferSize = _Sensor.DepthFrameSource.FrameDescription.LengthInPixels;

            _Texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.R16, false);
            _Data = new ushort[BufferSize];
            _Background = new ushort[SizeOfBackgroundSuppression, BufferSize];
            _BackgroundMean = new ushort[BufferSize];
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
                // Get current Frame Data
                frame.CopyFrameDataToArray(_Data);

                var a = (int)BufferSize * (FrameCounter % SizeOfBackgroundSuppression);

                // Fill buffer for background supression
                Buffer.BlockCopy(_Data, 0, _Background,
                    (int)BufferSize * 2 * (FrameCounter % SizeOfBackgroundSuppression),
                    (int)BufferSize * 2);

                // After the first N frame (aka when the backgroud supression buffer is full)
                if (FrameCounter >= SizeOfBackgroundSuppression)
                {
                    for (int i = 0; i < BufferSize; ++i)
                    {
                        _BackgroundMean[i] = 0;
                        for (int j = 0; j < SizeOfBackgroundSuppression; ++j)
                        {
                            
                            _BackgroundMean[i] += (ushort)(_Background[j, i] / (float)SizeOfBackgroundSuppression);
                        }
                    }

                    // compute mean of backgrounds to supress
                    for (int i = 0; i < BufferSize; ++i)
                    {
                        _Data[i] = (ushort)Math.Max((_BackgroundMean[i] - _Data[i]) * ScalingFactorZBuffer, 0);
                    }
                }

                _Texture.LoadRawTextureData(ShortArray2ByteArray(_Data));

                frame.Dispose();
                frame = null;
                FrameCounter++;
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

    #region Converter
    byte[] ShortArray2ByteArray(ushort[] shortArray)
    {
        byte[] result = new byte[shortArray.Length * sizeof(short)];
        Buffer.BlockCopy(shortArray, 0, result, 0, result.Length);
        return result;
    }

    ushort[] ByteArray2ShortArray(byte[] bytes)
    {
        ushort[] result = new ushort[bytes.Length / sizeof(ushort)];
        Buffer.BlockCopy(bytes, 0, result, 0, result.Length);
        return result;
    }
    #endregion
}