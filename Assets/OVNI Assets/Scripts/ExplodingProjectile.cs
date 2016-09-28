using UnityEngine;
using System.Collections;

public class ExplodingProjectile : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    void OnCollisionEnter(Collision col)
    {
        if (GameVariables.current_weapon == "bomb")
        {
            GameObject prefab = Resources.Load("Explosion") as GameObject;
            GameObject Bomb = Instantiate(prefab) as GameObject;
            Bomb.transform.position = transform.position;

            Destroy(Bomb, 5);
            Destroy(gameObject);

        }
    }
}
