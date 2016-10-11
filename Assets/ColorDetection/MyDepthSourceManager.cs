using System;
using System.Drawing;
using System.Drawing.Imaging;
using UnityEngine;
using Windows.Kinect;
using AForge.Imaging.Filters;

public class MyDepthSourceManager : MonoBehaviour
{
    [Range(0, 100)] public float ScalingFactorZBuffer = 10.0f;
    [Range(1, 30)] public ushort NbFrameForBackgroundSuppression = 5;

    private KinectSensor _sensor;
    private DepthFrameReader _reader;
    private Texture2D _texture;
    private ushort[] _data;
    private ushort[,] _background;
    private ushort[] _backgroundMean;
    private int _framecounter;
    private uint _bufferSize;

    public byte[] GetData()
    {
        return MyConverter.ShortArray2ByteArray(_data);
    }

    public ushort GetRawZ(int i, int j)
    {
        return (ushort) (_backgroundMean[i + j*GetDescriptor().Width] -
                         _data[i + j*GetDescriptor().Width]/ScalingFactorZBuffer);
    }

    public Texture2D GetDepthTexture()
    {
        return _texture;
    }

    public FrameDescription GetDescriptor()
    {
        return _sensor.DepthFrameSource.FrameDescription;
    }

    void Start()
    {
        _framecounter = 0;
        _sensor = KinectSensor.GetDefault();

        if (_sensor != null)
        {
            _reader = _sensor.DepthFrameSource.OpenReader();
            var frameDesc = _sensor.DepthFrameSource.FrameDescription;
            _bufferSize = _sensor.DepthFrameSource.FrameDescription.LengthInPixels;

            _texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.RGB24, false);
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
                var desc = frame.FrameDescription;

                // Get current Frame Data
                frame.CopyFrameDataToArray(_data);

                // Fill buffer for background supression
                Buffer.BlockCopy(_data, 0, _background,
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
                    }

                    // compute mean of backgrounds to supress
                    for (int i = 0; i < _bufferSize; ++i)
                    {
                        _data[i] = (ushort) Math.Max((_backgroundMean[i] - _data[i])*ScalingFactorZBuffer, 0);
                    }
                }
                var zBuffer = AForge.Imaging.Image.Convert16bppTo8bpp(MyConverter.ByteArray2Bmp(MyConverter.ShortArray2ByteArray(_data), 
                    desc.Width,
                    desc.Height,
                    PixelFormat.Format16bppGrayScale));
                var grey2Rgb = new GrayscaleToRGB();
                zBuffer = grey2Rgb.Apply(zBuffer);

                _texture.LoadRawTextureData(MyConverter.Bmp2ByteArray(zBuffer));

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
}