using UnityEngine;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Windows.Kinect;
using AForge.Imaging;
using AForge.Imaging.Filters;
using MathNet.Numerics;
using Color = System.Drawing.Color;

public class MyBlobTracker : MonoBehaviour
{
    public GameObject ColorManager;
    public GameObject DepthManager;

    [Range(500, 4500)] public int DepthCenter = 2775;
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
    private int _framesWithoutBlob;
    private double[] _polyEqtn;
    private double[] _linEqtn;
    private float _lastDist;


    void Start()
    {
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
        _linEqtn = null;
        _polyEqtn = null;
        _colorImpactLin = new Vector3(0, 0, 0);
        _colorImpactPol = new Vector3(0, 0, 0);
        _depthImpactLin = new Vector3(0, 0, 0);
        _depthImpactPol = new Vector3(0, 0, 0);
    }

    void Update()
    {
        var depthFrameDescriptor = _depthManager.GetDescriptor();

        // Create z-buffer bitmap from depth manager data (16 bit/pixel greyscale)
        var zBuffer = MyConverter.ByteArray2Bmp(_depthManager.GetData(),
            depthFrameDescriptor.Width,
            depthFrameDescriptor.Height,
            PixelFormat.Format16bppGrayScale);

        // Resize depth buffer
        var depthResizeFilter = new ResizeNearestNeighbor((int) (ZBufferScale*depthFrameDescriptor.Width),
            (int) (ZBufferScale*depthFrameDescriptor.Height));

        _resizedZBuffer = depthResizeFilter.Apply(zBuffer);
        _resizedZBuffer = AForge.Imaging.Image.Convert16bppTo8bpp(_resizedZBuffer);

        // Mean filter
        Mean filter = new Mean();
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

        // Create color bitmap from color manager data (32 bit/pixel argb)
        var colorFrameDescriptor = _colorManager.GetDescriptor();

        var colorImg = MyConverter.ByteArray2Bmp(_colorManager.GetData(),
            colorFrameDescriptor.Width,
            colorFrameDescriptor.Height,
            PixelFormat.Format32bppArgb);

        // Resize color bitmap
        var resizeFilter = new ResizeNearestNeighbor((int) (ColorScale*colorFrameDescriptor.Width),
            (int) (ColorScale*colorFrameDescriptor.Height));

        _resizedColor = resizeFilter.Apply(colorImg);

        // Analyse trajectory
        AnalyseTrajectory();

        // Color analysis
        AnalyseColor();

        // Compute impact point
        if (_polyEqtn != null && _linEqtn != null)
        {
            // Compute impact point with polynomial eqtn
            int polyZ = DepthCenter;
            int polyX = (int) (_linEqtn[0]*polyZ + _linEqtn[1]);
            int polyY = (int) (_polyEqtn[0] + _polyEqtn[1]*polyZ + _polyEqtn[2]*polyZ*polyZ);

            if (polyX >= 0 && polyX < _resizedZBuffer.Width &&
                polyY >= 0 && polyY < _resizedZBuffer.Height)
            {
                _depthImpactPol = new Vector3(polyX, polyY, polyZ);
                _colorImpactPol = DepthPoint2ColorPointVector3(_depthImpactPol);
            }

            // Compute impact point with linear eqtn
            int linZ = DepthCenter;
            var deltaZ = DepthCenter - _trajectory.Last()[2];
            int linX = (int) (deltaZ*_speed.normalized[0] + _trajectory.Last()[0]);
            int linY = (int) (deltaZ*_speed.normalized[1] + _trajectory.Last()[1]);

            if (linX >= 0 && linX < _resizedZBuffer.Width &&
                linY >= 0 && linY < _resizedZBuffer.Height)
            {
                _depthImpactLin = new Vector3(linX, linY, linZ);
                _colorImpactLin = DepthPoint2ColorPointVector3(_depthImpactLin);
            }

            // If we should hit the wall at next frame
            if (_lastDist + _speed[2] > DepthCenter)
            {
                float xScreenSpace = (_colorImpactLin[0] - LeftBotomScreen[0]*_resizedColor.Width)/(RightTopScreen[0] - LeftBotomScreen[0] * _resizedColor.Width) * Camera.main.pixelWidth;
                float yScreenSpace = (1 - (RightTopScreen[1] - (_resizedZBuffer.Height - _colorImpactLin[1]))/ (RightTopScreen[1] - LeftBotomScreen[1] * _resizedColor.Height)) * Camera.main.pixelHeight;
                var screenPoint = new Vector3(xScreenSpace, yScreenSpace, 0);

                _speed.z /= 100;

                GameObject projectileShooterObject = GameObject.Find("PlayerController");
                if (projectileShooterObject != null)
                {
                    shooter = projectileShooterObject.GetComponent<ProjectileShooter>();
                }

                if (shooter)
                {
                    var pt = Camera.main.ScreenToWorldPoint(new Vector3 (screenPoint.x,screenPoint.y,10));
                    shooter.ShootProjectile(
                        pt,
                        new Vector3(0,0,1)
                        );

                    Debug.Log("proj position:" + Camera.main.ScreenToWorldPoint(screenPoint));
                }

                Debug.Log("impact : " + _depthImpactLin);
                Debug.Log("speed : " + _speed);

                ResetTrajectory();
            }
        }
    }

    public void SetShooter(ProjectileShooter shoot)
    {
        shooter = shoot;
    }

    private Vector3 DepthPoint2ColorPointVector3(Vector3 depthPoint)
    {
        DepthSpacePoint point = new DepthSpacePoint();
        point.X = (int) (depthPoint.x/ZBufferScale);
        point.Y = (int) (depthPoint.y/ZBufferScale);

        var depth = _depthManager.GetRawZ((int) point.X, (int) point.Y);

        var colorSpacePoint = _sensor.CoordinateMapper.MapDepthPointToColorSpace(point, depth);

        if (!float.IsNegativeInfinity(colorSpacePoint.X) && !float.IsNegativeInfinity(colorSpacePoint.Y))
        {
            colorSpacePoint.X *= ColorScale;
            colorSpacePoint.Y *= ColorScale;
        }

        return new Vector3(colorSpacePoint.X, colorSpacePoint.Y, depth);
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
            int minI = x - radius < _thresholdedZBuffer.Width ? x - radius : _thresholdedZBuffer.Width;
            int maxI = x + radius < _thresholdedZBuffer.Width ? x + radius : _thresholdedZBuffer.Width;
            int minJ = y - radius < _thresholdedZBuffer.Height ? y - radius : _thresholdedZBuffer.Height;
            int maxJ = y + radius < _thresholdedZBuffer.Height ? y + radius : _thresholdedZBuffer.Height;

            // Get the max Z value in a neighborhood around center of ball
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
                if (_trajectory.Count > MinPointRequired &&
                    _linEqtn != null &&
                    _polyEqtn != null)
                {
                    // Check if the point is over 'curve'

                    //int xcheck = (int) (_linEqtn[0]*maxZ + _linEqtn[1]);
                    //int ycheck = (int) (_polyEqtn[0] + _polyEqtn[1]*maxZ + _polyEqtn[2]*maxZ*maxZ);
                    //if (Math.Abs(x - xcheck) < ThresholdTrajectory && Math.Abs(y - ycheck) < ThresholdTrajectory)
                    {
                        var newPoint = new Vector3(x, y, maxZ);
                        _speed = (newPoint - _trajectory.Last())/(_framesWithoutBlob + 1);
                        _trajectory.Add(newPoint);
                        _lastDist = maxZ;
                    }
                }
                else
                {
                    // Add the point                            // TODO we need to make sure no false point pass through here
                    _trajectory.Add(new Vector3(x, y, maxZ));
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

        // Find the coefficients for polynomial equation
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

            // Find polynomial coefficients
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
        AnalyseBallColorOverTrajectory();
    }

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

        foreach (Vector3 ballPosition in colorTrajectory)
        {
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

    public List<Vector3> GetColorTrajectory()
    {
        List<Vector3> colorTrajectory = new List<Vector3>();
        foreach (var pos in _trajectory)
        {
            var colorPos = DepthPoint2ColorPointVector3(pos);
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