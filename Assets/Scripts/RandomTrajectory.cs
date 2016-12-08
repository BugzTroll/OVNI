using UnityEngine;
using System.Collections;

public class RandomTrajectory : MonoBehaviour
{
    public float radius = 13;
    private Rigidbody body;
    private Vector3 startPos;
    private bool goingLeft = true;
    private bool hit = false;


    void Start()
    {
        body = gameObject.GetComponent<Rigidbody>();
        startPos = gameObject.transform.position;
    }

    void Update()
    {
        if (!hit)
        {
            if (gameObject.transform.position.x < startPos.x - radius)
            {
                goingLeft = false;
                body.velocity = Vector3.zero;
            }
            else if (gameObject.transform.position.x > startPos.x + radius)
            {
                goingLeft = true;
                body.velocity = Vector3.zero;
            }

            if (goingLeft)
            {
                body.AddForce(Vector3.left);
            }
            else
            {
                body.AddForce(Vector3.right);
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        hit = true;
    }
}