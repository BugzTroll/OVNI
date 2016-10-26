using UnityEngine;

public class View : MonoBehaviour
{
    public GameObject ViewManager;
    private ViewManager _viewManager;
    private Texture2D _backgroundTexture;

    void Start()
    {
        if (!DebugManager.Debug)
        {
            this.gameObject.SetActive(false);
        }

        gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
    }

    void Update()
    {
        if (ViewManager == null)
        {
            return;
        }

        _viewManager = ViewManager.GetComponent<ViewManager>();
        if (_viewManager == null)
        {
            return;
        }
        gameObject.GetComponent<Renderer>().material.mainTexture = _viewManager.GetTexture();
    }
}