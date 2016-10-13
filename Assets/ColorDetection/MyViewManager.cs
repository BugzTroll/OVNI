using System.Linq;
using UnityEngine;

public class MyViewManager : MonoBehaviour
{
    public GameObject ColorManager;
    public GameObject DepthManager;
    public GameObject BlobTracker;

    [Range(1, 5)] public int SizeOfPoint = 1;

    public bool ShowDepth = false;
    public bool ShowMeanBuffer = false;
    public bool ShowThresholdedZBuffer = false;
    public bool ShowPositions = false;
    public bool ShowTrajectory = false;
    public bool ShowImpactPoint = false;

    private MyBlobTracker _blobTracker;
    private Texture2D _texture;

    public Texture2D GetTexture()
    {
        return _texture;
    }

    void Start()
    {
        _blobTracker = BlobTracker.GetComponent<MyBlobTracker>();
    }

    void Update()
    {
        // Set texture 
        if (ShowThresholdedZBuffer)
        {
            var bmp = _blobTracker.GetThresholdedZBuffer();
            _texture = new Texture2D(bmp.Width, bmp.Height, TextureFormat.RGB24, false);
            _texture.LoadRawTextureData(MyConverter.Bmp2ByteArray(bmp));
        }

        else if (ShowMeanBuffer)
        {
            var bmp = _blobTracker.GetMeanZBuffer();
            _texture = new Texture2D(bmp.Width, bmp.Height, TextureFormat.RGB24, false);
            _texture.LoadRawTextureData(MyConverter.Bmp2ByteArray(bmp));
        }

        else if (ShowDepth)
        {
            var bmp = _blobTracker.GetResizedZBuffer();
            _texture = new Texture2D(bmp.Width, bmp.Height, TextureFormat.RGB24, false);
            _texture.LoadRawTextureData(MyConverter.Bmp2ByteArray(bmp));
        }
        else
        {
            var bmp = _blobTracker.GetResizedColor();
            _texture = new Texture2D(bmp.Width, bmp.Height, TextureFormat.BGRA32, false);
            _texture.LoadRawTextureData(MyConverter.Bmp2ByteArray(bmp));
        }

        // Draw Positions
        if (ShowPositions)
        {
            var trajectory = ShowColor()
                ? _blobTracker.GetColorTrajectory()
                : _blobTracker.GetDepthTrajectory();

            if (trajectory.Count > 0)
            {
                foreach (var pos in trajectory)
                {
                    int iMin = pos[0] - SizeOfPoint < _texture.width ? (int) pos[0] - SizeOfPoint : _texture.width;
                    int iMax = pos[0] + SizeOfPoint < _texture.width ? (int) pos[0] + SizeOfPoint : _texture.width;
                    int jMin = pos[1] - SizeOfPoint < _texture.height ? (int) pos[1] - SizeOfPoint : _texture.height;
                    int jMax = pos[1] + SizeOfPoint < _texture.height ? (int) pos[1] + SizeOfPoint : _texture.height;

                    for (int i = iMin; i <= iMax; i++)
                    {
                        for (int j = jMin; j <= jMax; j++)
                        {
                            _texture.SetPixel(i, j, Color.green);
                        }
                    }
                }
            }
        }

        // Draw trajectory 
        if (ShowTrajectory && !ShowColor())
        {
            // Draw polynomial curve
            var poly = _blobTracker.GetPolynomialEqtn();
            var linearCoef = _blobTracker.GetLinearEqtn();

            if (poly != null && linearCoef != null)
            {
                for (int z = 500; z < 3000; z++)
                {
                    int x = (int) (linearCoef[0]*z + linearCoef[1]);
                    int y = (int) (poly[0] + poly[1]*z + poly[2]*z*z);

                    // Check if it's in image range
                    if (x >= 0 && x < _texture.width && y >= 0 && y < _texture.height)
                    {
                        _texture.SetPixel(x, y, Color.cyan);
                    }
                }
            }
            // Draw linear curve
            if (_blobTracker.GetSpeed() != Vector3.zero)
            {
                for (int z = 500; z < 3000; z++)
                {
                    var last = _blobTracker.GetDepthTrajectory().Last();
                    var coef = z - last[2];
                    var point = _blobTracker.GetSpeed().normalized*coef + last;

                    var x = (int) point[0];
                    var y = (int) point[1];

                    //check if it's in image range
                    if (x >= 0 && x < _texture.width && y >= 0 && y < _texture.height)
                    {
                        _texture.SetPixel(x, y, Color.magenta);
                    }
                }
            }
        }

        // Draw impact point
        if (ShowImpactPoint)
        {
            int iMax, iMin, jMax, jMin;
            int x, y;

            if (ShowColor())
            {
                var polImpact = _blobTracker.GetColorImpactPoint_poly();
                var linImpact = _blobTracker.GetColorImpactPoint_lin();

                // Draw polynomial impact point in color coordinate
                x = (int) polImpact[0];
                y = (int) polImpact[1];
                iMin = x - SizeOfPoint < _texture.width ? x - SizeOfPoint : _texture.width;
                iMax = x + SizeOfPoint < _texture.width ? x + SizeOfPoint : _texture.width;
                jMin = y - SizeOfPoint < _texture.height ? y - SizeOfPoint : _texture.height;
                jMax = y + SizeOfPoint < _texture.height ? y + SizeOfPoint : _texture.height;

                for (int i = iMin; i <= iMax; i++)
                {
                    for (int j = jMin; j <= jMax; j++)
                    {
                        _texture.SetPixel(i, j, Color.blue);
                    }
                }

                // Draw linear impact point in color coordinate
                x = (int) linImpact[0];
                y = (int) linImpact[1];
                iMin = x - SizeOfPoint < _texture.width ? x - SizeOfPoint : _texture.width;
                iMax = x + SizeOfPoint < _texture.width ? x + SizeOfPoint : _texture.width;
                jMin = y - SizeOfPoint < _texture.height ? y - SizeOfPoint : _texture.height;
                jMax = y + SizeOfPoint < _texture.height ? y + SizeOfPoint : _texture.height;

                for (int i = iMin; i <= iMax; i++)
                {
                    for (int j = jMin; j <= jMax; j++)
                    {
                        _texture.SetPixel(i, j, Color.red);
                    }
                }
            }
            else
            {
                // Draw polynomial impact point in depth coordinate
                x = (int) _blobTracker.GetDepthImpactPoint_poly()[0];
                y = (int) _blobTracker.GetDepthImpactPoint_poly()[1];
                iMin = x - SizeOfPoint < _texture.width ? x - SizeOfPoint : _texture.width;
                iMax = x + SizeOfPoint < _texture.width ? x + SizeOfPoint : _texture.width;
                jMin = y - SizeOfPoint < _texture.height ? y - SizeOfPoint : _texture.height;
                jMax = y + SizeOfPoint < _texture.height ? y + SizeOfPoint : _texture.height;

                for (int i = iMin; i <= iMax; i++)
                {
                    for (int j = jMin; j <= jMax; j++)
                    {
                        _texture.SetPixel(i, j, Color.blue);
                    }
                }

                // Draw linear impact point in depth coordinate
                x = (int) _blobTracker.GetDepthImpactPoint_lin()[0];
                y = (int) _blobTracker.GetDepthImpactPoint_lin()[1];
                iMin = x - SizeOfPoint < _texture.width ? x - SizeOfPoint : _texture.width;
                iMax = x + SizeOfPoint < _texture.width ? x + SizeOfPoint : _texture.width;
                jMin = y - SizeOfPoint < _texture.height ? y - SizeOfPoint : _texture.height;
                jMax = y + SizeOfPoint < _texture.height ? y + SizeOfPoint : _texture.height;

                for (int i = iMin; i <= iMax; i++)
                {
                    for (int j = jMin; j <= jMax; j++)
                    {
                        _texture.SetPixel(i, j, Color.red);
                    }
                }
            }
        }

        _texture.Apply();
    }

    private bool ShowColor()
    {
        return !ShowMeanBuffer && !ShowDepth && !ShowThresholdedZBuffer;
    }
}