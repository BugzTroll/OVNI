﻿using UnityEngine;
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
            if (proj.destroyOnFirstHit)
            {
                Destroy(projectileObject);
            }
        }

        DamageDealer damageDealer = collision.gameObject.GetComponent<DamageDealer>();
        if (damageDealer)
        {
            int dmg = damageDealer.baseDamage;

            damageDone += (dmg * collision.relativeVelocity.magnitude);
            durability -= damageDone;

            Debug.Log("Damage done: " + damageDone);
            Debug.Log("Remaining durability: " + durability);
        }

        if (durability <= 0)
        {
            Destroy(gameObject);
            // Destruction animation (?) would go here
            gameController.AddScore(pointsWhenDestroyed);

            Debug.Log("Object destroyed! Points earned: " + pointsWhenDestroyed);
        }
    }
}