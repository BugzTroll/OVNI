using System;
using System.Linq;
using UnityEngine;

public enum DisplayMode
{
    None = -1,
    CurrentPos,
    AllPosOnTrajectory
}

public class MyViewManager : MonoBehaviour
{
    public GameObject ColorManager;
    public GameObject DepthManager;
    public GameObject BlobTracker;

    [Range(0, 600)] public int ColorCheckSquareSize = 300;

    public bool ShowDepth = false;
    public bool ShowThresholdedZBuffer = false;
    public bool ShowCurrentPos = false;
    public bool ShowPositionOnTrajectory = false;
    public bool ShowTrajectory = false;

    private MyColorSourceManager _colorManager;
    private MyDepthSourceManager _depthManager;
    private MyBlobTracker _blobTracker;
    private Texture2D _texture;

    public Texture2D GetTexture()
    {
        return _texture;
    }

    void Start()
    {
        _depthManager = DepthManager.GetComponent<MyDepthSourceManager>();
        _colorManager = ColorManager.GetComponent<MyColorSourceManager>();
        _blobTracker = BlobTracker.GetComponent<MyBlobTracker>();
    }

    void Update()
    {
        DisplayMode displayMode = ShowCurrentPos
            ? DisplayMode.CurrentPos
            : ShowPositionOnTrajectory
                ? DisplayMode.AllPosOnTrajectory
                : DisplayMode.None;

        if (ShowDepth)
        {
            var bmp = _blobTracker.GetResizedZBuffer();
            _texture = new Texture2D(bmp.Width, bmp.Height, TextureFormat.RGB24, false);
            _texture.LoadRawTextureData(MyConverter.Bmp2ByteArray(bmp));
        }
        else if (ShowThresholdedZBuffer)
        {
            var bmp = _blobTracker.GetThresholdedZBuffer();
            _texture = new Texture2D(bmp.Width, bmp.Height, TextureFormat.RGB24, false);
            _texture.LoadRawTextureData(MyConverter.Bmp2ByteArray(bmp));
        }
        else
        {
            var bmp = _blobTracker.GetResizedColor();
            _texture = new Texture2D(bmp.Width, bmp.Height, TextureFormat.BGRA32, false);
            _texture.LoadRawTextureData(MyConverter.Bmp2ByteArray(bmp));
        }

        int radius = 1;

        switch (displayMode)
        {
            case DisplayMode.AllPosOnTrajectory:
            {
                var trajectory = ShowDepth || ShowThresholdedZBuffer
                    ? _blobTracker.GetDepthTrajectory()
                    : _blobTracker.GetColorTrajectory();
                foreach (var pos in trajectory)
                {
                    int iMax = pos[0] + radius < _texture.width ? (int) pos[0] + radius : _texture.width;
                    int jMax = pos[1] + radius < _texture.height ? (int) pos[1] + radius : _texture.height;

                    for (int i = pos[0] - radius > 0 ? (int) pos[0] - radius : 0; i <= iMax; i++)
                    {
                        for (int j = pos[1] - radius > 0 ? (int) pos[1] - radius : 0; j <= jMax; j++)
                        {
                            _texture.SetPixel(i, j, Color.red);
                        }
                    }
                }
                break;
            }
            case DisplayMode.CurrentPos:
            {
                var trajectory = ShowDepth || ShowThresholdedZBuffer
                    ? _blobTracker.GetDepthTrajectory()
                    : _blobTracker.GetColorTrajectory();
                if (trajectory.Count > 0)
                {
                    var pos = trajectory.Last();
                    int iMax = pos[0] + radius < _texture.width ? (int) pos[0] + radius : _texture.width;
                    int jMax = pos[1] + radius < _texture.height ? (int) pos[1] + radius : _texture.height;

                    for (int i = pos[0] - radius > 0 ? (int) pos[0] - radius : 0; i <= iMax; i++)
                    {
                        for (int j = pos[1] - radius > 0 ? (int) pos[1] - radius : 0; j <= jMax; j++)
                        {
                            _texture.SetPixel(i, j, Color.red);
                        }
                    }
                }
                break;
            }
        }

        if (ShowTrajectory)
        {
            var poly = _blobTracker.GetPoly();
            var linearCoef = _blobTracker.GetLinX();
            if (poly != null && linearCoef != null)
            {
                for (int z = 500; z < 3000; z++)
                {
                    int x = (int) (linearCoef[0]*z + linearCoef[1]);
                    int y = (int) (poly[0] + poly[1]*z + poly[2]*z*z);

                    if (x > 0 && x < _texture.width && y > 0 && y < _texture.height)
                    {
                        _texture.SetPixel(x, y, Color.cyan);

                        if (z == _blobTracker.DepthCenter)
                        {
                            int iMax = x + radius+1 < _texture.width ? x + radius+1 : _texture.width;
                            int jMax = y + radius+1 < _texture.height ? y + radius+1 : _texture.height;

                            for (int i = x - (radius+1) > 0 ? x - (radius+1) : 0; i <= iMax; i++)
                            {
                                for (int j = y - (radius+1) > 0 ? y - (radius+1) : 0; j <= jMax; j++)
                                {
                                    _texture.SetPixel(i, j, Color.green);
                                }
                            }
                        }
                    }
                }


            }
        }

        _texture.Apply();
    }
}


//THIS SHOULD BE REFACTOR SOMEWHERE ELSE

//public bool AnalyseSquareForColor = false;
//private UnityEngine.Color[] colors =
//{
//    Color.red, //r
//    Color.green, //g
//    Color.blue, //b
//    Color.yellow //y
//};
//private Projectile AnalyseSquare()
//{
//    if (!ShowDepth)
//    {
//        // square drawing
//        int xMin = 1920 / 2 - ColorCheckSquareSize / 2;
//        int xMax = 1920 / 2 + ColorCheckSquareSize / 2;
//        int yMin = 1080 / 2 - ColorCheckSquareSize / 2;
//        int yMax = 1080 / 2 + ColorCheckSquareSize / 2;

//        for (int i = 0; i < ColorCheckSquareSize; i++)
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
//        int[] colorMax = { 0, 0, 0, 0 };

//        for (int x = xMin + 1; x < xMax - 1; x++)
//        {
//            for (int y = yMin + 1; y < yMax - 1; y++)
//            {
//                Color current = _Texture.GetPixel(x, y);
//                short min = -1;
//                float distMin = float.MaxValue;

//                for (short c = 0; c < colors.Length; c++)
//                {
//                    float dist = MyHelper.ColorSquareDiff(current, colors[c]);

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
//        //for (int x = 1; x < 50; x++)
//        //{
//        //    for (int y = 1; y < 50; y++)
//        //    {
//        //        _Texture.SetPixel(x, y, colors[maxIndex]);
//        //    }
//        //}

//        return (Projectile)maxIndex;
//    }
//    return Projectile.None;
//}