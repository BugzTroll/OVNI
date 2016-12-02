using UnityEngine;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    
    public int secondes = 10;
    private float time;
    private bool killed = false;

	void Start ()
	{
	    time = GameManager.Instance.visitedPlanet.Contains(SceneManager.GetActiveScene().buildIndex) ? secondes : 0;
	    GameManager.Instance.CurrentState = GameManager.GameState.PopUp;
	}
	
	void Update ()
	{
	    time += Time.deltaTime;
	    if (time >= secondes && !killed)
	    {
            gameObject.SetActive(false);
	        killed = true;
            GameManager.Instance.CurrentState = GameManager.GameState.InGame;
        }

	}
}
