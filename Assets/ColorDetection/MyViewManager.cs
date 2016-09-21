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
using Color = UnityEngine.Color;
using WrapMode = UnityEngine.WrapMode;

public class MyViewManager : MonoBehaviour
{
    public GameObject ColorManager;
    public GameObject DepthManager;
    public bool ShowDepth = false;
    public bool ZBufferEdgeDetection = false;
    public bool ZBufferHoughDetection = false;
    public bool Detection = false;
    [Range(0, 600)] public int SquareSize = 300;
    public bool TakePicture = false;
    [Range(0, 1)] public float PictureScaleFactor = 0.5f;

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

        if (TakePicture)
        {
            var frameDexcriptor = _colorManager.GetDescriptor();
            var bmp = ByteArray2Bmp(_colorManager.GetData(), frameDexcriptor.Width, frameDexcriptor.Height,
                PixelFormat.Format32bppArgb);
            var res = ResizeBmp(bmp, PictureScaleFactor);
            res.Save("C:\\Users\\nadm2208\\Desktop\\Picture.bmp");
            TakePicture = false;
        }

        if (ShowDepth)
        {
            var frameDescriptor = _depthManager.GetDescriptor();
            var zBuffer = new Bitmap(frameDescriptor.Width/2, frameDescriptor.Height/2, PixelFormat.Format32bppArgb);
            var rawData = _depthManager.GetRawData();

            for (int i = 0; i < frameDescriptor.Width; i += 2)
            {
                for (int j = 0; j < frameDescriptor.Height; j += 2)
                {
                    var pîxelGreyLvl = (short) ((rawData[i + j*frameDescriptor.Width]*255)/8000.0f);

                    zBuffer.SetPixel(i/2, j/2, System.Drawing.Color.FromArgb(
                        pîxelGreyLvl,
                        pîxelGreyLvl,
                        pîxelGreyLvl,
                        255));
                }
            }

            if (ZBufferEdgeDetection)
            {
                //ZBufferEdgeDetection = false;

                zBuffer = Grayscale.CommonAlgorithms.BT709.Apply(zBuffer);
                var filter = new CannyEdgeDetector();
                filter.ApplyInPlace(zBuffer);
                //temp.Save("C:\\Users\\nadm2208\\Desktop\\contour.bmp");

                if (ZBufferHoughDetection)
                {
                    var trans = new HoughCircleTransformation(10);
                    trans.ProcessImage(zBuffer);
                    zBuffer = trans.ToBitmap();
                    //img.Save("C:\\Users\\nadm2208\\Desktop\\hough.bmp");

                    HoughCircle[] circles = trans.GetCirclesByRelativeIntensity(0.9);
                    int a = 0;
                }

                var convert = new GrayscaleToRGB();
                zBuffer = convert.Apply(zBuffer);
                //zBuffer.Save("C:\\Users\\nadm2208\\Desktop\\RBG.bmp");

                byte[] arr = Bmp2ByteArray(zBuffer);
                _Texture = new Texture2D(zBuffer.Width, zBuffer.Height, TextureFormat.RGB24, false);
                _Texture.LoadRawTextureData(arr);
            }
            else
            {
                byte[] arr = Bmp2ByteArray(zBuffer);
                _Texture = new Texture2D(zBuffer.Width, zBuffer.Height, TextureFormat.ARGB32, false);
                _Texture.LoadRawTextureData(arr);
                ZBufferHoughDetection = false;
            }
            Detection = false;
        }
        else 
        {
            ZBufferEdgeDetection = false;
            ZBufferHoughDetection = false;
            if (Detection)
            {
                DetectProjectile();
            }
        }

        _Texture.Apply();
    }

    public Texture2D GetTexture()
    {
        return _Texture;
    }

    private Projectile DetectProjectile()
    {
        if (!ShowDepth)
        {
            // square drawing
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

            // set zone in top left corner
            for (int x = 1; x < 50; x++)
            {
                for (int y = 1; y < 50; y++)
                {
                    _Texture.SetPixel(x, y, colors[maxIndex]);
                }
            }
            return (Projectile) maxIndex;
        }
        else
        {
            ShowDepth = false;
        }
        return Projectile.None;
    }

    public static Bitmap ResizeBmp(Bitmap bmp, float scale)
    {
        var width = (int) (bmp.Width*scale);
        var height = (int) (bmp.Height*scale);
        var brush = new SolidBrush(System.Drawing.Color.Black);
        var result = new Bitmap((int) width, (int) height);
        var graph = System.Drawing.Graphics.FromImage(result);

        // uncomment for higher quality output
        //graph.InterpolationMode = InterpolationMode.High;
        //graph.CompositingQuality = CompositingQuality.HighQuality;
        //graph.SmoothingMode = SmoothingMode.AntiAlias;

        graph.FillRectangle(brush, new RectangleF(0, 0, width, height));
        graph.DrawImage(bmp, new Rectangle(0, 0, width, height));

        return result;
    }

    #region Converters
    Bitmap ByteArray2Bmp(Byte[] arr, int width, int height, PixelFormat format)
    {
        Bitmap img = new Bitmap(width, height, format);

        // Bmp->ByteArray
        BitmapData bitmapData = img.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.ReadOnly,
            img.PixelFormat);

        // Copy bitmap to byte[]
        Marshal.Copy(arr, 0, bitmapData.Scan0, arr.Length);
        img.UnlockBits(bitmapData);

        return img;
    }

    byte[] Bmp2ByteArray(System.Drawing.Bitmap img)
    {
        BitmapData bitmapData = img.LockBits(
            new Rectangle(0, 0, img.Width, img.Height),
            ImageLockMode.WriteOnly,
            img.PixelFormat);

        byte[] result = new byte[System.Math.Abs(bitmapData.Stride)*bitmapData.Height];

        Marshal.Copy(bitmapData.Scan0, result, 0, result.Length);
        img.UnlockBits(bitmapData);

        return result;
    }
    #endregion
}