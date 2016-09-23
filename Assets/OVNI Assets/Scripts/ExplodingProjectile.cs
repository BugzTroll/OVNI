using UnityEngine;
using System.Collections;

public class Explodingprojectile : MonoBehaviour {

	// Use this for initialization
	void Start () {}
	
	// Update is called once per frame
	void OnCollisionEnter (Collision collision)
    {
        GameObject prefab = Resources.Load("Fireball") as GameObject;
        GameObject FireBall = Instantiate(prefab) as GameObject;
        FireBall.transform.position = transform.position;
        Destroy(FireBall, 5);
        Destroy(gameObject);

    }
}
