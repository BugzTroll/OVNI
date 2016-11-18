using UnityEngine;
using System.Collections;

public class RigidBodyCenterOfMass : MonoBehaviour {
    public Vector3 com;
    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = com;
    }

}
