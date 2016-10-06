using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Windows.Kinect;
using AForge.Imaging;
using AForge.Imaging.Filters;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;

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

    private MyDepthSourceManager _depthManager;
    private Bitmap _resizedZBuffer;
    private Bitmap _thresholdedZBuffer;

    private MyColorSourceManager _colorManager;
    private Bitmap _resizedColor;

    private GrayscaleToRGB _grey2Rgb;
    private List<Vector3> _trajectory;
    private Blob[] _blobs;
    private double[] _poly;
    private float[] _lin;
    private float _lastDist;
    private int _framesWithoutBlob;

    public List<Vector3> GetDepthTrajectory()
    {
        return _trajectory;
    }

    public List<Vector3> GetColorTrajectory()
    {
        var sensor = KinectSensor.GetDefault();
        List<Vector3> colorTrajectory = new List<Vector3>();
        foreach (var pos in _trajectory)
        {
            DepthSpacePoint point = new DepthSpacePoint();
            point.X = (int) (pos[0]/ZBufferScale);
            point.Y = (int) (pos[1]/ZBufferScale);

            var z = _depthManager.GetRawZ((int) point.X, (int) point.Y);

            var colorSpacePoint = sensor.CoordinateMapper.MapDepthPointToColorSpace(point, z);
            colorSpacePoint.X *= ColorScale;
            colorSpacePoint.Y *= ColorScale;

            colorTrajectory.Add(new Vector3(colorSpacePoint.X, colorSpacePoint.Y, 0.0f));
        }

        return colorTrajectory;
    }

    public double[] GetPoly()
    {
        return _poly;
    }

    public float[] GetLinX()
    {
        return _lin;
    }

    public Bitmap GetResizedZBuffer()
    {
        return _grey2Rgb.Apply(_resizedZBuffer);
    }

    public Bitmap GetThresholdedZBuffer()
    {
        return _grey2Rgb.Apply(_thresholdedZBuffer);
    }

    public Bitmap GetResizedColor()
    {
        return _resizedColor;
    }

    void Start()
    {
        _grey2Rgb = new GrayscaleToRGB();
        _depthManager = DepthManager.GetComponent<MyDepthSourceManager>();
        _colorManager = ColorManager.GetComponent<MyColorSourceManager>();
        _blobs = new Blob[0];
        _trajectory = new List<Vector3>();
        _framesWithoutBlob = 0;
        _lin = null;
        _poly = null;
        _lastDist = 0;
    }

    void Update()
    {
        var depthFrameDescriptor = _depthManager.GetDescriptor();

        // Create Z-Buffer Bitmap From Depth Manager Data (16 bit/pixel greyscale)
        var zBuffer = MyConverter.ByteArray2Bmp(_depthManager.GetData(),
            depthFrameDescriptor.Width,
            depthFrameDescriptor.Height,
            PixelFormat.Format16bppGrayScale);

        // Resize Depth Buffer
        var depthResizeFilter = new ResizeNearestNeighbor((int) (ZBufferScale*depthFrameDescriptor.Width),
            (int) (ZBufferScale*depthFrameDescriptor.Height));

        zBuffer = depthResizeFilter.Apply(zBuffer);
        _resizedZBuffer = AForge.Imaging.Image.Convert16bppTo8bpp(zBuffer);

        // threshold Z-Buffer To Reduce Noise
        Threshold treshFilter = new Threshold(ThresholdBlob);
        _thresholdedZBuffer = treshFilter.Apply(_resizedZBuffer);

        // Blob Filtering
        var blobFilter = new BlobCounter();
        blobFilter.CoupledSizeFiltering = true;
        blobFilter.FilterBlobs = true;
        blobFilter.MinHeight = MinSizeBlob;
        blobFilter.MinWidth = MinSizeBlob;
        blobFilter.MaxHeight = MaxSizeBlob;
        blobFilter.MaxWidth = MaxSizeBlob;
        blobFilter.ProcessImage(_thresholdedZBuffer);

        // Fill Blob Array 
        _blobs = blobFilter.GetObjectsInformation();

        var biggestBlob = GetBiggestBlob();

        // Trajectory Analysis
        if (biggestBlob != null)
        {
            _framesWithoutBlob = 0;
            int x = (int) biggestBlob.CenterOfGravity.X;
            int y = (int) biggestBlob.CenterOfGravity.Y;
            int z = _depthManager.GetRawZ((int)(x/ZBufferScale), (int)(y/ZBufferScale));
            if (z > KinectSensor.GetDefault().DepthFrameSource.DepthMinReliableDistance &&
                z < KinectSensor.GetDefault().DepthFrameSource.DepthMaxReliableDistance &&
                z > _lastDist)
            {
                if (_trajectory.Count > 2 && _poly != null && _lin != null)
                {
                    int xcheck = (int)(_lin[0] * z + _lin[1]);
                    int ycheck = (int)(_poly[0] + _poly[1] * z + _poly[2] * z * z);
                    if (Math.Abs(x - xcheck) < ThresholdTrajectory && Math.Abs(y - ycheck) < ThresholdTrajectory)
                    {
                        _trajectory.Add(new Vector3(x, y, z));
                        _lastDist = z;
                    }
                }
                else if (_trajectory.Count < 3)
                {
                    _trajectory.Add(new Vector3(x, y, z));
                    _lastDist = z;
                }
            }

        }
        else
        {
            _framesWithoutBlob++;
            if (_framesWithoutBlob >= FramesWithoutBlobNeededToClear)
            {
                _trajectory.Clear();
                _lastDist = 0;
                _poly = null;
                _lin = null;
            }
        }

        if (_trajectory.Count > 2)
        {
            _lin = new float[2];
            _lin[0] = (_trajectory[0][0] - _trajectory.Last()[0])/(_trajectory[0][2] - _trajectory.Last()[2]);
            _lin[1] = _trajectory[0][0] - _lin[0]*_trajectory[0][2];

            // generate parametric data for the polynomial regression
            double[] zData = new double[_trajectory.Count];
            double[] yData = new double[_trajectory.Count];

            for (int i = 0; i < _trajectory.Count; i++)
            {
                var pos = _trajectory[i];
                zData[i] = pos[2];
                yData[i] = pos[1];
            }

            // find polynomial coefficients
            _poly = Fit.Polynomial(zData, yData, 2);
        }

        // Color Analysis
        var colorFrameDescriptor = _colorManager.GetDescriptor();

        var colorImg = MyConverter.ByteArray2Bmp(_colorManager.GetData(),
            colorFrameDescriptor.Width,
            colorFrameDescriptor.Height,
            PixelFormat.Format32bppArgb);

        var resizeFilter = new ResizeNearestNeighbor((int) (ColorScale*colorFrameDescriptor.Width),
            (int) (ColorScale*colorFrameDescriptor.Height));

        _resizedColor = resizeFilter.Apply(colorImg);
    }

    private Blob GetBiggestBlob()
    {
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
        return biggestBlob;
    }
}