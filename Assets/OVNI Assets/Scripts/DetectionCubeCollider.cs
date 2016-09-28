using UnityEngine;
using System.Collections;

public class DetectionCubeCollider : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Destructable")
        {
            Destroy(other.gameObject);
        }
    }
}
