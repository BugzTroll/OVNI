using UnityEngine;
using System.Collections;

public class ProjectileShooter : MonoBehaviour {

    public enum ProjectileType
    {
        TOMATO,
        BOMB,
        TYPE_COUNT
        // TO BE CONTINUED
    }

    public GameObject tomatoPrefab;
    public GameObject bombPrefab;

    public int tomatoCount;
    public int bombCount;

    // change per prefab
    public float speed;

    //Rect rect;
    //Texture texturebomb;
    //Texture texturetomate;

    private ProjectileType equippedProjectile;


    // Used to create projectiles and create the GUI for remaining ammo
    void Start ()
    {
        //// Load Textures
        //texturebomb = Resources.Load("Textures/Bomb") as Texture;
        //texturetomate = Resources.Load("Textures/Tomate") as Texture;

        //// Picutre Draw Frame
        //rect = new Rect(Screen.width * 0.03f, Screen.height * 0.80f, Screen.width * 0.03f, Screen.width * 0.03f);

        equippedProjectile = ProjectileType.TOMATO;
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        if (Input.GetKeyDown("space")) // find kinect gesture eventually ?
        {
            // cycle between available projectile types
            int nextType = ((int)equippedProjectile) + 1;
            nextType = nextType % ((int)ProjectileType.TYPE_COUNT);
            equippedProjectile = (ProjectileType)nextType;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;


            // make a switch (equippedProjectile) if we ever have too many types
            if (equippedProjectile == ProjectileType.BOMB)
            {
                if (bombCount > 0)
                {
                    CreateProjectile(bombPrefab);
                    bombCount--;
                }
            }
            if (equippedProjectile == ProjectileType.TOMATO)
            {
                if (tomatoCount > 0)
                {
                    CreateProjectile(tomatoPrefab);
                    tomatoCount--;
                }
            }
        }
    }

    void CreateProjectile(GameObject projectilePrefab)
    {
        GameObject projectile = Instantiate(projectilePrefab) as GameObject;
        Vector3 positions2 = Input.mousePosition;
        positions2.z = 3;
        Vector3 position = Camera.main.ScreenToWorldPoint(positions2);
        projectile.transform.position = position;
        
        //Velocity
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.velocity = Camera.main.transform.forward * speed;
    }
}
