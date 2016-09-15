using UnityEngine;
using System.Collections;

public class MyDepthView : MonoBehaviour
{
    public GameObject DepthManager;
    private MyDepthManager _DepthManager;

    void Start()
    {
        gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
    }

    void Update()
    {
        if (DepthManager == null)
        {
            return;
        }

        _DepthManager = DepthManager.GetComponent<MyDepthManager>();
        if (_DepthManager == null)
        {
            return;
        }

        gameObject.GetComponent<Renderer>().material.mainTexture = _DepthManager.GetDepthTexture();
    }
}