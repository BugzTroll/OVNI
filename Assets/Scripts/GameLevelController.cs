﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameLevelController : MonoBehaviour
{
    public static event UnityAction<float> ScoreUpdated;

    public int ScoreToWin;
    public GameObject TargetObject = null;
    [Range(0, 30)] public float TimeToLose = 3.0f;
    [Range(0, 1)] public float EndScreenSlowMoFactor;

    private float _score;
    private float _timer = 0.0f;
    private ProjectileShooter _shooter;

    public void AddScore(int addScore)
    {
        _score += addScore;

        if (ScoreUpdated != null)
        {
            ScoreUpdated(_score);
        }
    }

    public void RetryLevel()
    {
        GameManager.Instance.RestartScene();
    }

    public void ReturnToLevelSelection()
    {
        Time.timeScale = 1.0f;
        GameObject.Find("Fading").GetComponent<Fader>().EndScene("LevelSelect");
        //GameManager.Instance.ChangeScene("LevelSelect");
    }

    private void Start ()
    {
        GameManager.Instance.CurrentState = GameManager.GameState.InGame;

        GameObject projectileShooterObject = GameObject.Find("PlayerController");
        if (projectileShooterObject != null)
        {
            _shooter = projectileShooterObject.GetComponent<ProjectileShooter>();
        }

        Time.timeScale = 1.0f;  // put it at 1 again in case it got slowed down during the fail/win screen !

        // Hacked Score
        if (TargetObject != null)
            ScoreToWin = 1000000000;

        _score = 0;
    }
	
	private void Update ()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.InGame)
        {
            CheckfailureCondition();
            CheckWinCondition();
        }
	}

    private void CheckfailureCondition()
    {
        if (_shooter.GetRemainingAmmoCount() == 0)
        {
            _timer += Time.deltaTime;
        }
        if (_timer > TimeToLose)
        {
            LevelFailed();
        }
    }

    private void CheckWinCondition()
    {
        if (_score > ScoreToWin || (TargetObject != null  && CheckIfAllTargetsDestroyed()))
        {
            LevelSuccess();
        }
    }

    private bool CheckIfAllTargetsDestroyed()
    {
        int remainingObjects = 0;
        foreach (Transform child in TargetObject.transform)
        {
            if (child.gameObject.tag == "Container")
            {
                remainingObjects += child.childCount;
            }

            else
                remainingObjects++;

            if (remainingObjects > 0)
                return false;

        }

        return true;
    }

    private void LevelFailed()
    {
        Debug.Assert(GameManager.Instance.CurrentState == GameManager.GameState.InGame);

        GameManager.Instance.CurrentState = GameManager.GameState.GameOver;
        Time.timeScale = EndScreenSlowMoFactor;
    }

    private void LevelSuccess()
    {
        Debug.Assert(GameManager.Instance.CurrentState == GameManager.GameState.InGame);

        GameManager.Instance.CurrentState = GameManager.GameState.GameSuccess;
        Time.timeScale = EndScreenSlowMoFactor;

        // Ajoute la planète a la liste de progression
        GameManager.Instance.UpdateProgression(SceneManager.GetActiveScene());
    }
}
