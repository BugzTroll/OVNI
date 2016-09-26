using UnityEngine;
using System.Collections;

public class ProjectileShooter : MonoBehaviour {

    GameObject projectilePrefab;

    public float speed = 20;
    Rect rect;
    Texture texture;
    // Use this for initialization
    void Start () {
        projectilePrefab = Resources.Load("projectile") as GameObject;
        rect = new Rect(Screen.width * 0.05f, Screen.height * 0.80f, Screen.width * 0.05f, Screen.width * 0.05f);
        texture = Resources.Load("Texture/bomb") as Texture;

    }

    
    void OnGUI()
    {
        for (int i = 0; i < GameVariables.ammunition; i++)
        {
            Rect newRect = new Rect(rect.x, rect.y - i * Screen.width * 0.07f, rect.width, rect.width);
            GUI.DrawTexture(newRect, texture);
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
