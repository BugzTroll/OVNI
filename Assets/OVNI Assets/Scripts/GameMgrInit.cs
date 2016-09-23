using UnityEngine;
using System.Collections;

public class GameMgrInit : MonoBehaviour {

    void Start ()
    {
        Debug.Log(gameObject.scene.name);
        GameManager.Instance.InitIfNeeded();
    }

}
