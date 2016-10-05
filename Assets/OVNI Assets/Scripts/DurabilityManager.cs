using UnityEngine;
using System.Collections;

public class DurabilityManager : MonoBehaviour
{
    private GameLevelController gameController;

    public int pointsWhenDestroyed;
    public int durability;


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
        GameObject projectile;

        if (collision.gameObject.tag == "Projectile")
        {
            projectile = collision.gameObject;
        }
        Debug.Log(collision.relativeVelocity.magnitude);
        //if (collision.collider.gameObject.tag == "Projectile")
        //{

        if (collision.relativeVelocity.magnitude > 10)
            gameController.AddScore(pointsWhenDestroyed);

        // temp
        //Destroy(collision.collider.gameObject);
        //Destroy(gameObject);
        //}

    }
}