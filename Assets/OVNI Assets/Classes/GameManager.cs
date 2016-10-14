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
        MAIN_MENU = 0,
        OPTIONS_MENU = 1,
        PAUSED = 2,
        IN_GAME = 3,
        LEVEL_SELECT = 4,
        QUIT = 5
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

        //InitKIM();

        Debug.Log("GameManager was created");

    }

    public void InitIfNeeded()
    {
        if (GameObject.FindGameObjectsWithTag("Kinect").Length == 0)
        {
            //InitKIM();
            //UnityEngine.Object.Instantiate(KinectMenuInteraction);
        }
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
        Debug.Log("-----" + KinectMenuInteraction.scene.buildIndex);

        Debug.Log("Kinect Interaction Manager was created");

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
                case GameState.QUIT:
                    Quit();
                    break;
            }
            currentState = value;
        }
    }

    public void ChangeScene(int sceneIndex)
    {
        //SceneManager.UnloadScene(SceneManager.GetActiveScene());
        SceneManager.LoadScene(sceneIndex);
        currentScene = SceneManager.GetActiveScene();
    }

    public void ChangeScene(string sceneName)
    {
        //SceneManager.UnloadScene(SceneManager.GetActiveScene());
        SceneManager.LoadScene(sceneName);
        currentScene = SceneManager.GetActiveScene();
    }

    void OnSceneChanged(Scene oldScene, Scene newScene)
    {
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

