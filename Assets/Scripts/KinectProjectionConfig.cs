using UnityEngine;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using AForge.Imaging;

public class KinectProjectionConfig : MonoBehaviour
{
    public GameObject ColorManager;
    public GameObject BlobTracker;

    private ColorSourceManager _colorManager;
    private BlobTracker _blobTracker;
    private int _clickCounter;
    private Bitmap _source;
    private Bitmap _template;
    private Bitmap bmp;
    private Texture _texture;

    // Use this for initialization
    void Start()
    {
        _clickCounter = 0;
        _colorManager = ColorManager.GetComponent<ColorSourceManager>();
        _blobTracker = BlobTracker.GetComponent<BlobTracker>();
        _template = (Bitmap) Bitmap.FromFile( "C:\\Users\\nadm2208\\Documents\\OVNI\\Assets\\Resources\\4f89eb55d2007a0f1e84553ff5105411.jpg");

        _texture = new Texture2D(1920,1080, TextureFormat.RGB24, false);
        _texture = Resources.Load("OVNI") as Texture;
    }

    // Update is called once per frame
    void Update()
    {
        _source = Converter.ByteArray2Bmp(_colorManager.GetData(), _colorManager.GetDescriptor().Width, 
        _colorManager.GetDescriptor().Height, PixelFormat.Format32bppArgb);
        ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.921f);

        // image formate trouble -> ony 24bpp
       // TemplateMatch[] matchings = tm.ProcessImage(_source, _template);
    }

    void OnGUI()
    {
        GUI.DrawTextureWithTexCoords(new Rect(0, 0, Screen.width, Screen.height), _texture,
            new Rect(0, 0, -1, 1));
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