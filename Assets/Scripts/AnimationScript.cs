using UnityEngine;
using System.Collections;

public class AnimationScript : MonoBehaviour {

    public float timer = 10;
    private float time = 0;
    public bool Fin = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        time += Time.deltaTime;

        if (time >= timer)
        {
            if (!Fin)
            {
                GameManager.Instance.CurrentState = GameManager.GameState.MainMenu;
            }
            else
            {
               GameManager.Instance.Reset();
            }
        }
    }
}
