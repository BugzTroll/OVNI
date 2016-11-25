using UnityEngine;
using System.Collections;

public class TargetZone : MonoBehaviour {

    void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.GetComponent<Rigidbody>().gameObject;
        if (obj)
        {
            GameObject.Find("GameLevelController").GetComponent<GameLevelController>().AddScore(1000);
        }
    }
}
