using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneSkip : MonoBehaviour {

    public static event UnityAction<float, float> ClickDetected;

    // Use this for initialization
    void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
        {
            GameObject.Find("Fading").GetComponent<Fader>().EndScene("MainMenu");
        }
         
    }
}
