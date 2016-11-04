using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour
{
    public static event UnityAction<float, float> ClickDetected;



    // Use this for initialization
    void Start ()
    {
        GameManager.Instance.CurrentState = GameManager.GameState.LevelSelect;
        Time.timeScale = 1.0f;

        if (GameManager.Instance.LevelProgression.Count > 0)
        {
            GameManager.GameLevel lastLevelDone = GameManager.Instance.LevelProgression.Last();
            Debug.Log("Last level done: " + lastLevelDone.ToString());

            switch(lastLevelDone)
            {
                case GameManager.GameLevel.Planete1:
                    // show level 2 and 3 by moving camera, etc.
                    break;

                case GameManager.GameLevel.Planete2:
                    // check if 3 was done previously
                    // update scene to show that planet 2 has been done and 3 still needs to be done (or not)

                    break;

                case GameManager.GameLevel.Planete3:
                    // check if 2 was done previously
                    // update scene to show that planet 3 has been done and 2 still needs to be done (or not)

                    break;

                case GameManager.GameLevel.Planete4:
                    // CONGRATS ! do stuff for winning (cool unlocks ? choose order ?)
                    break;
            }
        }
            
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
}
