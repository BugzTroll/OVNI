using UnityEngine;
using System.Collections;

public class KinectConfiguration : MonoBehaviour
{
    public GameObject BlobTracker;
    private MyBlobTracker _blobTracker;
    private int cpt;

    // Use this for initialization
    void Start()
    {
        _blobTracker = BlobTracker.GetComponent<MyBlobTracker>();
        cpt = 0;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnMouseDown()
    {
        if (cpt%2 == 0)
        {
            _blobTracker.LeftBotomScreen = new Vector2(Input.mousePosition.x/Screen.width, Input.mousePosition.y/Screen.height);
        }
        else
        {
            _blobTracker.RightTopScreen = new Vector2(Input.mousePosition.x/Screen.width, Input.mousePosition.y/Screen.height);
        }
        cpt++;
    }
}