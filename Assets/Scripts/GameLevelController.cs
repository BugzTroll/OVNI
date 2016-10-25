using UnityEngine;
using System.Collections;

public class GameLevelController : MonoBehaviour {

    private ProjectileShooter shooter;

    public UnityEngine.UI.Text scoreText;
    public UnityEngine.UI.Text tomateText;
    public UnityEngine.UI.Text bombText;

    public int score = 0;
    public int scoreToWin;
    public GameObject targetObjects;

	// Use this for initialization
	void Start () {
        Time.timeScale = 1.0f;  // put it at 1 again in case it got slowed down during the fail/win screen !

        if (targetObjects != null)
            scoreToWin = 1000000000;

        GameObject projectileShooterObject = GameObject.Find("PlayerController");
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

        // it would be better here than in LevelEnd
        //CheckIfAllTargetsDestroyed(targetObjects);
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

    public bool CheckIfAllTargetsDestroyed()
    {
        int remainingObjects = 0;
        foreach (Transform child in targetObjects.transform)
        {
            if (child.gameObject.tag == "Container")
            {
                remainingObjects += child.childCount;
            }

            else
                remainingObjects++;

            if (remainingObjects > 0)
                return false;

        }

        return true;
    }

}
