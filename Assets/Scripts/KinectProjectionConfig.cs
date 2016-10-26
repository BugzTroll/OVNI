using UnityEngine;
using System.Collections;

public class KinectProjectionConfig : MonoBehaviour
{
    public GameObject ViewManager;
    public GameObject BlobTracker;
    private ViewManager _viewManager;
    private BlobTracker _blobTracker;
    private Texture2D _backgroundTexture;
    private int _clickCounter;


    // Use this for initialization
    void Start()
    {
        _clickCounter = 0;
        _backgroundTexture = new Texture2D(Screen.width, Screen.height);
        _viewManager = ViewManager.GetComponent<ViewManager>();
        _blobTracker = BlobTracker.GetComponent<BlobTracker>();
        _backgroundTexture = new Texture2D(Screen.width, Screen.height);
    }

    // Update is called once per frame
    void Update()
    {
        _backgroundTexture = _viewManager.GetTexture();
    }

    void OnGUI()
    {
        GUI.DrawTextureWithTexCoords(new Rect(0, 0, Screen.width, Screen.height), _backgroundTexture,
            new Rect(0, 0, 1, -1));
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

            if (!DebugManager.Debug)
            {
                _viewManager.Shutdown();
            }
        }
        _clickCounter++;
    }
}