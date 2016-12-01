using UnityEngine;
using System.Collections;

public class AnimationScript : MonoBehaviour {

    public float timer = 10;
    private float time = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        time += Time.deltaTime;

        if (time >= timer)
        {
            GameManager.Instance.CurrentState = GameManager.GameState.MainMenu;
        }
        


    }
}
