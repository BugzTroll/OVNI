using UnityEngine;
using System.Collections;

public class GameLevelController : MonoBehaviour {

    private ProjectileShooter shooter;

    public UnityEngine.UI.Text scoreText;
    public UnityEngine.UI.Text tomateText;
    public UnityEngine.UI.Text bombText;

    public int score = 0;
    public int scoreToWin;

	// Use this for initialization
	void Start () {

        GameObject projectileShooterObject = GameObject.Find("Player");
        if (projectileShooterObject != null)
        {
            shooter = projectileShooterObject.GetComponent<ProjectileShooter>();
        }

        score = 0;
        scoreText.text = "Score: ";
        tomateText.text = " Tomates Restantes ";
        bombText.text = " Bombes Restantes ";
    }
	
	// Update is called once per frame
	void Update () {
        UpdateScoreText();
        UpdateAmmoCount();
	}

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }
    void UpdateAmmoCount()
    {
        tomateText.text = shooter.tomatoCount + " Tomates Restantes ";
        bombText.text = shooter.bombCount + " Bombes Restantes ";
    }


    public void AddScore(int addScore)
    {
        score += addScore;
    }

}
