using UnityEngine;
using System.Collections;

public class OncollisionLevelSelector : MonoBehaviour {

    void OnCollisionEnter(Collision collision)
    {
        GameObject.Find("Fading").GetComponent<Fader>().EndScene(gameObject.name);
    }
}
