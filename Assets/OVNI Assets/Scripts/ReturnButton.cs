using UnityEngine;
using System.Collections;

public class ReturnButton : MonoBehaviour {

    public void ReturnButtonPressed()
    {
        GameManager.Instance.ChangeScene("MainMenu");
    }
}
