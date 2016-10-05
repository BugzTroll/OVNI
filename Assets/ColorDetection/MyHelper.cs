using System;
using UnityEngine;

public class MyHelper : MonoBehaviour
{
    public static float ColorSquareDiff(Color col1, Color col2)
    {
        return Math.Abs(col1.r - col2.r) +
               Math.Abs(col1.g - col2.g) +
               Math.Abs(col1.b - col2.b);
    }

    public static Color ColorAverage(int x, int y, int radius, Texture2D texture)
    {
        Color avColor = Color.black;
        int cpt = 0;
        int iMax = x + radius < texture.width ? x + radius : texture.width;
        int jMax = y + radius < texture.height ? y + radius : texture.height;

        for (int i = x - radius > 0 ? x - radius : 0; i <= iMax; i++)
        {
            for (int j = y - radius > 0 ? y - radius : 0; j <= jMax; j++)
            {
                avColor += texture.GetPixel(i, j);
                cpt++;
            }
        }
        return avColor/cpt;
    }
}