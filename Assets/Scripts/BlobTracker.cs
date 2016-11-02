using System;
using UnityEngine;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Windows.Kinect;
using AForge.Imaging;
using AForge.Imaging.Filters;
using UnityEngine.Events;
using Color = System.Drawing.Color;
using Image = AForge.Imaging.Image;

public class BlobTracker : MonoBehaviour
{
    public static event UnityAction<float, float> ImpactPointDetected;

    public GameObject ColorManager;
    public GameObject DepthManager;
    public Vector2[] ScreenCorners = new Vector2[4];
    [Range(0, 1)] public float ZBufferScale = 0.5f;
    [Range(0, 1)] public float ColorScale = 0.15f;
    [Range(1, 30)] public int MinSizeBlob = 6;
    [Range(1, 50)] public int MaxSizeBlob = 20;
    [Range(0, 255)] public int ThresholdBlob = 30;
    [Range(0, 30)] public int FramesWithoutBlobNeededToClear = 10;
    [Range(2, 5)] public int MinPointRequired = 2;
    [Range(0, 100)] public float PercentThresholdZMiss = 80.0f;

    private KinectSensor _sensor;
    private ProjectileShooter shooter = null;
    private int _nbFrameBetweenThrow;

    private DepthSourceManager _depthManager;
    private Bitmap _resizedZBuffer;
    private Bitmap _thresholdedZBuffer;
    private Bitmap _meanZBuffer;

    private ColorSourceManager _colorManager;
    private GrayscaleToRGB _grey2Rgb;
    private Bitmap _resizedColor;
    private Color _blobColor;

    private List<Vector3> _trajectory;
    private Vector3 _colorImpactLin;
    private Vector3 _depthImpactLin;
    private Vector3 _speed;
    private Blob[] _blobs;
    private int _projectionDistance;
    private int _framesWithoutBlob;
    private float _lastDist;

    public void InitProjectionDistance()
    {
        DepthSpacePoint[] p = new DepthSpacePoint[1920*1080];
        _sensor.CoordinateMapper.MapColorFrameToDepthSpace(_depthManager.GetRawData(), p);

        var lb = new Vector2(ScreenCorners[0].x*1920, ScreenCorners[0].y*1080); // left bottom
        var lt = new Vector2(ScreenCorners[1].x*1920, ScreenCorners[1].y*1080); // left top
        var rt = new Vector2(ScreenCorners[2].x*1920, ScreenCorners[2].y*1080); // right top
        var rb = new Vector2(ScreenCorners[3].x*1920, ScreenCorners[3].y*1080); // right bottom
        var mm = (lb + rt)/2.0f; // midle

        DepthSpacePoint dlb = p[(int) lb.y*1920 + (int) lb.x];
        DepthSpacePoint dlt = p[(int) lt.y*1920 + (int) lt.x];
        DepthSpacePoint drt = p[(int) rt.y*1920 + (int) rt.x];
        DepthSpacePoint drb = p[(int) rb.y*1920 + (int) rb.x];
        DepthSpacePoint dmm = p[(int) mm.y*1920 + (int) mm.x];

        if (!float.IsNegativeInfinity(dlb.X) && !float.IsNegativeInfinity(dlb.Y) &&
            !float.IsNegativeInfinity(dlt.X) && !float.IsNegativeInfinity(dlt.Y) &&
            !float.IsNegativeInfinity(drt.X) && !float.IsNegativeInfinity(drt.Y) &&
            !float.IsNegativeInfinity(drb.X) && !float.IsNegativeInfinity(drb.Y) &&
            !float.IsNegativeInfinity(dmm.X) && !float.IsNegativeInfinity(dmm.Y))
        {
            int zlb = _depthManager.GetRawZ((int) dlb.X, (int) dlb.Y);
            int zlt = _depthManager.GetRawZ((int) dlt.X, (int) dlt.Y);
            int zrt = _depthManager.GetRawZ((int) drt.X, (int) drt.Y);
            int zrb = _depthManager.GetRawZ((int) drb.X, (int) drb.Y);
            int zmm = _depthManager.GetRawZ((int) dmm.X, (int) dmm.Y);

            _projectionDistance = Mathf.Min(zlb, Mathf.Min(zlt, Mathf.Min(zrb, Math.Min(zmm, zrt))));
        }
        else
        {
            if (DebugManager.Debug)
            {
                Debug.Log("ERROR YOU CLICK OUT OF DEPTH RANGE");
            }
        }
    }

    void Start()
    {
        _sensor = KinectSensor.GetDefault();
        _grey2Rgb = new GrayscaleToRGB();

        _depthManager = DepthManager.GetComponent<DepthSourceManager>();
        _colorManager = ColorManager.GetComponent<ColorSourceManager>();

        _blobs = new Blob[0];
        _trajectory = new List<Vector3>();
        _speed = new Vector3(0, 0, 0);
        _framesWithoutBlob = 0;
        _lastDist = 0;
        _nbFrameBetweenThrow = 0;
        _projectionDistance = 0;

        _resizedZBuffer = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
        _thresholdedZBuffer = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
        _meanZBuffer = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
        _resizedColor = new Bitmap(1, 1, PixelFormat.Format32bppArgb);

        _colorImpactLin = new Vector3(0, 0, 0);
        _depthImpactLin = new Vector3(0, 0, 0);
    }

    void Update()
    {
        var depthFrameDescriptor = _depthManager.GetDescriptor();

        // Create z-buffer bitmap from depth manager data (16 bit/pixel greyscale)
        var zBuffer = Converter.ByteArray2Bmp(_depthManager.GetData(),
            depthFrameDescriptor.Width,
            depthFrameDescriptor.Height,
            PixelFormat.Format16bppGrayScale);

        // Resize depth buffer
        var depthResizeFilter = new ResizeNearestNeighbor((int) (ZBufferScale*depthFrameDescriptor.Width),
            (int) (ZBufferScale*depthFrameDescriptor.Height));

        _resizedZBuffer = depthResizeFilter.Apply(zBuffer);
        _resizedZBuffer = Image.Convert16bppTo8bpp(_resizedZBuffer);

        // Create color bitmap from color manager data (32 bit/pixel argb) 
        var colorFrameDescriptor = _colorManager.GetDescriptor();

        var colorImg = Converter.ByteArray2Bmp(_colorManager.GetData(),
            colorFrameDescriptor.Width,
            colorFrameDescriptor.Height,
            PixelFormat.Format32bppArgb);

        // Resize color bitmap
        var resizeFilter = new ResizeNearestNeighbor((int) (ColorScale*colorFrameDescriptor.Width),
            (int) (ColorScale*colorFrameDescriptor.Height));

        _resizedColor = resizeFilter.Apply(colorImg);

        // Mean filter TODO : combiner les filtres ( threshold et median ) en les faisant a la main pour faire moins de passes sur le bmp
        Mean filter = new Mean(); // TODO coute cher (3-4 fps) 
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

        // Compute impact point and notify game manager
        ComputeImpactPoint();
    }

    private Vector3 DepthPoint2ColorPoint(Vector3 depthPoint)
    {
        DepthSpacePoint point = new DepthSpacePoint();
        point.X = (int) depthPoint.x;
        point.Y = (int) depthPoint.y;
        var z = _depthManager.GetRawZ((int) point.X, (int) point.Y);
        var colorSpacePoint = _sensor.CoordinateMapper.MapDepthPointToColorSpace(point, z);

        return new Vector3(colorSpacePoint.X, colorSpacePoint.Y, z);
    }

    float[] Projection2Square(Vector2[] Q, float x, float y)
    {
        float ax = (x - Q[0].x) + (Q[1].x - Q[0].x)*(y - Q[0].y)/(Q[0].y - Q[1].y);
        float a3x = (Q[3].x - Q[0].x) + (Q[1].x - Q[0].x)*(Q[3].y - Q[0].y)/(Q[0].y - Q[1].y);
        float a2x = (Q[2].x - Q[0].x) + (Q[1].x - Q[0].x)*(Q[2].y - Q[0].y)/(Q[0].y - Q[1].y);
        float ay = (y - Q[0].y) + (Q[3].y - Q[0].y)*(x - Q[0].x)/(Q[0].x - Q[3].x);
        float a1y = (Q[1].y - Q[0].y) + (Q[3].y - Q[0].y)*(Q[1].x - Q[0].x)/(Q[0].x - Q[3].x);
        float a2y = (Q[2].y - Q[0].y) + (Q[3].y - Q[0].y)*(Q[2].x - Q[0].x)/(Q[0].x - Q[3].x);
        float bx = x*y - Q[0].x*Q[0].y + (Q[1].x*Q[1].y - Q[0].x*Q[0].y)*(y - Q[0].y)/(Q[0].y - Q[1].y);
        float b3x = Q[3].x*Q[3].y - Q[0].x*Q[0].y + (Q[1].x*Q[1].y - Q[0].x*Q[0].y)*(Q[3].y - Q[0].y)/(Q[0].y - Q[1].y);
        float b2x = Q[2].x*Q[2].y - Q[0].x*Q[0].y + (Q[1].x*Q[1].y - Q[0].x*Q[0].y)*(Q[2].y - Q[0].y)/(Q[0].y - Q[1].y);
        float by = x*y - Q[0].x*Q[0].y + (Q[3].x*Q[3].y - Q[0].x*Q[0].y)*(x - Q[0].x)/(Q[0].x - Q[3].x);
        float b1y = Q[1].x*Q[1].y - Q[0].x*Q[0].y + (Q[3].x*Q[3].y - Q[0].x*Q[0].y)*(Q[1].x - Q[0].x)/(Q[0].x - Q[3].x);
        float b2y = Q[2].x*Q[2].y - Q[0].x*Q[0].y + (Q[3].x*Q[3].y - Q[0].x*Q[0].y)*(Q[2].x - Q[0].x)/(Q[0].x - Q[3].x);

        float[] L_Out = new float[2];
        L_Out[0] = (ax/a3x) + (1 - a2x/a3x)*(bx - b3x*ax/a3x)/(b2x - b3x*a2x/a3x);
        L_Out[1] = (ay/a1y) + (1 - a2y/a1y)*(by - b1y*ay/a1y)/(b2y - b1y*a2y/a1y);

        return L_Out;
    }

    private void ResetTrajectory()
    {
        _speed = new Vector3(0, 0, 0);
        _trajectory.Clear();
        _lastDist = 500;
        _depthImpactLin = Vector3.zero;
        _colorImpactLin = Vector3.zero;
    }

    private void AnalyseTrajectory()
    {
        // update count of frame between throw
        _nbFrameBetweenThrow++;

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
            // Blob found
            _framesWithoutBlob = 0;

            int x = (int) biggestBlob.CenterOfGravity.X;
            int y = (int) biggestBlob.CenterOfGravity.Y;

            int maxZ = 500;
            int radius = 1;

            // Manage side of image
            int minI = x - radius >= 0 ? x - radius : 0;
            int maxI = x + radius < _resizedZBuffer.Width ? x + radius : _resizedZBuffer.Width;
            int minJ = y - radius >= 0 ? y - radius : 0;
            int maxJ = y + radius < _resizedZBuffer.Height ? y + radius : _resizedZBuffer.Height;

            // Get the max Z value in a neighborhood around center of ball
            for (int i = minI; i < maxI; i++)
            {
                for (int j = minJ; j < maxJ; j++)
                {
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
                if (_trajectory.Count > 0)
                {
                    var newPoint = new Vector3(x/ZBufferScale, y/ZBufferScale, maxZ); //full depth rez 512x424
                    _speed = (newPoint - _trajectory.Last())/(_framesWithoutBlob + 1);
                    _trajectory.Add(newPoint);
                    _lastDist = maxZ;
                }
                else
                {
                    // Add the point
                    _trajectory.Add(new Vector3(x/ZBufferScale, y/ZBufferScale, maxZ)); //full depth rez 512x424
                    _lastDist = maxZ;
                }
            }
        }
        else
        {
            if (_colorImpactLin.magnitude > 1e-5)
            {
                float[] points = Projection2Square(ScreenCorners, _colorImpactLin[0] / 1920.0f,
                    (1080 - _colorImpactLin[1]) / 1080.0f);

                float xNormalized = 1.0f - points[0];
                float yNormalized = points[1];

                if (xNormalized >= 0.0f && xNormalized <= 1.0f &&
                    yNormalized >= 0.0f && yNormalized <= 1.0f)
                {
                    if (_nbFrameBetweenThrow > 10)
                    {
                        ImpactPointDetected(xNormalized * Screen.width, yNormalized * Screen.height);
                    }
                    _nbFrameBetweenThrow = 0;

                    if (DebugManager.Debug)
                    {
                        Debug.Log("impact : " + _depthImpactLin);
                        Debug.Log("speed : " + _speed);
                    }
                }
                ResetTrajectory();
            }
            // No ball detected
            _framesWithoutBlob++;
            if (_framesWithoutBlob >= FramesWithoutBlobNeededToClear)
            {
                ResetTrajectory();
            }
        }
    }

    private void ComputeImpactPoint()
    {
        if (_trajectory.Count >= MinPointRequired)
        {
            // Compute impact point with linear eqtn
            int linZ = _projectionDistance;
            var deltaZ = _projectionDistance - _trajectory.Last()[2];
            var _normzSpeed = _speed/_speed[2];
            int linX = (int)(deltaZ * _normzSpeed[0] + _trajectory.Last()[0]);
            int linY = (int)(deltaZ * _normzSpeed[1] + _trajectory.Last()[1]);

            if (linX >= 0 && linX < 512 &&
                linY >= 0 && linY < 424)
            {
                _depthImpactLin = new Vector3(linX, linY, linZ);
                _colorImpactLin = DepthPoint2ColorPoint(_depthImpactLin);
            }

            // If we should hit the wall at next frame
            if (_lastDist + _speed[2] > _projectionDistance - 100)
            {
                float[] points = Projection2Square(ScreenCorners, _colorImpactLin[0] / 1920.0f,
                    (1080 - _colorImpactLin[1]) / 1080.0f);

                float xNormalized = 1.0f - points[0];
                float yNormalized = points[1];

                if (xNormalized >= 0.0f && xNormalized <= 1.0f &&
                    yNormalized >= 0.0f && yNormalized <= 1.0f)
                {
                    if (_nbFrameBetweenThrow > 10)
                    {
                        ImpactPointDetected(xNormalized * Screen.width, yNormalized * Screen.height);
                    }
                    _nbFrameBetweenThrow = 0;


                    if (DebugManager.Debug)
                    {
                        Debug.Log("impact : " + _depthImpactLin);
                        Debug.Log("speed : " + _speed);
                    }
                }
                ResetTrajectory();
            }
        }
    }

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
            var colorPos = DepthPoint2ColorPoint(new Vector3(pos.x, pos.y, pos.z));
            colorTrajectory.Add(colorPos);
        }

        return colorTrajectory;
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

    public Vector3 GetColorImpactPoint_lin()
    {
        return _colorImpactLin;
    }

    public Vector3 GetDepthImpactPoint_lin()
    {
        return _depthImpactLin;
    }

    #endregion
}