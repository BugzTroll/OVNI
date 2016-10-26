using System;
using UnityEngine;

public class MyHelper : MonoBehaviour
{
    public static float ColorSquareDiff(System.Drawing.Color col1, System.Drawing.Color col2)
    {
        return Math.Abs(col1.R - col2.R) +
               Math.Abs(col1.G - col2.G) +
               Math.Abs(col1.B - col2.B);
    }

    public static Color ColorAverage(int x, int y, int radius, Texture2D texture)
    {
        Color avColor = Color.black;
        int cpt = 0;
        int iMin = x - radius < 0 ? x - radius : 0;
        int iMax = x + radius < texture.width ? x + radius : texture.width;
        int jMin = y - radius < 0 ? y - radius : 0;
        int jMax = y + radius < texture.height ? y + radius : texture.height;

        for (int i = iMin; i <= iMax; i++)
        {
            for (int j = jMin; j <= jMax; j++)
            {
                avColor += texture.GetPixel(i, j);
                cpt++;
            }
        }
        return avColor/cpt;
    }
}