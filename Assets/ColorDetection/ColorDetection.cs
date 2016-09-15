using UnityEngine;
using System.Collections;

public class ColorDetection : MonoBehaviour
{
    public GameObject ColorSourceManager;
    private MyColorSourceManager _ColorManager;

    // Use this for initialization
    void Start()
    {
        _ColorManager = ColorSourceManager.GetComponent<MyColorSourceManager>();
    }

    // Update is called once per frame
    void Update()
    {
        _ColorManager.DetectProjectile();
    }
}