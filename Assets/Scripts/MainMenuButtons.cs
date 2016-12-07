using UnityEngine;
using System.Collections;

public class MainMenuButtons : MonoBehaviour
{

    public string startScene;

    public GameObject optionsPanel;                         //Store a reference to the Game Object OptionsPanel 
    public GameObject optionsTint;                          //Store a reference to the Game Object OptionsTint 

    public void StartButtonClicked()
    {
        GameManager.Instance.ChangeScene(startScene);
    }

    public void ShowOptions()
    {
        optionsPanel.SetActive(true);
        optionsTint.SetActive(true);
    }

    public void HideOptions()
    {
        optionsPanel.SetActive(false);
        optionsTint.SetActive(false);
    }

    public void QuitButtonClicked()
    {
        GameManager.Instance.CurrentState = GameManager.GameState.Quit;
    }

}