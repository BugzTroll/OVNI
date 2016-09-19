using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

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
	    if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "Clickable")
                {
                    //Destroy(gameObject);
                    
                    LoadLevel();
                }
                    
            }
        }
	}

    void LoadLevel()
    {
        StartOptions opt = ui.GetComponent<StartOptions>();
        opt.sceneToStart = 2;
        opt.Invoke("StartButtonClicked", 0);

        // temp
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        
    }
}
