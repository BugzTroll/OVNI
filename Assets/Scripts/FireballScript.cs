using UnityEngine;
using System.Collections;

public class FireballScript : MonoBehaviour {

    public GameObject firePrefab;

    //void OnCollisionEnter(Collision col)
    //{
    //    ////GameObject prefab = Resources.Load("Explosion") as GameObject;
    //    //GameObject fire = Instantiate(firePrefab) as GameObject;
    //    //fire.transform.position = transform.position;
    //    //Destroy(fire, 2);
    //    //Destroy(gameObject);
    //}
    void OnDestroy()
    {
        GameObject fire = Instantiate(firePrefab) as GameObject;
        fire.transform.position = transform.position;
        Destroy(fire, 2);
        Destroy(gameObject);
    }


    }
