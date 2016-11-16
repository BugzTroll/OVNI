using UnityEngine;
using System.Collections;

public class Splash : MonoBehaviour
{
    public Material BaseMaterial;
    public Material SplashMaterial;

    private Renderer renderer;
    private bool NeverBeenHit = true;

    // Use this for initialization
    void Start ()
    {
        renderer = gameObject.GetComponent<Renderer>();
        renderer.material = BaseMaterial;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Projectile" && NeverBeenHit)
        {
            renderer.material = SplashMaterial;
            NeverBeenHit = false;
            GameObject.Find("GameLevelController").GetComponent<GameLevelController>().AddScore(100);
        }
    }
}
