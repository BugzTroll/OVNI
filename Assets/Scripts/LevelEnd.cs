using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LevelEnd : MonoBehaviour
{
    public GameObject levelEndPanel;
    public GameObject optionsTint;
    public GameObject pauseBtn;
    public GameObject RetryBtn;
    [Range(0, 1)] public float endScreenSlowMoFactor;
    [Range(0, 30)] public float TimeToLose = 3.0f;

    private ProjectileShooter shooter;
    private GameLevelController gameLvlController;
    private float timer = 0.0f;

    // Use this for initialization
    void Start()
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
    void Update()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.IN_GAME)
        {
            CheckAmmoCount();
            CheckWinCondition();
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
                GameManager.Instance.ActionFromBallOrClick(Input.mousePosition.x, Input.mousePosition.y);
        }
    }
    public void RetryButtonClicked()
    {
        // enable Panel  + OptionTint when condition is met
        GameManager.Instance.ChangeScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnButtonClicked()
    {
        GameManager.Instance.ChangeScene("LevelSelect");
    }

    void CheckAmmoCount()
    {
        if (shooter.GetCurrentAmmoCount() == 0)
        {
            timer += Time.deltaTime;
        }
        if (timer > TimeToLose)
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

        GameObject retryText = levelEndPanel.transform.Find("retryText").gameObject;
        retryText.SetActive(true);
    }

    public void LevelSuccess()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.IN_GAME)
            return;

        GameManager.Instance.CurrentState = GameManager.GameState.GAME_SUCCESS;
        levelEndPanel.SetActive(true);
        //optionsTint.SetActive(true);
        pauseBtn.SetActive(false);
        Time.timeScale = endScreenSlowMoFactor;

        GameObject continueText = levelEndPanel.transform.Find("continueText").gameObject;
        continueText.SetActive(true);

        // Ajoute la planète a la liste de progression
        GameManager.Instance.UpdateProgression(SceneManager.GetActiveScene());
    }
}

