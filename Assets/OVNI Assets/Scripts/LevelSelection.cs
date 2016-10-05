using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour {

    public GameObject Shadow_planete2;
    public GameObject Sphere_planete2;

    public GameObject Shadow_planete3;
    public GameObject Sphere_planete3;

    public GameObject Shadow_planete4;
    public GameObject Sphere_planete4;

    public GameObject Shadow_planete5;
    public GameObject Sphere_planete5;

    // Use this for initialization
    void Start ()
    {
        updatePlanetUI();
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
                    if (hit.collider.name == "Level1SelectSphere")
                    {
                        // change to scene associated to planet
                        GameManager.Instance.ChangeScene("Planete1");                      
                    }
                    if (hit.collider.name == "Level2SelectSphere")
                    {
                        // change to scene associated to planet
                        GameManager.Instance.ChangeScene("Planete2");
                    }
                    if (hit.collider.name == "Level3SelectSphere")
                    {
                        // change to scene associated to planet
                        GameManager.Instance.ChangeScene("Planete3");
                    }
                    if (hit.collider.name == "Level4SelectSphere")
                    {
                        // change to scene associated to planet
                        GameManager.Instance.ChangeScene("Planete4");
                    }
                    if (hit.collider.name == "Level5SelectSphere")
                    {
                        // change to scene associated to planet
                        GameManager.Instance.ChangeScene("Planete5");
                    }
                }
                    
            }
        }
	}

    void updatePlanetUI()
    {
        if (GameManager.Instance.LevelProgression.Contains(GameManager.GameLevel.Planete1))
        {
            Shadow_planete2.SetActive(false);
            Sphere_planete2.tag = "Clickable";

            Shadow_planete3.SetActive(false);
            Sphere_planete3.tag = "Clickable";
        }

        if (GameManager.Instance.LevelProgression.Contains(GameManager.GameLevel.Planete2) && GameManager.Instance.LevelProgression.Contains(GameManager.GameLevel.Planete3))
        {
            Shadow_planete4.SetActive(false);
            Sphere_planete4.tag = "Clickable";
        }
    }
}
