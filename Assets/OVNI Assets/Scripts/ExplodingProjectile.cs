using UnityEngine;
using System.Collections;

public class ExplodingProjectile : MonoBehaviour {

    public GameObject explosionPrefab;

	// Use this for initialization
	void Start () {
	
	}

    void OnCollisionEnter(Collision col)
    {
        //GameObject prefab = Resources.Load("Explosion") as GameObject;
        GameObject explosion = Instantiate(explosionPrefab) as GameObject;
        explosion.transform.position = transform.position;

        Destroy(explosion, 5);
        Destroy(gameObject);
    }
}
