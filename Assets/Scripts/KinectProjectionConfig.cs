using System;
using UnityEngine;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using Windows.Kinect;
using AForge.Imaging;
using AForge.Imaging.Filters;
using Color = Microsoft.Kinect.Face.Color;

public class KinectProjectionConfig : MonoBehaviour
{
    public GameObject ColorManager;
    public GameObject BlobTracker;
    public GameObject ViewManager;
    public bool AlienDetection;

    private ColorSourceManager _colorManager;
    private ViewManager _viewManager;
    private BlobTracker _blobTracker;
    private int _clickCounter;
    private Bitmap _source;
    private Bitmap _template;
    private Bitmap bmp;
    private Texture _texture;
    private int cpt;

    // Use this for initialization
    void Start()
    {
        _clickCounter = 0;
        _colorManager = ColorManager.GetComponent<ColorSourceManager>();
        _blobTracker = BlobTracker.GetComponent<BlobTracker>();
        _viewManager = ViewManager.GetComponent<ViewManager>();

        Bitmap template =
            (Bitmap)
            Bitmap.FromFile(
                "C:\\Users\\nadm2208\\Documents\\OVNI\\Assets\\Resources\\Alien.jpg");

        var resizeFilter = new ResizeNearestNeighbor((int) (0.18*template.Width),
            (int) (0.16*template.Height));

        _template = resizeFilter.Apply(template);
        if (AlienDetection)
        {
            _texture = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            _texture = Resources.Load("OVNI") as Texture;
        }
        else
        {
            _texture = _viewManager.GetTexture();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (AlienDetection)
        {
            cpt++;
            if (cpt > 60)
            {
                var source = Converter.ByteArray2Bmp(_colorManager.GetData(), _colorManager.GetDescriptor().Width,
                    _colorManager.GetDescriptor().Height, PixelFormat.Format32bppArgb);

                var colorFrameDescriptor = _colorManager.GetDescriptor();
                var resizeFilter = new ResizeBilinear((int) (0.3*colorFrameDescriptor.Width),
                    (int) (0.3*colorFrameDescriptor.Height));

                _source = resizeFilter.Apply(source);

                var grey = new Grayscale(0.2125, 0.7154, 0.0721);

                var greySource = grey.Apply(_source);
                greySource.Save("C:\\Users\\nadm2208\\Desktop\\Alien1.bmp");

                var thres = new Threshold(220);
                var temp = thres.Apply(greySource);
                temp.Save("C:\\Users\\nadm2208\\Desktop\\Alien2.bmp");

                ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.75f);

                var greytemplate = grey.Apply(_template);
                greytemplate.Save("C:\\Users\\nadm2208\\Desktop\\Alien3.bmp");

                // image formate trouble -> ony 24bpp
                TemplateMatch[] matchings = tm.ProcessImage(temp, greytemplate);

                BitmapData data = _source.LockBits(
                    new Rectangle(0, 0, _source.Width, _source.Height),
                    ImageLockMode.ReadWrite, _source.PixelFormat);

                foreach (TemplateMatch m in matchings)
                {
                    Drawing.Rectangle(data, m.Rectangle, System.Drawing.Color.Tomato);
                }

                _source.UnlockBits(data);
                _source.Save("C:\\Users\\nadm2208\\Desktop\\AlienDetecter.bmp");
                GameManager.Instance.CurrentState = GameManager.GameState.MAIN_MENU;
            }
        }
        else
        {
            _texture = _viewManager.GetTexture();
        }
    }

    void OnGUI()
    {
        Rect rect = AlienDetection
            ? new Rect(0, 0, -1, 1)
            : new Rect(0, 0, 1, -1);
        GUI.DrawTextureWithTexCoords(new Rect(0, 0, Screen.width, Screen.height), _texture,
            rect);
    }

    void OnMouseDown()
    {
        if (_clickCounter <= 3)
        {
            _blobTracker.ScreenCorners[_clickCounter] = new Vector2(Input.mousePosition.x/Screen.width,
                Input.mousePosition.y/Screen.height);
        }
        if (_clickCounter == 3)
        {
            _blobTracker.InitProjectionDistance();
            GameManager.Instance.CurrentState = GameManager.GameState.MAIN_MENU;
        }
        _clickCounter++;
    }

}