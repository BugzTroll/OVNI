using UnityEngine;
using System.Collections;

public class IntroLevelStart : MonoBehaviour {

    public GameObject introPanel;
    public GameObject tutorialPanel;

    void Start()
    {

    } 


    public void ShowTutorialPanel()
    {
        tutorialPanel.SetActive(true);
    }

    public void HideTutorialPanel()
    {
        tutorialPanel.SetActive(false);
    }

    public void ShowIntroPanel()
    {
        introPanel.SetActive(true);
    }

    public void HideIntroPanel()
    {
        introPanel.SetActive(false);
    }
}
