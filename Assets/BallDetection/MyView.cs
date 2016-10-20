using UnityEngine;

public class MyView : MonoBehaviour
{
    public GameObject ViewManager;
    private MyViewManager _viewManager;
    private Texture2D _backgroundTexture;

    void Start()
    {
        //gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
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
        //gameObject.GetComponent<Renderer>().material.mainTexture = _viewManager.GetTexture();
        _backgroundTexture = _viewManager.GetTexture();
    }

    void OnGUI()
    {
        GUI.DrawTextureWithTexCoords(new Rect(0, 0, Screen.width, Screen.height), _backgroundTexture, new Rect(0,0,1,-1));
    }
}