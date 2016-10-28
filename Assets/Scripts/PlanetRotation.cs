using UnityEngine;
using System.Collections;

public class PlanetRotation : MonoBehaviour {

    public float rotationAngle;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, rotationAngle * Time.deltaTime, 0);
    }
}
