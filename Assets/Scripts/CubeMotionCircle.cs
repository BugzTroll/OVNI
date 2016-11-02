using UnityEngine;
using System.Collections;

public class CubeMotionCircle : MonoBehaviour {


    float timeCounter = 0;

    public float speed;
    public float width;
    public float height;

	// Use this for initialization
	void Start () {
        speed = 2;
        width = 10;
        height = 10;	
	}
	
	// Update is called once per frame
	void Update () {
        timeCounter += Time.deltaTime*speed;

        float x = Mathf.Cos(timeCounter) * width;
        float y = 6;
        float z = -5;

        transform.position = new Vector3(x, y, z);
	
	}
}
