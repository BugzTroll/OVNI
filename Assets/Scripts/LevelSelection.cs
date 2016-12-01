using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour
{
    public static event UnityAction<float, float> ClickDetected;
    public int speed = 10;
    private Animator _cameraAnimator;
    public GameObject alienPrefrab;
    private Vector3 startPosition;

    // Use this for initialization
    void Start ()
    {
        GameManager.Instance.CurrentState = GameManager.GameState.LevelSelect;
        Time.timeScale = 1.0f;

        _cameraAnimator = GameObject.Find("LevelSelectCamera").GetComponent<Animator>();
        _cameraAnimator.enabled = true;

        GameManager.GameLevel lastLevelDone = GameManager.GameLevel.None;

        if (GameManager.Instance.LevelProgression.Count > 0)
            lastLevelDone = GameManager.Instance.LevelProgression.Last();

        if(DebugManager.Debug)
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
                if (GameManager.Instance.LevelProgression.Contains(GameManager.GameLevel.Planete3))
                {
                    MoveToPlanet4();
                }
                else
                {
                    GameObject.Find("Planete2").GetComponent<Renderer>().material.color = Color.black;
                    GameObject.Find("Planete2").GetComponent<Collider>().enabled = false;
                }
                    
                // check if 3 was done previously
                // update scene to show that planet 2 has been done and 3 still needs to be done (or not)

                break;

            case GameManager.GameLevel.Planete3:
                _cameraAnimator.SetTrigger("EnteringPlanets2&3");
                if (GameManager.Instance.LevelProgression.Contains(GameManager.GameLevel.Planete2))
                {
                    MoveToPlanet4();
                }
                else
                {
                    GameObject.Find("Planete3").GetComponent<Renderer>().material.color = Color.black;
                    GameObject.Find("Planete3").GetComponent<Collider>().enabled = false;
                }

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
            if (ClickDetected != null)
            {
                ClickDetected(Input.mousePosition.x, Input.mousePosition.y);
            }
        }


    }

    public void ShootProjectile( float x, float y)
    {
        Vector3 screenPosition = new Vector3(x, y, 1);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        Vector3 velocity = worldPosition - Camera.main.gameObject.transform.position;
        velocity.Normalize();

        GameObject projectile = Instantiate(alienPrefrab);

        // Initial Projectile Position
        projectile.transform.position = worldPosition;
        var angle = -Mathf.Acos(Vector3.Dot(velocity, Camera.main.transform.forward)) * Mathf.Rad2Deg;
        var axis = Vector3.Cross(velocity, Camera.main.transform.forward);
        Quaternion quat = Quaternion.AngleAxis(angle, axis);
        projectile.transform.rotation *= quat;

        //Velocity
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        // temp (will come from the "Speed" of the tracking)
        rb.velocity = velocity * speed;

    }

    private void MoveToPlanets2And3()
    {
        _cameraAnimator.SetTrigger("ToPlanets2&3");
    }

    private void MoveToPlanet4()
    {
        _cameraAnimator.SetTrigger("ToPlanet4");
    }

}
