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

    private static GameManager instance = new GameManager();

    private GameState currentState;

    // make sure the constructor is private, so it can only be instantiated here
    private GameManager()
    {
        currentState = GameState.NONE;
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
        SceneManager.UnloadScene(SceneManager.GetActiveScene());
        SceneManager.LoadScene(sceneIndex);
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.UnloadScene(SceneManager.GetActiveScene());
        SceneManager.LoadScene(sceneName);
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

