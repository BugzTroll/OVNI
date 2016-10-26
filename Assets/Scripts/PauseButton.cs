using UnityEngine;
using System.Collections;

public class PauseButton : MonoBehaviour {

    public GameObject pauseButton;
    public GameObject pausePanel;                         //Store a reference to the Game Object pausePanel 
    public GameObject optionsTint;
    private bool isPaused;

    // Use this for initialization
    void Start () {
        UnPause();
	}
	
	// Update is called once per frame
	void Update () {
        //Check if the Cancel button in Input Manager is down this frame (default is Escape key) and that game is not paused, and that we're not in main menu
        if (Input.GetButtonDown("Cancel") && !isPaused)
        {
            //Call the DoPause function to pause the game
            DoPause();
        }
        //If the button is pressed and the game is paused and not in main menu
        else if (Input.GetButtonDown("Cancel") && isPaused)
        {
            //Call the UnPause function to unpause the game
            UnPause();
        }
    }

    public void DoPause()
    {
        //Set isPaused to true
        isPaused = true;
        //Set time.timescale to 0, this will cause animations and physics to stop updating
        Time.timeScale = 0;
        //call the ShowPausePanel function of the ShowPanels script
        ShowPausePanel();
    }


    public void UnPause()
    {
        //Set isPaused to false
        isPaused = false;
        //Set time.timescale to 1, this will cause animations and physics to continue updating at regular speed
        Time.timeScale = 1;
        //call the HidePausePanel function of the ShowPanels script
        HidePausePanel();
    }

    void PauseButtonPressed()
    {
        ShowPausePanel();
    }

    public void ShowPausePanel()
    {
        pausePanel.SetActive(true);
        optionsTint.SetActive(true);
        pauseButton.SetActive(false);
    }

    //Call this function to deactivate and hide the Pause panel during game play
    public void HidePausePanel()
    {
        pausePanel.SetActive(false);
        optionsTint.SetActive(false);
        pauseButton.SetActive(true);
    }

    public void ChangeLevelButtonPressed()
    {
        GameManager.Instance.ChangeScene("LevelSelect");
    }
}
