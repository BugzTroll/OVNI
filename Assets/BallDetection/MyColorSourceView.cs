using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class MyColorSourceView : MonoBehaviour
{
    public GameObject ColorSourceManager;
    private MyColorSourceManager _ColorManager;
    
    void Start ()
    {
        gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
    }
    
    void Update()
    {
        if (ColorSourceManager == null)
        {
            return;
        }
        
        _ColorManager = ColorSourceManager.GetComponent<MyColorSourceManager>();
        if (_ColorManager == null)
        {
            return;
        }
        
        gameObject.GetComponent<Renderer>().material.mainTexture = _ColorManager.GetColorTexture();
    }
}
