using UnityEngine;
using System.Collections;

public class ProjectileShooter : MonoBehaviour {

    GameObject projectilePrefab;
    public float speed = 20;
    Rect rect;
    Rect win;
    Rect point_box;
    Texture texture;


    // Used to create the projectile and create the GUI for remaining ammos
    void Start () {
        projectilePrefab = Resources.Load("projectile") as GameObject;
        rect = new Rect(Screen.width * 0.05f, Screen.height * 0.80f, Screen.width * 0.05f, Screen.width * 0.05f);
        point_box = new Rect(Screen.width * 0.4f, Screen.height * 0.10f, Screen.width * 0.05f, Screen.width * 0.018f);
        texture = Resources.Load("Texture/bomb") as Texture;

    }

    // Draw both GUI ( points and Ammos)
    void OnGUI()
    {
        for (int i = 0; i < GameVariables.ammunition; i++)
        {
            Rect newRect = new Rect(rect.x, rect.y - i * Screen.width * 0.07f, rect.width, rect.width);
            GUI.DrawTexture(newRect, texture);
        }
        GUI.Box(point_box, " Points :  " + GameVariables.player_points);

        if (GameVariables.player_points > GameVariables.lvl_point)
        {
            GameVariables.ammunition = 0;
            win = new Rect(Screen.width * 0.25f, Screen.height * 0.45f, Screen.width * 0.1f, Screen.width * 0.1f);
            GUI.Box(win, "YOU WIN THE GAME");
        }
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            if (GameVariables.ammunition > 0)
            {
                GameObject projectile = Instantiate(projectilePrefab) as GameObject;

                // Direction of the projectile
                Vector3 positions2 = Input.mousePosition;
                positions2.z = 3;
                Vector3 position = Camera.main.ScreenToWorldPoint(positions2);
                projectile.transform.position = position;

                // Velocity
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                rb.velocity = Camera.main.transform.forward * speed;
                GameVariables.ammunition = GameVariables.ammunition-1;
            }
        }
    }
}
