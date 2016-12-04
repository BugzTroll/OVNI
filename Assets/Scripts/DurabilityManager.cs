using UnityEngine;
using System.Collections;

public class DurabilityManager : MonoBehaviour
{
    private GameLevelController gameController;

    public int pointsWhenDestroyed;
    public float durability;


    void Start()
    {
        GameObject gameControllerObject = GameObject.Find("GameLevelController");
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject.GetComponent<GameLevelController>();
        }
        if (gameController == null)
        {
            if (DebugManager.Debug)
                Debug.Log("Cannot find 'GameController' script");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        float damageDone = 0;
        GameObject projectileObject = collision.gameObject;
        Projectile proj = projectileObject.GetComponent<Projectile>();

        if (projectileObject.tag == "Projectile" && proj)
        {
            Destroy(projectileObject);
        }

        DamageDealer damageDealer = collision.gameObject.GetComponent<DamageDealer>();

        if (damageDealer)
        {
            if (damageDealer.name == "Acid(Clone)")
            {
                Color altColor = Color.green;
                Renderer rend;
                rend = GetComponent<Renderer>();
                rend.material.color = altColor;

                durability = 1;
                if (DebugManager.Debug)
                {
                    Debug.Log("Remaining durability: " + durability);
                }
            }
            else
            {
                int dmg = damageDealer.baseDamage;

                damageDone += dmg; //(dmg * collision.relativeVelocity.magnitude);
                durability -= damageDone;
                if (DebugManager.Debug)
                {
                    Debug.Log("Damage done: " + damageDone);
                    Debug.Log("Remaining durability: " + durability);
                }
            }
        }
    }


    void Update()
    {
        if (durability <= 0)
        {
            Destroy(gameObject);
            // Destruction animation (?) would go here
            if (gameController != null)
            {
                gameController.AddScore(pointsWhenDestroyed);
                if (DebugManager.Debug)
                    Debug.Log("Object destroyed! Points earned: " + pointsWhenDestroyed);
            }

        }
    }
}