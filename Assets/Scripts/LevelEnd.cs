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
        CheckAmmoCount();
        CheckWinCondition();

    }
    public void RetryButtonClicked()
    {
        // enable Panel  + OptionTint when condition is met
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        GameManager.Instance.CurrentState = GameManager.GameState.IN_GAME;
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
    }

    public void LevelSuccess()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.IN_GAME)
            return;

        GameManager.Instance.CurrentState = GameManager.GameState.GAME_SUCCESS;
        levelEndPanel.SetActive(true);
        optionsTint.SetActive(true);
        pauseBtn.SetActive(false);

        PanelText.text = " Niveau Réussi, Félicitations !";

        // Ajoute la planète a la liste de progression
        GameManager.Instance.UpdateProgression(SceneManager.GetActiveScene());
    }
}

