using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;


public class GameManager
{
    private static GameManager instance = new GameManager();

    // make sure the constructor is private, so it can only be instantiated here
    private GameManager()
    {
    }

    public static GameManager Instance {
        get { return instance; }
    }

    public static void ChangeScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public static void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

