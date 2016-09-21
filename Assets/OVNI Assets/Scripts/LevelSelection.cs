using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour {

    // add game states

	// Use this for initialization
	void Start ()
    {
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
                    GameManager.ChangeScene("Level1Test");
                }
                    
            }
        }
	}
}
