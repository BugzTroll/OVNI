using UnityEngine;
using System.Collections;

public class GameMgrInit : MonoBehaviour {

    void Start ()
    {
        if(DebugManager.Debug)
            Debug.Log(gameObject.scene.name);
        GameManager.Instance.InitIfNeeded();
    }

}
