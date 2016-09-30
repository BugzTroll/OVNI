using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelEnd : MonoBehaviour {


    private ProjectileShooter shooter;

    public GameObject levelEndPanel;                         //Store a reference to the Game Object pausePanel 
    public GameObject optionsTint;
    public GameObject pauseBtn;

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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ReturnButtonClicked()
    {
        GameManager.Instance.ChangeScene("LevelSelect");
    }
    void CheckAmmoCount()
    {
        if (shooter.tomatoCount + shooter.bombCount == 0)
        {
            levelEndPanel.SetActive(true);
            optionsTint.SetActive(true);
            pauseBtn.SetActive(false);
        }
      
    }
}
