using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Effects;


public class ExplosionDamage : MonoBehaviour {

    [HideInInspector]
    public float damageAtCenter;
    [HideInInspector]
    public float damageAtRadius;

	IEnumerator Start () {

        yield return null;

        float multiplier = GetComponent<ParticleSystemMultiplier>().multiplier;
        float explosionForce = GetComponent<ExplosionPhysicsForce>().explosionForce;

        float r = 10 * multiplier;
        var cols = Physics.OverlapSphere(transform.position, r);
        var damagedObjectsDurability = new List<DurabilityManager>();
        foreach (var col in cols)
        {
            DurabilityManager durMgr = col.gameObject.GetComponent<DurabilityManager>();
            if (durMgr != null && !damagedObjectsDurability.Contains(durMgr))
            {
                damagedObjectsDurability.Add(durMgr);
            }
        }

        foreach (var dur in damagedObjectsDurability)
        {
            if (DebugManager.Debug)
                Debug.Log("Explosion damage: " + damageAtCenter);
            dur.durability -= damageAtCenter;
            if (DebugManager.Debug)
                Debug.Log("HP after explosion: " + dur.durability);
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
