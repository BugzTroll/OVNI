using UnityEngine;
using System.Collections;

public class OnDestroyPoints : MonoBehaviour
{
    private GameLevelController gameController;
    public int pointsWhenDestroyed;

    void Start()
    {
        GameObject gameControllerObject = GameObject.Find("GameLevelController");
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject.GetComponent<GameLevelController>();
        }
        if (gameController == null)
        {
            Debug.Log("Cannot find 'GameController' script");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Projectile")
        {
            gameController.AddScore(pointsWhenDestroyed);

            // temp
            Destroy(collision.collider.gameObject);
            Destroy(gameObject);
        }
        
    }
}