using UnityEngine;
using System.Collections;

public class PointGUI : MonoBehaviour {


    Rect point_box;
    // Use this for initialization
    void Start ()
    {
        //Initialise Bomb pictures

        point_box = new Rect(Screen.width * 0.4f, Screen.height * 0.10f, Screen.width * 0.2f, Screen.width * 0.06f);

    }

    // Draw GUI ( points and Ammos)
    void OnGUI()
    {
        // Draw point label
        GUI.Box(point_box, " Points :  " + GameVariables.player_points + " / " + GameVariables.lvl_point);
    }

}
