using UnityEngine;
using System.Collections;

public class LevelEnd : MonoBehaviour {


    private ProjectileShooter shooter;

    public GameObject levelEndPanel;                         //Store a reference to the Game Object pausePanel 
    public GameObject optionsTint;

    // Use this for initialization
    void Start ()
    {
        GameObject projectileShooterObject = GameObject.Find("Player");
        if (projectileShooterObject != null)
        {
            shooter = projectileShooterObject.GetComponent<ProjectileShooter>();
        }
        if (shooter == null)
        {
            Debug.Log("Cannot find 'ProjectileShooter' script");
        }

    }

    // Update is called once per frame
    void Update ()
    {
        CheckAmmoCount();

    }
    public void RetryButtonClicked()
    {
        // Able Panel  + OptionTint when condition is met
           
     

    }
    public void ReturnButtonClicked()
    {



    }
    void CheckAmmoCount()
    {
        if (shooter.tomatoCount + shooter.bombCount == 0)
        {
            levelEndPanel.SetActive(true);
            optionsTint.SetActive(true);
        }
      
    }
}
