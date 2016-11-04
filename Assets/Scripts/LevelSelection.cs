using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour
{
    public static event UnityAction<float, float> ClickDetected;

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
        GameManager.Instance.CurrentState = GameManager.GameState.LevelSelect;
        Time.timeScale = 1;
        updatePlanetUI();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;
            if (ClickDetected != null)
                ClickDetected(Input.mousePosition.x, Input.mousePosition.y);
        }
    }

    void updatePlanetUI()
    {
        //if (GameManager.Instance.LevelProgression.Contains(GameManager.GameLevel.Planete1))
        //{
        //    Shadow_planete2.SetActive(false);
        //    Sphere_planete2.tag = "Clickable";

        //    Shadow_planete3.SetActive(false);
        //    Sphere_planete3.tag = "Clickable";
        //}

        //if (GameManager.Instance.LevelProgression.Contains(GameManager.GameLevel.Planete2) && GameManager.Instance.LevelProgression.Contains(GameManager.GameLevel.Planete3))
        //{
        //    Shadow_planete4.SetActive(false);
        //    Sphere_planete4.tag = "Clickable";
        //}
    }
}
