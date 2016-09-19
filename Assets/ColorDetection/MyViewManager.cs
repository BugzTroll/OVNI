using System;
using UnityEngine;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using Color = UnityEngine.Color;

public class MyViewManager : MonoBehaviour
{
    public GameObject ColorManager;
    public GameObject DepthManager;
    public bool ShowDepth = false;
    public bool Detection = false;
    public int SquareSize = 300;

    private MyColorSourceManager _colorManager;
    private MyDepthSourceManager _depthManager;
    private Texture2D _Texture;

    private enum Projectile
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
        var yeah = _Texture.GetPixel(10, 10);
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
            int xMin = 1920 / 2 - SquareSize / 2;
            int xMax = 1920 / 2 + SquareSize / 2;
            int yMin = 1080 / 2 - SquareSize / 2;
            int yMax = 1080 / 2 + SquareSize / 2;

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
            int[] colorMax = { 0, 0, 0, 0 };

            for (int x = xMin + 1; x < xMax - 1; x++)
            {
                for (int y = yMin + 1; y < yMax - 1; y++)
                {
                    Color current = _Texture.GetPixel(x, y);
                    short min = -1;
                    float distMin = float.MaxValue;

                    for (short c = 0; c < colors.Length; c++)
                    {
                        float dist = (current.r - colors[c].r) * (current.r - colors[c].r) +
                                     (current.g - colors[c].g) * (current.g - colors[c].g) +
                                     (current.b - colors[c].b) * (current.b - colors[c].b);

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
            return (Projectile)maxIndex;
        }
        return 0;
    }

    #region Converters
    System.Drawing.Bitmap ByteArray2Bmp(Byte[] arr, int width, int height, PixelFormat format)
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

        byte[] result = new byte[System.Math.Abs(bmpdata.Stride)*bmpdata.Height];

        IntPtr ptr = bmpdata.Scan0;
        Marshal.Copy(result, 0, ptr, result.Length);
        bmp.UnlockBits(bmpdata);

        return result;
    }

    #endregion
}