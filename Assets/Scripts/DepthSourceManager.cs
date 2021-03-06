﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using UnityEngine;
using Windows.Kinect;
using AForge.Imaging.Filters;

public class DepthSourceManager : MonoBehaviour
{
    [Range(0, 100)] public float ScalingFactorZBuffer = 10.0f;
    [Range(1, 30)] public ushort NbFrameForBackgroundSuppression = 5;

    private KinectSensor _sensor;
    private DepthFrameReader _reader;
    private ushort[] _rawData;
    private ushort[] _data;
    private ushort[,] _background;
    private ushort[] _backgroundMean;
    private int _framecounter;
    private uint _bufferSize;

    void Start()
    {
        _framecounter = 0;
        _sensor = KinectSensor.GetDefault();

        if (_sensor != null)
        {
            _reader = _sensor.DepthFrameSource.OpenReader();
            _bufferSize = _sensor.DepthFrameSource.FrameDescription.LengthInPixels;

            _rawData = new ushort[_bufferSize];
            _data = new ushort[_bufferSize];

            _background = new ushort[NbFrameForBackgroundSuppression, _bufferSize];
            _backgroundMean = new ushort[_bufferSize];
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
                // Get current Frame Data
                frame.CopyFrameDataToArray(_rawData);

                // Fill buffer for background supression
                Buffer.BlockCopy(_rawData, 0, _background,
                    (int) _bufferSize*2*(_framecounter%NbFrameForBackgroundSuppression),
                    (int) _bufferSize*2);


                // After the first N frame (aka when the backgroud supression buffer is full)
                if (_framecounter >= NbFrameForBackgroundSuppression)
                {
                    for (int i = 0; i < _bufferSize; ++i)
                    {
                        _backgroundMean[i] = 0;

                        // Compute the background mean over the N saved frames
                        for (int j = 0; j < NbFrameForBackgroundSuppression; ++j)
                        {
                            _backgroundMean[i] += (ushort) (_background[j, i]/(float) NbFrameForBackgroundSuppression);
                        }

                        // compute mean of backgrounds to supress
                        var t = (_backgroundMean[i] - _rawData[i]) * ScalingFactorZBuffer;
                        _data[i] = (ushort) (t >= 0 ? t : 0);
                    }
                }

                frame.Dispose();
                frame = null;
                _framecounter++;
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
        return Converter.ShortArray2ByteArray(_data);
    }

    public ushort GetRawZ(int i, int j)
    {
        return _rawData[i + j*GetDescriptor().Width];
    }

    public ushort[] GetRawData()
    {
        return _rawData;
    }

    public FrameDescription GetDescriptor()
    {
        return _sensor.DepthFrameSource.FrameDescription;
    }
}