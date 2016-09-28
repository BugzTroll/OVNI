using UnityEngine;
using System.Collections;

public class GameLevelController : MonoBehaviour {

    public UnityEngine.UI.Text scoreText;

    private int score = 0;

	// Use this for initialization
	void Start () {
        score = 0;
        scoreText.text = "Score: ";	
	}
	
	// Update is called once per frame
	void Update () {
        UpdateScoreText();
	}

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }

    public void AddScore(int addScore)
    {
        score += addScore;
    }
}
