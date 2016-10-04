using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using AForge.Imaging;
using AForge.Imaging.Filters;
using Windows.Kinect;
using Color = UnityEngine.Color;
using AForge;

public class MyViewManager : MonoBehaviour
{
    public GameObject ColorManager;
    public GameObject DepthManager;

    [Range(0, 1)] public float ZBufferScale = 0.5f;
    [Range(0, 1)] public float ColorBlobScale = 0.15f;
    [Range(1, 15)] public int MinSizeBlob = 6;
    [Range(1, 30)] public int MaxSizeBlob = 20;
    [Range(0, 255)] public int ThresoldBlob = 30;
    [Range(1, 15)] public int ballRadius1 = 5;
    [Range(1, 15)] public int ballRadius2 = 10;
    [Range(0, 3)] public float ThresholdColor = 0.2f;
    [Range(0, 600)] public int ColorCheckSquareSize = 300;
    [Range(0, 600)] public int size = 300;
    [Range(0, 30)] public int FrameCount = 10;

    public bool AnalyseSquareForColor = false;
    public bool AnalyseDepth = false;
    public bool ShowDepth = false;
    public bool ShowBlobDetection = false;
    public bool ShowDetection = false;
    public bool ShowColorFilter = false;

    private Blob[] blobs;
    private bool isTracking = false;
    private int TrackingLostCpt;
    private List<Vector3> trajectory = new List<Vector3>(); 
    private MyColorSourceManager _colorManager;
    private MyDepthSourceManager _depthManager;
    private Texture2D _Texture;

    private enum Projectile
    {
        None    = -1,
        red     = 0,
        green   = 1,
        blue    = 2,
        yellow  = 3,
    }

    private UnityEngine.Color[] colors =
    {
        Color.red,      //r
        Color.green,    //g
        Color.blue,     //b
        Color.yellow    //y
    };


    public Texture2D GetTexture()
    {
        return _Texture;
    }

    void Start()
    {
        _depthManager = DepthManager.GetComponent<MyDepthSourceManager>();
        _colorManager = ColorManager.GetComponent<MyColorSourceManager>();
        _Texture = ShowDepth
            ? _depthManager.GetDepthTexture()
            : _colorManager.GetColorTexture();

        TrackingLostCpt = FrameCount;
    }

    void Update()
    {
        _Texture = ShowDepth
            ? _depthManager.GetDepthTexture()
            : _colorManager.GetColorTexture();

        if (AnalyseDepth)
        {
            var frameDescriptor = _depthManager.GetDescriptor();
            var zBuffer = ByteArray2Bmp(_depthManager.GetData(), frameDescriptor.Width, frameDescriptor.Height,
                PixelFormat.Format16bppGrayScale);
            var resizeFilter = new ResizeNearestNeighbor((int)(ZBufferScale * frameDescriptor.Width),
                (int)(ZBufferScale * frameDescriptor.Height));
            zBuffer = resizeFilter.Apply(zBuffer);
            zBuffer = AForge.Imaging.Image.Convert16bppTo8bpp(zBuffer);

            if (ShowBlobDetection)
            {

                Threshold treshFilter = new Threshold(ThresoldBlob);
                treshFilter.ApplyInPlace(zBuffer);

                var blobFilter = new BlobCounter();
                blobFilter.CoupledSizeFiltering = true;
                blobFilter.FilterBlobs = true;
                blobFilter.MinHeight = MinSizeBlob;
                blobFilter.MinWidth = MinSizeBlob;
                blobFilter.MaxHeight = MaxSizeBlob;
                blobFilter.MaxWidth = MaxSizeBlob;
                blobFilter.ProcessImage(zBuffer);
                blobs = blobFilter.GetObjectsInformation();
            }

            var convert = new GrayscaleToRGB();
            zBuffer = convert.Apply(zBuffer);

            byte[] arr = Bmp2ByteArray(zBuffer);
            _Texture = new Texture2D(zBuffer.Width, zBuffer.Height, TextureFormat.RGB24, false);
            _Texture.LoadRawTextureData(arr);

            if (ShowDetection)
            {
                DetectProjectile();
            }
        }
        else
        {
            if (AnalyseSquareForColor)
            {
                AnalyseSquare();
            }
        }

        _Texture.Apply();
    }


    private Projectile DetectProjectile()
    {
        /// Blob detection on color image to check if detected circles are balls
        var frameDescriptor = _colorManager.GetDescriptor();
        var colorImg = ByteArray2Bmp(_colorManager.GetData(),
            frameDescriptor.Width,
            frameDescriptor.Height,
            PixelFormat.Format32bppArgb);

        var resizeFilter = new ResizeNearestNeighbor((int)(ColorBlobScale * frameDescriptor.Width),
            (int)(ColorBlobScale * frameDescriptor.Height));

        colorImg = resizeFilter.Apply(colorImg);
        //colorImg.Save("C:\\Users\\dubm3114\\Desktop\\img.bmp");

        // Test with Color filter 
        //EuclideanColorFiltering filter = new EuclideanColorFiltering();
        //filter.CenterColor = new RGB(215, 30, 30);
        //filter.FillOutside = false;
        //filter.FillColor = new RGB(255, 0, 0);
        //filter.Radius = 100;
        //filter.ApplyInPlace(colorImg);
        //filter.CenterColor = new RGB(30, 30, 215);
        //filter.FillColor = new RGB(0, 0, 255);
        //filter.Radius = 100;
        //filter.ApplyInPlace(colorImg);
        //filter.CenterColor = new RGB(30, 180, 30);
        //filter.FillColor = new RGB(0, 255, 0);
        //filter.Radius = 100;
        //filter.ApplyInPlace(colorImg);
        //filter.CenterColor = new RGB(215, 215, 30);
        //filter.FillColor = new RGB(255, 255, 0);
        //filter.Radius = 100;
        //filter.ApplyInPlace(colorImg);

        var _ColorTexture = new Texture2D(colorImg.Width, colorImg.Height, TextureFormat.BGRA32, false);
        _ColorTexture.LoadRawTextureData(Bmp2ByteArray(colorImg));

        if (ShowColorFilter)
        {
            _Texture = _ColorTexture;
        }

        Blob TheBlob = getBiggestBlob();

        if (TheBlob != null)
        {
            if (!isTracking)
            {
                TrackingLostCpt = FrameCount;
                isTracking = true;
            }

            int z = _depthManager.GetRawZ((int)TheBlob.CenterOfGravity.X, (int)TheBlob.CenterOfGravity.Y);
            Vector3 currentPos = new Vector3(TheBlob.CenterOfGravity.X,TheBlob.CenterOfGravity.Y, z);
            trajectory.Add(currentPos);

            if (ShowDepth)
            {
                foreach (Vector3 position in trajectory)
                {
                    for (int x = -5; x < 5; x++)
                    {
                        for (int y = -5; y < 5; y++)
                        {
                            if (x > 0 && x < _Texture.width && y > 0 && y < _Texture.height)
                            {
                                _Texture.SetPixel((int)(position[0]) + x, (int)(position[1]) + y, Color.red);
                            }
                        }
                    }
                }
            }
            
            //Display a colored square were we detect a ball (the color of the square = color of the ball detected)

            if (ShowColorFilter)
            {
                
                var sensor = KinectSensor.GetDefault();
                DepthSpacePoint point = new DepthSpacePoint();
                point.X = (int)(TheBlob.CenterOfGravity.X / ZBufferScale);
                point.Y = (int)(TheBlob.CenterOfGravity.Y / ZBufferScale);

                var zDepth = _depthManager.GetRawZ((int)point.X, (int)point.Y);

                var colorSpacePoint = sensor.CoordinateMapper.MapDepthPointToColorSpace(point, zDepth);
                colorSpacePoint.X *= ColorBlobScale;
                colorSpacePoint.Y *= ColorBlobScale;

                if (!float.IsNegativeInfinity(colorSpacePoint.X) && !float.IsNegativeInfinity(colorSpacePoint.Y))
                {
                    var pixColor = ColorAverage((int)colorSpacePoint.X, (int)colorSpacePoint.Y, 1, _ColorTexture);

                    if (true)//ColorSquareDiff(pixColor, color) < ThresholdColor
                    {
                        for (int x = -1; x < 2; x++)
                        {
                            for (int y = -1; y < 2; y++)
                            {
                                if (x > 0 && x < _Texture.width && y > 0 && y < _Texture.height)
                                {
                                    _Texture.SetPixel((int)colorSpacePoint.X + x, (int)colorSpacePoint.Y + y, Color.red);
                                }
                            }
                        }
                    }
                }
            }
        }
        else if (isTracking)
        {
            TrackingLostCpt--;
            if(TrackingLostCpt == 0)
            {
                isTracking = false;
                trajectory.Clear();
            }
        }


        return Projectile.None;
    }
    
    private Projectile AnalyseSquare()
    {
        if (!ShowDepth)
        {
            // square drawing
            int xMin = 1920/2 - ColorCheckSquareSize/2;
            int xMax = 1920/2 + ColorCheckSquareSize/2;
            int yMin = 1080/2 - ColorCheckSquareSize/2;
            int yMax = 1080/2 + ColorCheckSquareSize/2;

            for (int i = 0; i < ColorCheckSquareSize; i++)
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

            // Detection
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
                        float dist = ColorSquareDiff(current, colors[c]);

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

            // set zone in top left corner
            //for (int x = 1; x < 50; x++)
            //{
            //    for (int y = 1; y < 50; y++)
            //    {
            //        _Texture.SetPixel(x, y, colors[maxIndex]);
            //    }
            //}

            return (Projectile) maxIndex;
        }
        return Projectile.None;
    }

    #region Helpers
    float ColorSquareDiff(Color col1, Color col2)
    {
        return Math.Abs(col1.r - col2.r) +
               Math.Abs(col1.g - col2.g) +
               Math.Abs(col1.b - col2.b);
    }

    Blob getBiggestBlob()
    {
        Blob biggestBlob = null;
        int maxArea = -1;

        foreach (Blob element in blobs)
        {
            if (element.Area > maxArea)
            {
                maxArea = element.Area;
                biggestBlob = element;
            }
        }
        return biggestBlob;
    }

    Color ColorAverage(int x, int y, int radius, Texture2D texture)
    {
        Color avColor = Color.black;
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                avColor += texture.GetPixel(x + i, y + j);
            }
        }
        avColor /= (radius*2 + 1) * (radius*2 + 1);
        return avColor;
    }
    #endregion

    #region Converters
    Bitmap ByteArray2Bmp(Byte[] arr, int width, int height, PixelFormat format)
    {
        Bitmap img = new Bitmap(width, height, format);

        BitmapData bitmapData = img.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.ReadWrite,
            img.PixelFormat);

        // Copy byte[] to bitmap
        Marshal.Copy(arr, 0, bitmapData.Scan0, arr.Length);
        img.UnlockBits(bitmapData);

        return img;
    }

    byte[] Bmp2ByteArray(System.Drawing.Bitmap img)
    {
        BitmapData bitmapData = img.LockBits(
            new Rectangle(0, 0, img.Width, img.Height),
            ImageLockMode.ReadOnly,
            img.PixelFormat);

        byte[] result = new byte[System.Math.Abs(bitmapData.Stride)*bitmapData.Height];

        // Copy Bitmap to byte[]
        Marshal.Copy(bitmapData.Scan0, result, 0, result.Length);
        img.UnlockBits(bitmapData);

        return result;
    }
    #endregion
}