using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class MyView : MonoBehaviour
{
    public GameObject ViewManager;
    private MyViewManager _viewManager;
    
    void Start ()
    {
        gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
    }
    
    void Update()
    {
        if (ViewManager == null)
        {
            return;
        }

        _viewManager = ViewManager.GetComponent<MyViewManager>();
        if (_viewManager == null)
        {
            return;
        }
        var dbg = _viewManager.GetTexture();
        gameObject.GetComponent<Renderer>().material.mainTexture = _viewManager.GetTexture();
    }
}
