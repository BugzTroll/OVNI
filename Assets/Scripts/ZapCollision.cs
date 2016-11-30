using UnityEngine;
using System.Collections;

public class ZapCollision : MonoBehaviour {

    public GameObject zapEffect;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.tag == "Cow")
        {
           GameObject particle =  Instantiate(zapEffect);
            particle.transform.position = transform.position;
        }
    }
}
