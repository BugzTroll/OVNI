using UnityEngine;
using System.Collections;

public class CubeRain : MonoBehaviour {

    public GameObject cubePrefab = null;
    public GameObject cadeauPrefab = null;
    public float rayon = 5.0f;
    int cadeau = 0;
    int flocon = 0;
    public int timer_cadeau = 150;
    public int timer_flocon = 4;
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        cadeau++;
        if(cadeau == timer_cadeau)
        {
            GameObject projectile2 = Instantiate(cadeauPrefab);
            Vector3 position2 = new Vector3(Random.Range(-rayon, rayon), 0, Random.Range(-rayon, rayon));
            projectile2.transform.position = gameObject.transform.position - position2;
            cadeau = 0;
        }
        flocon++;
        if (flocon == timer_flocon)
        {
            GameObject projectile = Instantiate(cubePrefab);
        Vector3 position = new Vector3(Random.Range(-rayon, rayon), 0, Random.Range(-rayon, rayon));
        projectile.transform.position = gameObject.transform.position - position;
            flocon = 0;
        }
    }
}
