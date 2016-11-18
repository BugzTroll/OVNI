using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Events;

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
        InGame,
        LevelSelect,
        Quit,
        GameSuccess,
        GameOver
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

    public void ChangeScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        _currentScene = SceneManager.GetActiveScene();
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        _currentScene = SceneManager.GetActiveScene();
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

    private void OnImpactPointDetected(float x, float y)
    {
        if (DebugManager.Debug)
        {
            Debug.Log("point d'impact ! " + x + "," + y);
        }
        HandleInputPoint(x, y);
    }

    private void OnClickDetected(float x, float y)
    {
        if (DebugManager.Debug)
        {
            Debug.Log("point de click ! " + x + "," + y);
        }
        HandleInputPoint(x, y);
    }

    // x and y in normalized screen space 
    private void HandleInputPoint(float x, float y)
    {
        switch (_currentState)
        {
            case GameState.InGame:
            {
                GameObject projectileShooterObject = GameObject.Find("PlayerController");
                if (projectileShooterObject != null) 
                    projectileShooterObject.GetComponent<ProjectileShooter>().ShootProjectile(new Vector2(x, y));        
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
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.tag == "Clickable")
                    {
                            // This works only if the name of the objet is the same as the corresponding scene

                            //Instance.ChangeScene(hit.collider.name);
                            GameObject.Find("Fading").GetComponent<Fader>().EndScene(hit.collider.name);
                    }
                }
                break;
            }
        }
    }

    private void OnSceneChanged (Scene oldScene, Scene newScene)
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