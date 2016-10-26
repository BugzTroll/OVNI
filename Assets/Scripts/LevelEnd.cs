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
    public float endScreenSlowMoFactor;     // 0.0 to 1.0


    // Use this for initialization
    void Start ()
    {
        GameManager.Instance.CurrentState = GameManager.GameState.IN_GAME;

        GameObject projectileShooterObject = GameObject.Find("PlayerController");
        if (projectileShooterObject != null)
        {
            shooter = projectileShooterObject.GetComponent<ProjectileShooter>();
        }

        GameObject GameLevelControllerObject = GameObject.Find("GameLevelController");
        if (GameLevelControllerObject != null)
        {
            gameLvlController = GameLevelControllerObject.GetComponent<GameLevelController>();
        }
    }

    // Update is called once per frame
    void Update ()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.IN_GAME)
        {
            CheckAmmoCount();
            CheckWinCondition();
        }

    }
    public void RetryButtonClicked()
    {
        // enable Panel  + OptionTint when condition is met
        GameManager.Instance.ChangeScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnButtonClicked()
    {
        GameManager.Instance.CurrentState = GameManager.GameState.LEVEL_SELECT;
    }

    void CheckAmmoCount()
    {
        if (shooter.tomatoCount + shooter.bombCount <= 0)
        {
            LevelFailed();
        }
      
    }
    void CheckWinCondition()
    {

        if (gameLvlController.score > gameLvlController.scoreToWin
            || gameLvlController.CheckIfAllTargetsDestroyed())
        {
            LevelSuccess();
        }
    }

    public void LevelFailed()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.IN_GAME)
            return;

        GameManager.Instance.CurrentState = GameManager.GameState.GAME_OVER;
        levelEndPanel.SetActive(true);
        optionsTint.SetActive(true);
        pauseBtn.SetActive(false);
        Time.timeScale = endScreenSlowMoFactor;
    }

    public void LevelSuccess()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.IN_GAME)
            return;

        GameManager.Instance.CurrentState = GameManager.GameState.GAME_SUCCESS;
        levelEndPanel.SetActive(true);
        optionsTint.SetActive(true);
        pauseBtn.SetActive(false);
        Time.timeScale = endScreenSlowMoFactor;

        PanelText.text = " Niveau Réussi, Félicitations !";

        // Ajoute la planète a la liste de progression
        GameManager.Instance.UpdateProgression(SceneManager.GetActiveScene());
    }
}

