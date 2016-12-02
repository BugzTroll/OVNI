using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using System;

public class GameManager
{
    public static readonly GameManager Instance = new GameManager();
    public static event UnityAction<GameState> GameStateUpdated;

    public enum GameState
    {
        None = -1,
        MainMenu,
        OptionsMenu,
        Paused,
        PopUp,
        InGame,
        LevelSelect,
        Quit,
        GameSuccess,
        GameOver,
        Animation
    }
    public enum GameLevel
    {
        None = 0,
        Planete1 = 1,
        Planete2 = 2,
        Planete3 = 3,
        Planete4 = 4,
        Planete5 = 5
    }

    public List<GameLevel> LevelProgression = new List<GameLevel>();
    private GameState _currentState;
    private Scene _currentScene;
    public List<int> visitedPlanet = new List<int>();
    public GameObject prefabAlien;

    public void ChangeScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        _currentScene = SceneManager.GetActiveScene();
        if (!visitedPlanet.Contains(sceneIndex))
        {
            visitedPlanet.Add(sceneIndex);
        }
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        _currentScene = SceneManager.GetActiveScene();
        if (!visitedPlanet.Contains(_currentScene.buildIndex))
        {
            visitedPlanet.Add(_currentScene.buildIndex);
        }
    }

    public void RestartScene()
    {
        Time.timeScale = 1.0f;
        GameObject.Find("Fading").GetComponent<Fader>().EndScene(SceneManager.GetActiveScene().name);
        // ChangeScene(SceneManager.GetActiveScene().name);
    }

    public void UpdateProgression(Scene level)
    {
        GameLevel latestLevelDone = GameLevel.None;
        switch (level.name)
        {
            case "Planete1":
                latestLevelDone = GameLevel.Planete1;
                break;
            case "Planete2":
                latestLevelDone = GameLevel.Planete2;
                break;
            case "Planete3":
                latestLevelDone = GameLevel.Planete3;
                break;
            case "Planete4":
                latestLevelDone = GameLevel.Planete4;
                break;
        }
        if (!LevelProgression.Contains(latestLevelDone) && latestLevelDone != GameLevel.None)
        {
            LevelProgression.Add(latestLevelDone);
        }


    }

    public GameState CurrentState
    {
        get { return _currentState; }
        set
        {
            switch (value)
            {
                case GameState.Animation:
                    {
                        ChangeScene("animationScene");
                        break;
                    }
                case GameState.MainMenu:
                    {
                        ChangeScene("MainMenu");
                        break;
                    }
                case GameState.GameSuccess:
                    {
                        if (_currentState == GameState.GameOver)
                            value = _currentState;
                        break;
                    }
                case GameState.GameOver:
                    {
                        if (_currentState == GameState.GameSuccess)
                            value = _currentState;
                        break;
                    }
                case GameState.Quit:
                    {
                        Quit();
                        break;
                    }
            }

            if (value != _currentState && GameStateUpdated != null)
            {
                GameStateUpdated(value);
            }
            _currentState = value;

            if (DebugManager.Debug)
                Debug.Log("Game State changed to: " + _currentState.ToString());
        }
    }

    // Made constructor private, so it can only be instantiated here
    private GameManager()
    {
        _currentState = GameState.None;
        _currentScene = SceneManager.GetSceneAt(0);
        SceneManager.activeSceneChanged += OnSceneChanged;
        BlobTracker.ImpactPointDetected += OnImpactPointDetected;
        ProjectileShooter.ClickDetected += OnClickDetected;
        LevelSelection.ClickDetected += OnClickDetected;

        if (DebugManager.Debug)
            Debug.Log("GameManager was created");
    }

    ~GameManager()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
        BlobTracker.ImpactPointDetected -= OnImpactPointDetected;
        ProjectileShooter.ClickDetected -= OnClickDetected;
        LevelSelection.ClickDetected -= OnClickDetected;
    }

    private void OnImpactPointDetected(float x, float y, float speed)
    {
        if (DebugManager.Debug)
        {
            Debug.Log("point d'impact ! " + x + "," + y);
        }
        HandleInputPoint(x, y, speed);
    }

    private void OnClickDetected(float x, float y)
    {
        if (DebugManager.Debug)
        {
            Debug.Log("point de click ! " + x + "," + y);
        }
        HandleInputPoint(x, y, 30.0f);
    }

    // x and y in normalized screen space 
    private void HandleInputPoint(float x, float y, float speed = 10.0f)
    {
        switch (_currentState)
        {
            case GameState.InGame:
                {
                    GameObject projectileShooterObject = GameObject.Find("PlayerController");
                    if (projectileShooterObject != null)
                        projectileShooterObject.GetComponent<ProjectileShooter>().ShootProjectile(new Vector2(x, y), speed);
                    break;
                }
            case GameState.GameOver:
                {
                    GameObject gameCont = GameObject.Find("GameLevelController");
                    if (gameCont)
                        gameCont.GetComponent<GameLevelController>().RetryLevel();
                    break;
                }
            case GameState.GameSuccess:
                {
                    GameObject gameCont = GameObject.Find("GameLevelController");
                    if (gameCont)
                        gameCont.GetComponent<GameLevelController>().ReturnToLevelSelection();
                    break;
                }
            case GameState.LevelSelect:
                {
                    GameObject obj = GameObject.Find("LevelSelector");
                    var lvlselect = obj.GetComponent<LevelSelection>();
                    lvlselect.ShootProjectile(x, y);
                    break;
                }
            case GameState.PopUp:
                {
                    GameObject startPanel = GameObject.Find("StartLvlPanel");
                    GameObject tutorialPanel = GameObject.Find("TutorialPanel");

                    if (startPanel || tutorialPanel)
                    {
                        // deal with first scene ui
                        if (_currentScene.name == "Planete1")
                        {
                            GameObject ui = GameObject.Find("GameUI");
                            IntroLevelStart intro = ui.GetComponent<IntroLevelStart>();

                            if (intro.introPanel.activeInHierarchy)
                            {
                                intro.HideIntroPanel();
                                intro.ShowTutorialPanel();
                            }
                            else
                            {
                                intro.HideTutorialPanel();
                                Instance.CurrentState = GameState.InGame;
                            }
                            
                        }

                        // rest
                        else
                        {
                            startPanel.SetActive(false);
                            Instance.CurrentState = GameState.InGame;
                        }
                        
                    }

                    

                    break;
                }

        }
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        if (DebugManager.Debug)
            Debug.Log("New scene loaded: " + newScene.buildIndex + ", " + newScene.name);
    }

    private void Quit()
    {
        //If we are running in a standalone build of the game
#if UNITY_STANDALONE
        //Quit the application
        Application.Quit();
#endif

        //If we are running in the editor
#if UNITY_EDITOR
        //Stop playing the scene
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}