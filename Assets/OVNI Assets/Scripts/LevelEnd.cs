using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LevelEnd : MonoBehaviour {


    private ProjectileShooter shooter;
    private GameLevelController gameLvlController;

    public GameObject levelEndPanel;                         //Store a reference to the Game Object pausePanel 
    public GameObject optionsTint;
    public GameObject pauseBtn;
    public GameObject RetryBtn;
    public GameObject LvlSelect;
    public UnityEngine.UI.Text PanelText;



    // Use this for initialization
    void Start ()
    {
        GameObject projectileShooterObject = GameObject.Find("Player");
        if (projectileShooterObject != null)
        {
            shooter = projectileShooterObject.GetComponent<ProjectileShooter>();
        }

        GameObject GameLevelControllerObject = GameObject.Find("GameLevelController");
        if (projectileShooterObject != null)
        {
            gameLvlController = GameLevelControllerObject.GetComponent<GameLevelController>();
        }
    }

    // Update is called once per frame
    void Update ()
    {
        CheckAmmoCount();
        CheckPointCount();

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
    void CheckPointCount()
    {

       if (gameLvlController.score > gameLvlController.scoreToWin)
        {
            levelEndPanel.SetActive(true);
            optionsTint.SetActive(true);
            pauseBtn.SetActive(false);
            RetryBtn.SetActive(false);
            LvlSelect.SetActive(true);
            PanelText.text = " You have passed the level, Congratulasion ! ";
        }

    }
}
