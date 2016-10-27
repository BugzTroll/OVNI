using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager
{

    

    public enum GameState
    {
        NONE = -1,
        MAIN_MENU,
        OPTIONS_MENU,
        PAUSED,
        IN_GAME,
        LEVEL_SELECT,
        QUIT,
        GAME_SUCCESS,
        GAME_OVER
        //OTHERS
    }
    public enum GameLevel
    {
        Planete1 = 1,
        Planete2,
        Planete3,
        Planete4,
        Planete5,
        //OTHERS PLANETS
    }
    private static GameManager instance = new GameManager();

    public List<GameLevel> LevelProgression = new List<GameLevel>();

    private GameState currentState;

    private Scene currentScene;

    private GameObject KinectMenuInteraction;

    // make sure the constructor is private, so it can only be instantiated here
    private GameManager()
    {
        currentState = GameState.NONE;
        currentScene = SceneManager.GetSceneAt(0);
        SceneManager.activeSceneChanged += OnSceneChanged; // unsubscribe

        if (DebugManager.Debug)
            Debug.Log("GameManager was created");

        BlobTracker.ImpactPointDetected += OnImpactPointDetected;
    }

    public void InitIfNeeded()
    {
        if (DebugManager.Debug)
            Debug.Log("GameManager exists");
    }

    private void InitKIM()
    {
        KinectMenuInteraction = new GameObject();
        KinectManager km = KinectMenuInteraction.AddComponent<KinectManager>();
        InteractionManager im = KinectMenuInteraction.AddComponent<InteractionManager>();
        KinectMenuInteraction.tag = "Kinect";
        im.controlMouseCursor = true;
        im.controlMouseDrag = true;
        KinectMenuInteraction.name = "KinectInteractionManager";
        if (DebugManager.Debug)
        {
            Debug.Log("-----" + KinectMenuInteraction.scene.buildIndex);

            Debug.Log("Kinect Interaction Manager was created");
        }

        // textures
    }

    public static GameManager Instance
    {
        get { return instance; }
    }

    public GameState CurrentState   // static ?
    {
        get { return currentState; }
        set
        {
            switch (value)
            {
                case GameState.MAIN_MENU:
                    ChangeScene("MainMenu");
                    break;
                case GameState.LEVEL_SELECT:
                    ChangeScene("LevelSelect");
                    break;
                case GameState.PAUSED:
                    //UI.showPausePanel
                    break;
                case GameState.OPTIONS_MENU:    // is it really useful ?
                    //UI.showOptionsPanel
                    break;
                case GameState.IN_GAME:
                    //UI.showInGameUI
                    break;
                //TODO: shooting vs preparing/ball color detection
                
                case GameState.GAME_SUCCESS:
                    if (currentState == GameState.GAME_OVER)
                        value = currentState;
                    break;
                case GameState.GAME_OVER:
                    if (currentState == GameState.GAME_SUCCESS)
                        value = currentState;
                    break;
                case GameState.QUIT:
                    Quit();
                    break;
            }
            currentState = value;

            if (DebugManager.Debug)
                Debug.Log("Game State changed to: " + GameManager.Instance.currentState.ToString());
        }
    }

    void OnImpactPointDetected(float x, float y)
    {
        Debug.Log("point d'impact ! " + x + "," + y);
        Instance.ActionFromBallOrClick(x, y);
    }

    // x and y in normalized screen space 
    public void ActionFromBallOrClick(float x, float y)   // TODO: FIND A WAY BETTER NAME
    {
        GameObject projectileShooterObject = GameObject.Find("PlayerController");
        if (projectileShooterObject != null)
        {
            ProjectileShooter shooter = projectileShooterObject.GetComponent<ProjectileShooter>();
            // shooter.ShootProjectile(new Vector3(x, y, 0), Vector3.forward);  REACTIVATE WHEN BLOBTRACKER CAN BE MODIFIED
        }

        else if (SceneManager.GetActiveScene().name == "LevelSelect")
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "Clickable")
                {
                    // this works only if the name of the objet is the same as the corresponding scene
                    Instance.ChangeScene(hit.collider.name);
                }

            }
        }

        // other cases such as transition between levels
    }

    public void ChangeScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        currentScene = SceneManager.GetActiveScene();
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        currentScene = SceneManager.GetActiveScene();
    }

    void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        if (DebugManager.Debug)
            Debug.Log("New scene loaded: " + newScene.buildIndex + ", " + newScene.name);

        GameObject projectileShooterObject = GameObject.Find("PlayerController");
        GameObject blobTracker = GameObject.Find("BlobTracker");
        if (blobTracker && projectileShooterObject)
        {
            var shooter = projectileShooterObject.GetComponent<ProjectileShooter>();
            var tracker = blobTracker.GetComponent<BlobTracker>();
            if (tracker)
            {
                if (shooter)
                {
                    tracker.SetShooter(shooter);
                }
                else
                {
                    tracker.SetShooter(null);
                }
            }
        }
    }

    public void UpdateProgression(Scene level)
    {
        switch (level.name)
        {
            case "Planete1":
                Instance.LevelProgression.Add(GameLevel.Planete1);
                break;
            case "Planete2":
                Instance.LevelProgression.Add(GameLevel.Planete2);
                break;
            case "Planete3":
                Instance.LevelProgression.Add(GameLevel.Planete3);
                break;
            case "Planete4":
                Instance.LevelProgression.Add(GameLevel.Planete4);
                break;
            case "Planete5":
                Instance.LevelProgression.Add(GameLevel.Planete5);
                break;
        }
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

