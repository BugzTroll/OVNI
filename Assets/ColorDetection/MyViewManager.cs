using System;
using UnityEngine;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using AForge.Imaging;
using AForge.Imaging.Filters;
using Windows.Kinect;
using Color = UnityEngine.Color;
using WrapMode = UnityEngine.WrapMode;

public class MyViewManager : MonoBehaviour
{
    public GameObject ColorManager;
    public GameObject DepthManager;

    public bool ShowDepth = false;
    public float ZBufferScale = 0.5f;

    public bool ZBufferEdgeDetection = false;

    public bool ZBufferHoughDetection = false;
    public int ballRadius = 15;
    public float circleRelativeIntensity = 0.95f;

    public bool Detection = false;
    [Range(0,3)]
    public float ThresholdColor = 0.2f;


    private HoughCircle[] circles;
    //[Range(0, 600)] public int SquareSize = 300;
    //public bool TakePicture = false;
    //[Range(0, 1)] public float PictureScaleFactor = 0.5f;

    private MyColorSourceManager _colorManager;
    private MyDepthSourceManager _depthManager;
    private Texture2D _Texture;

    private enum Projectile
    {
        None = -1,
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

    void Start()
    {
        _depthManager = DepthManager.GetComponent<MyDepthSourceManager>();
        _colorManager = ColorManager.GetComponent<MyColorSourceManager>();
        _Texture = ShowDepth
            ? _depthManager.GetDepthTexture()
            : _colorManager.GetColorTexture();
    }

    void Update()
    {
        _Texture = ShowDepth
            ? _depthManager.GetDepthTexture()
            : _colorManager.GetColorTexture();

        //if (TakePicture)
        //{
        //    var frameDexcriptor = _colorManager.GetDescriptor();
        //    var bmp = ByteArray2Bmp(_colorManager.GetData(), frameDexcriptor.Width, frameDexcriptor.Height,
        //        PixelFormat.Format32bppArgb);
        //    bmp.Save("C:\\Users\\nadm2208\\Desktop\\Picture.bmp");
        //    TakePicture = false;
        //}

        circles = new HoughCircle[0];

        if (ShowDepth)
        {
            var frameDescriptor = _depthManager.GetDescriptor();
            var zBuffer = ByteArray2Bmp(_depthManager.GetData(), frameDescriptor.Width, frameDescriptor.Height, PixelFormat.Format16bppGrayScale);
            var resizeFilter = new ResizeNearestNeighbor((int)(ZBufferScale * frameDescriptor.Width), (int)(ZBufferScale * frameDescriptor.Height));
            zBuffer = resizeFilter.Apply(zBuffer);
            zBuffer = AForge.Imaging.Image.Convert16bppTo8bpp(zBuffer);

            if (ZBufferEdgeDetection)
            {
                var filter = new CannyEdgeDetector();
                filter.ApplyInPlace(zBuffer);
                //temp.Save("C:\\Users\\nadm2208\\Desktop\\contour.bmp");

                if (ZBufferHoughDetection)
                {
                    var trans = new HoughCircleTransformation(ballRadius);
                    trans.ProcessImage(zBuffer);
                    zBuffer = trans.ToBitmap();

                    //img.Save("C:\\Users\\nadm2208\\Desktop\\hough.bmp");

                    circles = trans.GetCirclesByRelativeIntensity(circleRelativeIntensity);
                }
            }
            else
            {
                ZBufferHoughDetection = false;
            }

            var convert = new GrayscaleToRGB();
            zBuffer = convert.Apply(zBuffer);

            byte[] arr = Bmp2ByteArray(zBuffer);
            _Texture = new Texture2D(zBuffer.Width, zBuffer.Height, TextureFormat.RGB24, false);
            _Texture.LoadRawTextureData(arr);

        }
        else 
        {
            ZBufferEdgeDetection = false;
            ZBufferHoughDetection = false;
        }

        if (ShowDepth && ZBufferEdgeDetection && ZBufferHoughDetection && Detection)
        {
            DetectProjectile();
        }

        _Texture.Apply();
    }

    public Texture2D GetTexture()
    {
        return _Texture;
    }

    private Projectile DetectProjectile()
    {

        var colorTexture = _colorManager.GetColorTexture();
        var colorFrameDesc = _colorManager.GetDescriptor();

        // temp
        for (int i = 0; i < circles.Length; i++)
        {
            for (int x = -5; x < 5; x++)
            {
                for (int y = -5; y < 5; y++)
                {
                    if (x > 0 && x < _Texture.width && y > 0 && y < _Texture.height)
                    {
                        _Texture.SetPixel(circles[i].X + x, circles[i].Y + y, Color.red);
                    }
                }
            }
        }

        for (int i = 0; i < circles.Length; i++)
        {
            var depthDesc = _depthManager.GetDescriptor();
            var sensor = KinectSensor.GetDefault();
            DepthSpacePoint point = new DepthSpacePoint();
            point.X = (int)(circles[i].X/ZBufferScale);
            point.Y = (int)(circles[i].Y/ZBufferScale);

            var z = _depthManager.GetRawData()[(int)(point.X + point.Y * depthDesc.Width)];
            var csp = sensor.CoordinateMapper.MapDepthPointToColorSpace(point, z);
            if (!float.IsNegativeInfinity(csp.X) && !float.IsNegativeInfinity(csp.Y))
            {
                var color = colorTexture.GetPixel((int)csp.X, (int)csp.Y);
                if (ColorSquareDiff(color, Color.red) < ThresholdColor)
                {
                    for (int x = -5; x < 5; x++)
                    {
                        for (int y = -5; y < 5; y++)
                        {
                            if (x > 0 && x < _Texture.width && y > 0 && y < _Texture.height)
                            {
                                _Texture.SetPixel(circles[i].X + x, circles[i].Y + y, Color.green);
                            }
                        }
                    }
                }
            }
        }
        return Projectile.None;
    }

    float ColorSquareDiff(Color col1, Color col2)
    {
        return (col1.r - col2.r)*(col1.r - col2.r) +
             (col1.g - col2.g)*(col1.g - col2.g) +
             (col1.b - col2.b)*(col1.b - col2.b);
    }

    //private Projectile AnalyseSquare()
    //{
    //    if (!ShowDepth)
    //    {
    //        // square drawing
    //        int xMin = 1920/2 - SquareSize/2;
    //        int xMax = 1920/2 + SquareSize/2;
    //        int yMin = 1080/2 - SquareSize/2;
    //        int yMax = 1080/2 + SquareSize/2;

    //        for (int i = 0; i < SquareSize; i++)
    //        {
    //            _Texture.SetPixel(xMin + i, yMin, Color.red);
    //            _Texture.SetPixel(xMin + i, yMin - 1, Color.red);

    //            _Texture.SetPixel(xMin + i, yMax, Color.red);
    //            _Texture.SetPixel(xMin + i, yMax + 1, Color.red);

    //            _Texture.SetPixel(xMin, yMin + i, Color.red);
    //            _Texture.SetPixel(xMin - 1, yMin + i, Color.red);

    //            _Texture.SetPixel(xMax, yMin + i, Color.red);
    //            _Texture.SetPixel(xMax + 1, yMin + i, Color.red);
    //        }

    //        // Detection
    //        int[] colorMax = {0, 0, 0, 0};

    //        for (int x = xMin + 1; x < xMax - 1; x++)
    //        {
    //            for (int y = yMin + 1; y < yMax - 1; y++)
    //            {
    //                Color current = _Texture.GetPixel(x, y);
    //                short min = -1;
    //                float distMin = float.MaxValue;

    //                for (short c = 0; c < colors.Length; c++)
    //                {
    //                    float dist = (current.r - colors[c].r)*(current.r - colors[c].r) +
    //                                 (current.g - colors[c].g)*(current.g - colors[c].g) +
    //                                 (current.b - colors[c].b)*(current.b - colors[c].b);

    //                    if (dist < distMin)
    //                    {
    //                        min = c;
    //                        distMin = dist;
    //                    }
    //                }

    //                _Texture.SetPixel(x, y, colors[min]);
    //                colorMax[min] += 1;
    //            }
    //        }

    //        int maxValue = colorMax.Max();
    //        int maxIndex = colorMax.ToList().IndexOf(maxValue);

    //        // set zone in top left corner
    //        for (int x = 1; x < 50; x++)
    //        {
    //            for (int y = 1; y < 50; y++)
    //            {
    //                _Texture.SetPixel(x, y, colors[maxIndex]);
    //            }
    //        }
    //        return (Projectile) maxIndex;
    //    }
    //    else
    //    {
    //        ShowDepth = false;
    //    }
    //    return Projectile.None;
    //}

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

//zBuffer = Grayscale.CommonAlgorithms.BT709.Apply(zBuffer);