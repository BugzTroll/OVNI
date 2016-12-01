
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class KinectProjectionConfig : MonoBehaviour
{
    public GameObject ColorManager;
    public GameObject BlobTracker;
    public GameObject ViewManager;

    private ColorSourceManager _colorManager;
    private ViewManager _viewManager;
    private BlobTracker _blobTracker;
    private int _clickCounter;
    private Texture2D _texture;
    private List<Vector2> _cornerPosition;
    private string[] _labels;
    private bool _depthConfigDone;


    // Use this for initialization
    void Start()
    {
        _clickCounter = 0;
        _colorManager = ColorManager.GetComponent<ColorSourceManager>();
        _blobTracker = BlobTracker.GetComponent<BlobTracker>();
        _viewManager = ViewManager.GetComponent<ViewManager>();
        _cornerPosition = new List<Vector2>();
        _depthConfigDone = false;
        _labels = new string[] {"Cliquer sur le coin en bas à gauche de la projection",
            "Cliquer sur le coin en haut à gauche de la projection",
            "Cliquer sur le coin en haut à droite de la projection",
            "Cliquer sur le coin en bas à droite de la projection"};

        _texture = _viewManager.GetTexture();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_depthConfigDone)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (_clickCounter <= 3)
                {
                    _blobTracker.ScreenCorners[_clickCounter] = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
                    _cornerPosition.Add(new Vector2((Input.mousePosition.x / Screen.width) * _texture.width, (1 - Input.mousePosition.y / Screen.height) * _texture.height));
                }
                if (_clickCounter == 3)
                {
                    _blobTracker.InitProjectionDistance();
                    _viewManager.ShowThresholdedZBuffer = true;
                    _viewManager.ShowPositions = true;
                    _depthConfigDone = true;
                }
                _clickCounter++;
            }
            else if (Input.GetMouseButtonDown(1) && _cornerPosition.Count > 0)
            {
                _cornerPosition.Remove(_cornerPosition.Last());
                _clickCounter--;
            }
        }
        else
        {
            if (Input.GetKeyDown("down"))
            {
                if(_blobTracker.ThresholdBlob > 1)
                {
                    _blobTracker.ThresholdBlob--;
                }
            }
            if (Input.GetKeyDown("up"))
            {
                if (_blobTracker.ThresholdBlob < 255)
                {
                    _blobTracker.ThresholdBlob++;
                }
            }
            if (Input.GetKeyDown("return") || Input.GetMouseButtonDown(0))
            {
                _blobTracker.ActivateThrowing();
                _viewManager.ShowThresholdedZBuffer = false;
                _viewManager.ShowPositions = false;
                GameManager.Instance.CurrentState = GameManager.GameState.Animation;
            }
        }


        _texture = _viewManager.GetTexture();

        if (!_depthConfigDone)
        {
            foreach (Vector2 corner in _cornerPosition)
            {
                _texture.SetPixel((int)corner.x - 1, (int)corner.y, UnityEngine.Color.red);
                _texture.SetPixel((int)corner.x, (int)corner.y - 1, UnityEngine.Color.red);
                _texture.SetPixel((int)corner.x, (int)corner.y, UnityEngine.Color.red);
                _texture.SetPixel((int)corner.x + 1, (int)corner.y, UnityEngine.Color.red);
                _texture.SetPixel((int)corner.x, (int)corner.y + 1, UnityEngine.Color.red);
            }
        }
        _texture.Apply();
    }

    void OnGUI()
    {
        GUI.DrawTextureWithTexCoords(new Rect(0, 0, Screen.width, Screen.height), _texture,
            new Rect(0, 0, 1, -1));

        if (!_depthConfigDone)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 30;
            style.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), _labels[_clickCounter], style);
        }

        if (_depthConfigDone)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 30;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;

            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Ajuster le seuil : " + _blobTracker.ThresholdBlob.ToString() + "\n flèche 'bas' pour diminuer\n flèche 'haut' pour augmenter\nEnter pour commencer", style);
        }
    }
}