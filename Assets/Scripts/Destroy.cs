using UnityEngine;
using System.Collections;

public class Destroy : MonoBehaviour
{
    public float LifeTime = 8.0f;
	
	// Update is called once per frame
	void Start () {
	 Destroy(gameObject, LifeTime);   
	}
}
