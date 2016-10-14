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
            Debug.Log("Switched to: " + equippedProjectile.ToString());
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            Vector3 positions2 = Input.mousePosition;
            positions2.z = 3;
            Vector3 positionFromClick = Camera.main.ScreenToWorldPoint(positions2);
            Vector3 trajectory = Camera.main.transform.forward;

            
            ShootProjectile(positionFromClick, trajectory, equippedProjectile);
        }
    }


    

    void CreateProjectile(GameObject projectilePrefab, Vector3 startPosition, Vector3 startTrajectory)
    {
        GameObject projectile = Instantiate(projectilePrefab) as GameObject;

        // Initial Projectile Position
        projectile.transform.position = startPosition;

        //Velocity
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        // temp (will come from the "speed" of the tracking)
        rb.velocity = startTrajectory * speed;
    }




    public void ShootProjectile(Vector3 startPosition, Vector3 startTrajectory, ProjectileType type)
    {
        switch (type)
        {
            case ProjectileType.BOMB:
                CreateProjectile(bombPrefab, startPosition, startTrajectory);
                bombCount--;
                break;

            case ProjectileType.TOMATO:
                CreateProjectile(tomatoPrefab, startPosition, startTrajectory);
                bombCount--;
                break;
        }

    }

    
}
