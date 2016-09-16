using System;
using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using AForge.Imaging.Filters;
using AForge.Math;
using Color = UnityEngine.Color;

public class MyColorSourceManager : MonoBehaviour
{
    public int ColorWidth { get; private set; }
    public int ColorHeight { get; private set; }

    public bool DetectionActivated = true;
    public int SquareSize = 300;

    public bool BlurActivated = false;

    private KinectSensor _Sensor;
    private ColorFrameReader _Reader;
    private Texture2D _Texture;
    private byte[] _Data;

    public enum Projectile
    {
        red = 0,
        blue = 1,
        green = 2,
        yellow = 3,
    }

    private UnityEngine.Color[] colors =
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow
    };

    public Texture2D GetColorTexture()
    {
        if (BlurActivated)
        {
            // Frame Descriptor 
            var frameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            ColorWidth = frameDesc.Width;
            ColorHeight = frameDesc.Height;

            // ByteArray -> Bmp
            Bitmap img = ByteArray2Bmp(_Data, ColorWidth, ColorHeight);

            // Convolution
            GaussianBlur filter = new GaussianBlur(42,6);
            filter.ApplyInPlace(img);

            // Bmp->ByteArray
            BitmapData bitmapData = img.LockBits(
                new Rectangle(0, 0, ColorWidth, ColorHeight), 
                ImageLockMode.ReadOnly,
                img.PixelFormat);


            // Copy bitmap to byte[]
            Marshal.Copy(bitmapData.Scan0, _Data, 0, _Data.Length);
            img.UnlockBits(bitmapData);

            _Texture.LoadRawTextureData(_Data);
            _Texture.Apply();
        }

        return _Texture;
    }

    System.Drawing.Bitmap ByteArray2Bmp(Byte[] arr, int width, int height)
    {
        Bitmap img = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        // Bmp->ByteArray
        BitmapData bitmapData = img.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.ReadOnly,
            img.PixelFormat);


        // Copy bitmap to byte[]
        Marshal.Copy(bitmapData.Scan0, arr, 0, arr.Length);
        img.UnlockBits(bitmapData);

        _Texture.LoadRawTextureData(arr);
        _Texture.Apply();

        return img;
    }

    //Byte[] Bmp2ByteArray(System.Drawing.Bitmap bmp)
    //{
    //    // Frame Descriptor 
    //    var frameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
    //    ColorWidth = frameDesc.Width;
    //    ColorHeight = frameDesc.Height;

    //    // ByteArray -> Bmp
    //    Bitmap img = new Bitmap(ColorWidth, ColorHeight, PixelFormat.Format32bppArgb);

    //    BitmapData bmpdata = img.LockBits(
    //        new Rectangle(0, 0, ColorWidth, ColorHeight),
    //        ImageLockMode.WriteOnly,
    //        img.PixelFormat);

    //    IntPtr ptr = bmpdata.Scan0;
    //    Marshal.Copy(_Data, 0, ptr, _Data.Length);
    //    img.UnlockBits(bmpdata);

    //    return img;
    //}

    void Start()
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.ColorFrameSource.OpenReader();

            var frameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            ColorWidth = frameDesc.Width;
            ColorHeight = frameDesc.Height;

            _Texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.BGRA32, false);
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
                frame.CopyConvertedFrameDataToArray(_Data, ColorImageFormat.Bgra);
                _Texture.LoadRawTextureData(_Data);

                frame.Dispose();
                frame = null;
            }
        }

        if (DetectionActivated)
        {
            int xMin = 1920/2 - SquareSize/2;
            int xMax = 1920/2 + SquareSize/2;
            int yMin = 1080/2 - SquareSize/2;
            int yMax = 1080/2 + SquareSize/2;

            for (int i = 0; i < SquareSize; i++)
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
        int xMin = 1920/2 - SquareSize/2;
        int xMax = 1920/2 + SquareSize/2;
        int yMin = 1080/2 - SquareSize/2;
        int yMax = 1080/2 + SquareSize/2;

        int[] colorMax = {0, 0, 0, 0};

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

        return (Projectile) maxIndex;
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