using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour
{

    private enum CameraProgress
    {
        Planet1,
        Planets2and3,
        Planet4
    }

    public static event UnityAction<float, float> ClickDetected;

    private Animator _cameraAnimator;

    // private  CameraProgress _cameraProgress;
    private Transform lastZone;

    
    // Use this for initialization
    void Start ()
    {
        GameManager.Instance.CurrentState = GameManager.GameState.LevelSelect;
        Time.timeScale = 1.0f;

        _cameraAnimator = GameObject.Find("LevelSelectCamera").GetComponent<Animator>();
        _cameraAnimator.enabled = true;

        //_cameraProgress = CameraProgress.Planet1;

        //if (GameManager.Instance.LevelProgression.Count > 0)
        //{
        GameManager.GameLevel lastLevelDone = GameManager.GameLevel.None;

        if (GameManager.Instance.LevelProgression.Count > 0)
            lastLevelDone = GameManager.Instance.LevelProgression.Last();


        Debug.Log("Last level done: " + lastLevelDone.ToString());

        switch(lastLevelDone)
        {
            case GameManager.GameLevel.Planete1:
                _cameraAnimator.SetTrigger("EnteringPlanet1");
                MoveToPlanets2And3();
                // show level 2 and 3 by moving camera, etc.
                break;

            case GameManager.GameLevel.Planete2:
                _cameraAnimator.SetTrigger("EnteringPlanets2&3");
                MoveToPlanet4();
                // check if 3 was done previously
                // update scene to show that planet 2 has been done and 3 still needs to be done (or not)

                break;

            case GameManager.GameLevel.Planete3:
                _cameraAnimator.SetTrigger("EnteringPlanets2&3");
                MoveToPlanet4();
                // check if 2 was done previously
                // update scene to show that planet 3 has been done and 2 still needs to be done (or not)
                break;

            case GameManager.GameLevel.Planete4:
                _cameraAnimator.SetTrigger("EnteringPlanet4");
                // CONGRATS ! do stuff for winning (cool unlocks ? choose order ?)
                break;

        default:
            _cameraAnimator.SetTrigger("EnteringPlanet1");
            break;
        }
            
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (_cameraAnimator.enabled)
        {
            if (_cameraAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !_cameraAnimator.IsInTransition(0))
            {
                _cameraAnimator.enabled = false;
            }

        }

        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;
            if (ClickDetected != null)
                ClickDetected(Input.mousePosition.x, Input.mousePosition.y);
        }


    }

    private void MoveToPlanets2And3()
    {
        _cameraAnimator.SetTrigger("ToPlanets2&3");
        //_cameraProgress = CameraProgress.Planets2and3;
    }

    private void MoveToPlanet4()
    {
        _cameraAnimator.SetTrigger("ToPlanet4");
        //_cameraProgress = CameraProgress.Planet4;
    }

}
