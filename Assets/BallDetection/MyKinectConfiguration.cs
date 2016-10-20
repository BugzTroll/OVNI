using UnityEngine;

public class MyKinectConfiguration : MonoBehaviour
{
    public GameObject BlobTracker;
    private MyBlobTracker _blobTracker;
    private int _cpt;

    void Start()
    {
        _blobTracker = BlobTracker.GetComponent<MyBlobTracker>();
        _cpt = 0;
    }

    void OnMouseDown()
    {
        if (_cpt%2 == 0)
        {
            _blobTracker.LeftBotomScreen = new Vector2(Input.mousePosition.x/Screen.width,
                Input.mousePosition.y/Screen.height);
        }
        else
        {
            _blobTracker.RightTopScreen = new Vector2(Input.mousePosition.x/Screen.width,
                Input.mousePosition.y/Screen.height);
        }
        _cpt++;
    }
}