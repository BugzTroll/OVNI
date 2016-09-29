﻿using System;
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
    private KinectSensor _Sensor;
    private DepthFrameReader _Reader;
    private Texture2D _Texture;
    private ushort[] _Data;
    private ushort[] _Background;
    private int cpt = 2;

    public byte[] GetData()
    {
        return ShortArray2ByteArray(_Data);
    }
    public ushort[] GetRawData()
    {
        return _Data;
    }
    public int GetRawSize()
    {
        return _Data.Length;
    }

    public ushort GetRawZ(int i, int j)
    {
        return (ushort) (_Background[(int) (i + j*GetDescriptor().Width)] -
                         _Data[(int) (i + j*GetDescriptor().Width)]);
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
            _Texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.R16, false);
            _Data = new ushort[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];
            _Background = new ushort[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];

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

                for (int i = 0; i < _Data.Length; ++i)
                {
                    _Data[i] = (ushort)Math.Max((_Background[i] - _Data[i]), 0);
                }

                _Texture.LoadRawTextureData(ShortArray2ByteArray(_Data));

                cpt--;
                if (cpt == 0)
                {
                    frame.CopyFrameDataToArray(_Background);
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

    Bitmap ByteArray2Bmp(Byte[] arr, int width, int height, PixelFormat format)
    {
        Bitmap img = new Bitmap(width, height, format);

        // Bmp->ByteArray
        BitmapData bitmapData = img.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.ReadOnly,
            img.PixelFormat);

        // Copy bitmap to byte[]
        Marshal.Copy(bitmapData.Scan0, arr, 0, arr.Length);
        img.UnlockBits(bitmapData);

        return img;
    }

    byte[] Bmp2ByteArray(System.Drawing.Bitmap bmp)
    {
        BitmapData bmpdata = bmp.LockBits(
            new Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.WriteOnly,
            bmp.PixelFormat);

        byte[] result = new byte[System.Math.Abs(bmpdata.Stride) * bmpdata.Height];

        IntPtr ptr = bmpdata.Scan0;
        Marshal.Copy(result, 0, ptr, result.Length);
        bmp.UnlockBits(bmpdata);

        return result;
    }
}