using UnityEngine;
using System.Collections;

public class Destroy : MonoBehaviour
{
    public float LifeTime = 10.0f;
	
	// Update is called once per frame
	void Update () {
	 Destroy(gameObject, LifeTime);   
	}
}
