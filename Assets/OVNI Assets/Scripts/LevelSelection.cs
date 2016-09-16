using UnityEngine;
using System.Collections;

public class LevelSelection : MonoBehaviour {

    private GameObject ui;
	// Use this for initialization
	void Start ()
    {
        ui = GameObject.Find("UI");
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    void onMouseDown()
    {
        StartOptions opt = ui.GetComponent<StartOptions>();
        opt.sceneToStart = 2;
        opt.Invoke("LoadDelayed", 3);
        Destroy(gameObject);
    }
}
