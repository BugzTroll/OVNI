using System.Linq;
using UnityEngine;

public class ViewManager : MonoBehaviour
{
    public GameObject ColorManager;
    public GameObject DepthManager;
    public GameObject BlobTracker;

    [Range(1, 5)] public int SizeOfPoint = 1;

    //TODO : devrait tout être a off en PLAYMODE, sert seulement dans la visualisation
    public bool ShowDepth = false;
    public bool ShowMeanBuffer = false;
    public bool ShowThresholdedZBuffer = false;
    public bool ShowPositions = false;
    public bool ShowTrajectory = false;
    public bool ShowImpactPoint = false;

    private BlobTracker _blobTracker;
    private Texture2D _texture;

    public void Shutdown()
    {
        if (!DebugManager.Debug)
        {
            var colorSourceManager = ColorManager.GetComponent<ColorSourceManager>();
            if (colorSourceManager)
            {
                colorSourceManager.Shutdown();
            }
            this.gameObject.SetActive(false);
        }
    }

    public Texture2D GetTexture()
    {
        return _texture;
    }

    void Start()
    {
        _blobTracker = BlobTracker.GetComponent<BlobTracker>();
    }

    void Update()
    {
        // Set texture 
        if (ShowThresholdedZBuffer)
        {
            var bmp = _blobTracker.GetThresholdedZBuffer();
            _texture = new Texture2D(bmp.Width, bmp.Height, TextureFormat.RGB24, false);
            _texture.LoadRawTextureData(Converter.Bmp2ByteArray(bmp));
        }

        else if (ShowMeanBuffer)
        {
            var bmp = _blobTracker.GetMeanZBuffer();
            _texture = new Texture2D(bmp.Width, bmp.Height, TextureFormat.RGB24, false);
            _texture.LoadRawTextureData(Converter.Bmp2ByteArray(bmp));
        }

        else if (ShowDepth)
        {
            var bmp = _blobTracker.GetResizedZBuffer();
            _texture = new Texture2D(bmp.Width, bmp.Height, TextureFormat.RGB24, false);
            _texture.LoadRawTextureData(Converter.Bmp2ByteArray(bmp));
        }
        else
        {
            var bmp = _blobTracker.GetResizedColor();
            _texture = new Texture2D(bmp.Width, bmp.Height, TextureFormat.BGRA32, false);
            _texture.LoadRawTextureData(Converter.Bmp2ByteArray(bmp));
        }

        var scale = ShowColor()
            ? _blobTracker.ColorScale
            : _blobTracker.ZBufferScale;

        // Draw Positions
        if (ShowPositions)
        {
            var trajectory = ShowColor()
                ? _blobTracker.GetColorTrajectory()
                : _blobTracker.GetDepthTrajectory();

            foreach (var position in trajectory)
            {
                var pos = position*scale;
                int iMin = (pos[0] - SizeOfPoint >= 0) ? (int) pos[0] - SizeOfPoint :0;
                int iMax = (pos[0] + SizeOfPoint < _texture.width) ? (int) pos[0] + SizeOfPoint : _texture.width;
                int jMin = (pos[1] - SizeOfPoint >= 0) ? (int) pos[1] - SizeOfPoint : 0;
                int jMax = (pos[1] + SizeOfPoint < _texture.height) ? (int) pos[1] + SizeOfPoint : _texture.height;

                for (int i = iMin; i <= iMax; i++)
                {
                    for (int j = jMin; j <= jMax; j++)
                    {
                        _texture.SetPixel(i, j, Color.green);
                    }
                }
            }
        }

        // Draw trajectory 
        if (ShowTrajectory && !ShowColor())
        {
            // Draw linear curve
            if (_blobTracker.GetSpeed() != Vector3.zero)
            {
                for (int z = 500; z < 3000; z++)
                {
                    var last = _blobTracker.GetDepthTrajectory().Last();
                    var coef = z - last[2];
                    var point = _blobTracker.GetSpeed().normalized*coef + last;

                    var x = (int) (point[0]*scale);
                    var y = (int) (point[1]*scale);

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
                var linImpact = _blobTracker.GetColorImpactPoint_lin()*scale;
                // Draw linear impact point in color coordinate
                x = (int) linImpact[0];
                y = (int) linImpact[1];
                iMin = x - SizeOfPoint >= 0 ? x - SizeOfPoint : 0;
                iMax = x + SizeOfPoint < _texture.width ? x + SizeOfPoint : _texture.width;
                jMin = y - SizeOfPoint >= 0 ? y - SizeOfPoint : 0;
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
                // Draw linear impact point in depth coordinate
                x = (int) (_blobTracker.GetDepthImpactPoint_lin()[0]*scale);
                y = (int) (_blobTracker.GetDepthImpactPoint_lin()[1]*scale);
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