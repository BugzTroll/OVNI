using UnityEngine;
using System.Collections;

public class ProjectileShooter : MonoBehaviour {

    GameObject tomatoPrefab;
    GameObject bombPrefab;
    public float speed = 100;
    Rect rect;
    Texture texturebomb;
    Texture texturetomate;


    // Used to create the projectile and create the GUI for remaining ammos
    void Start ()
    {
        // Load Ammo Prefabs
        tomatoPrefab = Resources.Load("tomato") as GameObject;
        bombPrefab = Resources.Load("bomb") as GameObject;

        // Load Textures
        texturebomb = Resources.Load("Texture/bomb") as Texture;
        texturetomate = Resources.Load("Texture/Tomate") as Texture;

        // Picutre Draw Frame
        rect = new Rect(Screen.width * 0.03f, Screen.height * 0.80f, Screen.width * 0.03f, Screen.width * 0.03f);

    }

    void OnGUI()
    {
        // For each remaining bomb draw a picture
        for (int i = 0; i < GameVariables.bomb; i++)
        {
            Rect newRect = new Rect(rect.x+40, rect.y - i * Screen.width * 0.03f, rect.width, rect.width);
            GUI.DrawTexture(newRect, texturebomb);
        }
        // For each remaining tomato draw a picture
        for (int i = 0; i < GameVariables.tomato; i++)
        {
            Rect newRect = new Rect(rect.x-20, rect.y - i * Screen.width * 0.03f, rect.width, rect.width);
            GUI.DrawTexture(newRect, texturetomate);
        }
    }

    // Update is called once per frame
    void Update ()
    {
        if (Input.GetKeyDown("space"))
        {
            if (GameVariables.current_weapon == "tomato")
            {
                GameVariables.current_weapon = "bomb";
            }
            else
            {
                GameVariables.current_weapon = "tomato";
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (GameVariables.current_weapon == "bomb")
            {
                if (GameVariables.bomb > 0)
                {
                    GameObject bomb = Instantiate(bombPrefab) as GameObject;
                    Vector3 positions2 = Input.mousePosition;
                    positions2.z = 3;
                    Vector3 position = Camera.main.ScreenToWorldPoint(positions2);
                    bomb.transform.position = position;
                    GameVariables.bomb -= 1;
                    //Velocity
                    Rigidbody rb = bomb.GetComponent<Rigidbody>();
                    rb.velocity = Camera.main.transform.forward * speed;
                }
            }
            if (GameVariables.current_weapon == "tomato")
            {
                if (GameVariables.tomato > 0)
                {
                    GameObject tomato = Instantiate(tomatoPrefab) as GameObject;
                    Vector3 positions2 = Input.mousePosition;
                    positions2.z = 3;
                    Vector3 position = Camera.main.ScreenToWorldPoint(positions2);
                    tomato.transform.position = position;
                    GameVariables.tomato -= 1;
                    //Velocity
                    Rigidbody rb = tomato.GetComponent<Rigidbody>();
                    rb.velocity = Camera.main.transform.forward * speed;
                }
            }
        }
    }
}
