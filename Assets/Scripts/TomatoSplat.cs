using UnityEngine;
using System.Collections;

public class TomatoSplat : MonoBehaviour {

    public GameObject splatEffect;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        
    }

    void OnCollisionEnter(Collision col)
    {
        GameObject particle = Instantiate(splatEffect);
        particle.transform.position = transform.position;
    }
    
}
