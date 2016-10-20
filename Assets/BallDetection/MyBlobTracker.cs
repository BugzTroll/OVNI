using UnityEngine;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Windows.Kinect;
using AForge.Imaging;
using AForge.Imaging.Filters;
using MathNet.Numerics;
using UnityEngine.Assertions.Comparers;
using Color = System.Drawing.Color;

public class MyBlobTracker : MonoBehaviour
{
    public GameObject ColorManager;
    public GameObject DepthManager;

    [Range(0, 1)] public float ZBufferScale = 0.5f;
    [Range(0, 1)] public float ColorScale = 0.15f;
    [Range(1, 30)] public int MinSizeBlob = 6;
    [Range(1, 50)] public int MaxSizeBlob = 20;
    [Range(0, 255)] public int ThresholdBlob = 30;
    [Range(0, 50)] public int ThresholdTrajectory = 10;
    [Range(0, 30)] public int FramesWithoutBlobNeededToClear = 10;
    [Range(0, 5)] public int ColorNeighborhoodSize = 2;
    [Range(2, 10)] public int MinPointRequired = 2;
    public Vector2 LeftBotomScreen = Vector2.zero;
    public Vector2 RightTopScreen = Vector2.zero;

    private KinectSensor _sensor;
    private ProjectileShooter shooter;

    private MyDepthSourceManager _depthManager;
    private Bitmap _resizedZBuffer;
    private Bitmap _thresholdedZBuffer;
    private Bitmap _meanZBuffer;

    private MyColorSourceManager _colorManager;
    private GrayscaleToRGB _grey2Rgb;
    private Bitmap _resizedColor;
    private Color _blobColor;

    private List<Vector3> _trajectory;
    private Vector3 _colorImpactLin;
    private Vector3 _colorImpactPol;
    private Vector3 _depthImpactLin;
    private Vector3 _depthImpactPol;
    private Vector3 _speed;
    private Blob[] _blobs;
    public int _ProjectionDistance;
    private int _framesWithoutBlob;
    private double[] _polyEqtn;
    private double[] _linEqtn;
    private float _lastDist;

    public void SetShooter(ProjectileShooter shoot)
    {
        shooter = shoot;
    }

    public void InitProjectionDistance()
    {
        DepthSpacePoint[] p = new DepthSpacePoint[1920 * 1080];
        _sensor.CoordinateMapper.MapColorFrameToDepthSpace(_depthManager.GetRawData(), p);

        var lb = new Vector3(LeftBotomScreen.x*1920, LeftBotomScreen.y*1080, 0);
        var rt = new Vector3(RightTopScreen.x*1920, RightTopScreen.y*1080, 0);

        DepthSpacePoint dlb = p[(int)lb.y * 1920 + (int)lb.x];
        DepthSpacePoint drt = p[(int)rt.y * 1920 + (int)rt.x];

        if (!float.IsNegativeInfinity(dlb.X) && !float.IsNegativeInfinity(dlb.X) &&
            !float.IsNegativeInfinity(drt.X) && !float.IsNegativeInfinity(drt.X))
        {
            int zlb = _depthManager.GetRawZ((int) dlb.X, (int) dlb.Y);
            int zrt = _depthManager.GetRawZ((int) drt.X, (int) drt.Y);
            _ProjectionDistance = Mathf.Min(zlb, zrt);
        }
        else
        {
            Debug.Log("ERROR YOU CLICK OUT OF DEPTH RANGE");
        }
    }

    void Start()
    {
        Application.targetFrameRate = 60;

        _sensor = KinectSensor.GetDefault();
        GameObject projectileShooterObject = GameObject.Find("Player");
        if (projectileShooterObject != null)
        {
            shooter = projectileShooterObject.GetComponent<ProjectileShooter>();
        }
        _grey2Rgb = new GrayscaleToRGB();

        _depthManager = DepthManager.GetComponent<MyDepthSourceManager>();
        _colorManager = ColorManager.GetComponent<MyColorSourceManager>();

        _blobs = new Blob[0];
        _trajectory = new List<Vector3>();
        _framesWithoutBlob = 0;
        _lastDist = 0;
        _speed = new Vector3(0, 0, 0);

        _resizedZBuffer = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
        _thresholdedZBuffer = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
        _meanZBuffer = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
        _resizedColor = new Bitmap(1, 1, PixelFormat.Format32bppArgb);

        _linEqtn = null;
        _polyEqtn = null;

        _colorImpactLin = new Vector3(0, 0, 0);
        _colorImpactPol = new Vector3(0, 0, 0);
        _depthImpactLin = new Vector3(0, 0, 0);
        _depthImpactPol = new Vector3(0, 0, 0);
        _ProjectionDistance = 2700;
    }

    void Update()
    {
        var depthFrameDescriptor = _depthManager.GetDescriptor();

        // Create z-buffer bitmap from depth manager data (16 bit/pixel greyscale)
        var zBuffer = MyConverter.ByteArray2Bmp(_depthManager.GetData(),
            depthFrameDescriptor.Width,
            depthFrameDescriptor.Height,
            PixelFormat.Format16bppGrayScale);

        // Resize depth buffer TODO : peux nuire à la reprojection ?
        var depthResizeFilter = new ResizeNearestNeighbor((int) (ZBufferScale*depthFrameDescriptor.Width),
            (int) (ZBufferScale*depthFrameDescriptor.Height));

        _resizedZBuffer = depthResizeFilter.Apply(zBuffer);
        _resizedZBuffer = AForge.Imaging.Image.Convert16bppTo8bpp(_resizedZBuffer);

        // Create color bitmap from color manager data (32 bit/pixel argb) 
        var colorFrameDescriptor = _colorManager.GetDescriptor();

        var colorImg = MyConverter.ByteArray2Bmp(_colorManager.GetData(),
            colorFrameDescriptor.Width,
            colorFrameDescriptor.Height,
            PixelFormat.Format32bppArgb);

        // Resize color bitmap
        var resizeFilter = new ResizeNearestNeighbor((int)(ColorScale * colorFrameDescriptor.Width),
            (int)(ColorScale * colorFrameDescriptor.Height));

        _resizedColor = resizeFilter.Apply(colorImg);

        // Mean filter TODO : combiner les filtres en les faisant a la main pour faire moins de passes sur le bmp? est-ce qu'on est clutch?
        Mean filter = new Mean();   // TODO tu coute cher mon ptit maudit 3-4 fps
        _meanZBuffer = filter.Apply(_resizedZBuffer);

        // Threshold z-buffer to reduce noise
        Threshold treshFilter = new Threshold(ThresholdBlob);
        _thresholdedZBuffer = treshFilter.Apply(_meanZBuffer);

        // Blob filtering
        var blobFilter = new BlobCounter();
        blobFilter.CoupledSizeFiltering = true;
        blobFilter.FilterBlobs = true;
        blobFilter.MinHeight = MinSizeBlob;
        blobFilter.MinWidth = MinSizeBlob;
        blobFilter.MaxHeight = MaxSizeBlob;
        blobFilter.MaxWidth = MaxSizeBlob;
        blobFilter.ProcessImage(_thresholdedZBuffer);

        // Fill blob array 
        _blobs = blobFilter.GetObjectsInformation();

        // Analyse trajectory
        AnalyseTrajectory();

        // Color analysis
        AnalyseColor();

        //Compute impact point
        if (_polyEqtn != null && _linEqtn != null)
        {
            // Compute impact point with polynomial eqtn TODO: remove this part
            int polyZ = _ProjectionDistance;
            int polyX = (int)(_linEqtn[0] * polyZ + _linEqtn[1]);
            int polyY = (int)(_polyEqtn[0] + _polyEqtn[1] * polyZ + _polyEqtn[2] * polyZ * polyZ);

            if (polyX >= 0 && polyX < 512 &&
                polyY >= 0 && polyY < 424)
            {
                _depthImpactPol = new Vector3(polyX, polyY, polyZ);
                _colorImpactPol = DepthPoint2ColorPoint(_depthImpactPol);
            }

            // Compute impact point with linear eqtn TODO: pour l'affichage a mettre en debug mode seulement
            int linZ = _ProjectionDistance;
            var deltaZ = _ProjectionDistance - _trajectory.Last()[2];
            int linX = (int)(deltaZ * _speed.normalized[0] + _trajectory.Last()[0]);
            int linY = (int)(deltaZ * _speed.normalized[1] + _trajectory.Last()[1]);

            if (linX >= 0 && linX < 512 &&
                linY >= 0 && linY < 424)
            {
                _depthImpactLin = new Vector3(linX, linY, linZ);
                _colorImpactLin = DepthPoint2ColorPoint(_depthImpactLin);
            }

            // If we should hit the wall at next frame
            if (_lastDist + _speed[2] > _ProjectionDistance - 500)
            {
                float xNormalized = (1 - (_colorImpactLin[0] - LeftBotomScreen[0] * 1920) /
                                     (RightTopScreen[0] * 1920 - LeftBotomScreen[0] * 1920));

                float yNormalized = (1 -
                                      (RightTopScreen[1] * 1080 - (1080 - _colorImpactLin[1])) /
                                      (RightTopScreen[1] * 1080 - LeftBotomScreen[1] * 1080));

                if (xNormalized >= 0.0f && xNormalized <= 1.0f &&
                    yNormalized >= 0.0f && yNormalized <= 1.0f)
                {

                    GameObject projectileShooterObject = GameObject.Find("PlayerController");
                    if (projectileShooterObject != null)
                    {
                        shooter = projectileShooterObject.GetComponent<ProjectileShooter>();
                    }

                    if (shooter)
                    {
                        shooter.ShootProjectile(
                            new Vector3(xNormalized * Screen.width, yNormalized * Screen.height),
                            _speed.normalized
                        );
                    }

                    //TODO : printer seulement en debug mode
                    Debug.Log("impact : " + _depthImpactLin);
                    Debug.Log("speed : " + _speed.normalized);


                }

                ResetTrajectory();
            }
        }
    }

    //TODO : Utiliser le moins possible
    private Vector3 DepthPoint2ColorPoint(Vector3 depthPoint)
    {
        DepthSpacePoint point = new DepthSpacePoint();
        point.X = (int)depthPoint.x;
        point.Y = (int)depthPoint.y;
        var z = _depthManager.GetRawZ((int) point.X, (int) point.Y);
        var colorSpacePoint = _sensor.CoordinateMapper.MapDepthPointToColorSpace(point, z);

        return new Vector3(colorSpacePoint.X, colorSpacePoint.Y, z);
    }

    #region Trajectory

    private void ResetTrajectory()
    {
        _speed = new Vector3(0, 0, 0);
        _trajectory.Clear();
        _lastDist = 0;
        _polyEqtn = null;
        _linEqtn = null;
    }

    private void AnalyseTrajectory()
    {
        // Get biggest blob
        Blob biggestBlob = null;
        int maxArea = 0;
        foreach (var blob in _blobs)
        {
            if (blob.Area > maxArea)
            {
                maxArea = blob.Area;
                biggestBlob = blob;
            }
        }

        // Get ball position
        if (biggestBlob != null)
        {
            _framesWithoutBlob = 0;

            int x = (int) biggestBlob.CenterOfGravity.X;
            int y = (int) biggestBlob.CenterOfGravity.Y;

            int maxZ = 0;
            int radius = 1;

            // Manage side of image
            int minI = x - radius < _resizedZBuffer.Width ? x - radius : _resizedZBuffer.Width;
            int maxI = x + radius < _resizedZBuffer.Width ? x + radius : _resizedZBuffer.Width;
            int minJ = y - radius < _resizedZBuffer.Height ? y - radius : _resizedZBuffer.Height;
            int maxJ = y + radius < _resizedZBuffer.Height ? y + radius : _resizedZBuffer.Height;

            // Get the max Z value in a neighborhood around center of ball TODO : au lieu  de prendre le max, on en prend un des qu'on le trouve, a valider
            for (int i = minI; i < maxI; i++)
            {
                for (int j = minJ; j < maxJ; j++)
                {
                    // TODO we should take the median instead of max
                    int z = _depthManager.GetRawZ((int) (i/ZBufferScale), (int) (j/ZBufferScale));
                    if (z > maxZ)
                    {
                        maxZ = z;
                    }
                }
            }

            // Check that the z is in kinect range and bigger than last point 
            if (maxZ < KinectSensor.GetDefault().DepthFrameSource.DepthMaxReliableDistance &&
                maxZ > KinectSensor.GetDefault().DepthFrameSource.DepthMinReliableDistance &&
                maxZ > _lastDist)
            {
                if (_trajectory.Count > MinPointRequired)
                {
                    // Check if the point is over 'curve'

                    //int xcheck = (int) (_linEqtn[0]*maxZ + _linEqtn[1]);
                    //int ycheck = (int) (_polyEqtn[0] + _polyEqtn[1]*maxZ + _polyEqtn[2]*maxZ*maxZ);
                    //if (Math.Abs(x - xcheck) < ThresholdTrajectory && Math.Abs(y - ycheck) < ThresholdTrajectory)
                    {
                        var newPoint = new Vector3(x/ZBufferScale, y/ZBufferScale, maxZ);       //full depth rez 512x424
                        _speed = (newPoint - _trajectory.Last())/(_framesWithoutBlob + 1);
                        _trajectory.Add(newPoint);
                        _lastDist = maxZ;
                    }
                }
                else
                {
                    // Add the point // TODO we need to make sure no false point pass through here
                    _trajectory.Add(new Vector3(x/ZBufferScale, y/ZBufferScale, maxZ));         //full depth rez 512x424
                    _lastDist = maxZ;
                }
            }
        }
        else
        {
            // No ball detected
            _framesWithoutBlob++;
            if (_framesWithoutBlob >= FramesWithoutBlobNeededToClear)
            {
                ResetTrajectory();
            }
        }

        // Find the coefficients for polynomial equation //TODO : bye bye
        ComputePolynomialEquation();
    }

    private void ComputePolynomialEquation()
    {
        if (_trajectory.Count > MinPointRequired)
        {
            // Find linear equation of X by Z
            _linEqtn = new double[2];
            _linEqtn[0] = (_trajectory[0][0] - _trajectory.Last()[0])/(_trajectory[0][2] - _trajectory.Last()[2]);
            _linEqtn[1] = _trajectory[0][0] - _linEqtn[0]*_trajectory[0][2];

            // Projection over Z plane
            double[] zData = new double[_trajectory.Count];
            double[] yData = new double[_trajectory.Count];

            // Generate parametric data for regression
            for (int i = 0; i < _trajectory.Count; i++)
            {
                var pos = _trajectory[i];
                zData[i] = pos[2];
                yData[i] = pos[1];
            }

            // Find polynomial coefficients //TODO long a computer ?
            _polyEqtn = Fit.Polynomial(zData, yData, 2);

            // Check if the curve is upward and swap to linear equation 
            if (_polyEqtn[2] < 0)
            {
                _polyEqtn[1] = (_trajectory[0][1] - _trajectory.Last()[1])/(_trajectory[0][2] - _trajectory.Last()[2]);
                _polyEqtn[0] = _trajectory[0][1] - _polyEqtn[1]*_trajectory[0][2];
                _polyEqtn[2] = 0.0f;
            }
        }
    }

    private void ComputeLinearEquation()
    {
        // ALLO
    }

    #endregion

    #region Color

    private void AnalyseColor()
    {
        //AnalyseBallColorOverTrajectory();
    }

    //TODO : fix me pliz
    private void AnalyseBallColorOverTrajectory()
    {
        List<int> colorMax = new List<int>() {0, 0, 0};

        Color[] colors =
        {
            Color.Red,
            Color.Green,
            Color.Blue
        };

        List<Vector3> colorTrajectory = GetColorTrajectory();

        foreach (Vector3 position in colorTrajectory)
        {
            var ballPosition = new Vector3(position.x/ColorScale, position.y/ColorScale);
            for (int x = -ColorNeighborhoodSize; x < ColorNeighborhoodSize + 1; x++)
            {
                for (int y = -ColorNeighborhoodSize; y < ColorNeighborhoodSize + 1; y++)
                {
                    if ((int) ballPosition[0] + x >= 0 && (int) ballPosition[0] + x < _resizedColor.Width &&
                        (int) ballPosition[1] + y >= 0 && (int) ballPosition[1] + y < _resizedColor.Height)
                    {
                        Color current = _resizedColor.GetPixel((int) ballPosition[0] + x,
                            (int) ballPosition[1] + y);

                        short min = -1;
                        float distMin = float.MaxValue;

                        for (short c = 0; c < colors.Length; c++)
                        {
                            float dist = MyHelper.ColorSquareDiff(current, colors[c]);

                            if (dist < distMin)
                            {
                                min = c;
                                distMin = dist;
                            }
                        }

                        colorMax[min] += 1;
                    }
                }
            }
        }

        int maxValue = colorMax.Max();
        int maxIndex = colorMax.IndexOf(maxValue);
        _blobColor = colors[maxIndex];
    }

    #endregion

    #region Getter

    public List<Vector3> GetDepthTrajectory()
    {
        return _trajectory;
    }

    //TODO Debug mode seulement et ne pas recalculer a chaque frame, maintenir la liste a jour a la place
    public List<Vector3> GetColorTrajectory()
    {
        List<Vector3> colorTrajectory = new List<Vector3>();
        foreach (var pos in _trajectory)
        {
            var colorPos = DepthPoint2ColorPoint(new Vector3(pos.x, pos.y, pos.z));
            colorTrajectory.Add(colorPos);
        }

        return colorTrajectory;
    }

    public double[] GetPolynomialEqtn()
    {
        return _polyEqtn;
    }

    public double[] GetLinearEqtn()
    {
        return _linEqtn;
    }

    public Vector3 GetSpeed()
    {
        return _speed;
    }

    public Bitmap GetResizedZBuffer()
    {
        return _grey2Rgb.Apply(_resizedZBuffer);
    }

    public Bitmap GetMeanZBuffer()
    {
        return _grey2Rgb.Apply(_meanZBuffer);
    }

    public Bitmap GetThresholdedZBuffer()
    {
        return _grey2Rgb.Apply(_thresholdedZBuffer);
    }

    public Bitmap GetResizedColor()
    {
        return _resizedColor;
    }

    public Vector3 GetColorImpactPoint_poly()
    {
        return _colorImpactPol;
    }

    public Vector3 GetColorImpactPoint_lin()
    {
        return _colorImpactLin;
    }

    public Vector3 GetDepthImpactPoint_poly()
    {
        return _depthImpactPol;
    }

    public Vector3 GetDepthImpactPoint_lin()
    {
        return _depthImpactLin;
    }

    #endregion
}