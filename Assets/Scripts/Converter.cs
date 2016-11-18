using System;
using UnityEngine;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public class Converter : MonoBehaviour
{
    public static Bitmap ByteArray2Bmp(Byte[] arr, int width, int height, PixelFormat format)
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

    public static Bitmap ShortArray2Bmp(short[] arr, int width, int height, PixelFormat format)
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

    public static byte[] Bmp2ByteArray(System.Drawing.Bitmap img)
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

    public static byte[] ShortArray2ByteArray(ushort[] shortArray)
    {
        byte[] result = new byte[shortArray.Length*sizeof(short)];
        Buffer.BlockCopy(shortArray, 0, result, 0, result.Length);
        return result;
    }

    public static ushort[] ByteArray2ShortArray(byte[] bytes)
    {
        ushort[] result = new ushort[bytes.Length/sizeof(ushort)];
        Buffer.BlockCopy(bytes, 0, result, 0, result.Length);
        return result;
    }
}