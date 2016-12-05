using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

    public bool destroyOnFirstHit;
    private TrailRenderer trail ;

	// Use this for initialization
	void Start () {
            trail = GetComponent<TrailRenderer>();
       
    }
	
	// Update is called once per frame
	void Update () {
	    
    }

    void OnCollisionEnter(Collision col)
    {
       // if (col.relativeVelocity.magnitude > 30.0f)
            if (trail != null)
            trail.enabled = false;
        // trail.Clear();
    }
}
