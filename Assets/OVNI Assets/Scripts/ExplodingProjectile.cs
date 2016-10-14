using UnityEngine;
using System.Collections;
using UnityStandardAssets.Effects;

public class ExplodingProjectile : MonoBehaviour {

    public GameObject explosionPrefab;

    public float explosionForce;
    public float particleMultiplier;

	// Use this for initialization
	void Start () {
	
	}

    void OnCollisionEnter(Collision col)
    {
        //GameObject prefab = Resources.Load("Explosion") as GameObject;
        GameObject explosion = Instantiate(explosionPrefab) as GameObject;
        explosion.transform.position = transform.position;

        
        ExplosionPhysicsForce expForce = explosion.GetComponent<ExplosionPhysicsForce>();
        ParticleSystemMultiplier PSMultiplier = explosion.GetComponent<ParticleSystemMultiplier>();
        if (expForce != null && PSMultiplier != null)
        {
            expForce.explosionForce = explosionForce;
            PSMultiplier.multiplier = particleMultiplier;
        }

        DamageDealer dmg = GetComponent<DamageDealer>();
        ExplosionDamage expDmg = explosion.GetComponent<ExplosionDamage>();
        if (expDmg != null && dmg != null)
        {
            expDmg.damageAtCenter = dmg.baseDamage;
        }

        Destroy(explosion, 5);
        Destroy(gameObject);
    }
}
