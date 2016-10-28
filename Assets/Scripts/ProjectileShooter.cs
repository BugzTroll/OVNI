using UnityEngine;
using System.Collections;

public class ProjectileShooter : MonoBehaviour
{

    public enum ProjectileType
    {
        TOMATO,
        BOMB,
        TYPE_COUNT
        // TO BE CONTINUED
    }

    public UnityEngine.UI.RawImage Upcomming_0;
    public UnityEngine.UI.RawImage Upcomming_1;
    public UnityEngine.UI.RawImage Upcomming_2;
    public UnityEngine.UI.RawImage Upcomming_3;
    public UnityEngine.UI.RawImage Upcomming_4;
    public GameObject tomatoPrefab;
    public GameObject bombPrefab;
    public string Ammos;


    // change per prefab
    public  float speed = 10;
    private int projectileShooted;
    private ProjectileType equippedProjectile;

    // Used to create projectiles and create the GUI for remaining ammo
    void Start()
    {
        equippedProjectile = (ProjectileType)((int)char.GetNumericValue(Ammos[projectileShooted]));
        updateAmmoPictures();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown("space")) // find kinect gesture eventually ?
        //{
        //    // cycle between available projectile types
        //    int nextType = ((int)equippedProjectile) + 1;
        //    nextType = nextType % ((int)ProjectileType.TYPE_COUNT);
        //    equippedProjectile = (ProjectileType)nextType;
        //    if(DebugManager.Debug)
        //        Debug.Log("Switched to: " + equippedProjectile.ToString());
        //}

        if (Input.GetMouseButtonDown(0))
        {
            // Make sure the click was not on a UI object (an object linked to an EventSystem)
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            ShootProjectile(Input.mousePosition, Vector3.forward);
            if (Ammos.Length > projectileShooted)
            {
                equippedProjectile = (ProjectileType)((int)char.GetNumericValue(Ammos[projectileShooted]));    
            }
            updateAmmoPictures();
        }
        
    }

    void updateAmmoPictures()
    {
        // Image du prochain projectile
        if (Ammos.Length > projectileShooted)
        {
            ProjectileType type = (ProjectileType)((int)char.GetNumericValue(Ammos[projectileShooted]));
            switch (type)
            {
                case ProjectileType.BOMB:
                    Upcomming_0.texture = Resources.Load("Textures/Bomb") as Texture2D;
                    break;

                case ProjectileType.TOMATO:
                    Upcomming_0.texture = Resources.Load("Textures/Tomate") as Texture2D;
                    break;
            }
        }
        else
        {
            Upcomming_0.texture = Resources.Load("Textures/Red-X") as Texture2D;

        }

        // Image du +1
        if (Ammos.Length > projectileShooted + 1)
        {
            ProjectileType type1 = (ProjectileType)((int)char.GetNumericValue(Ammos[projectileShooted +1]));
            switch (type1)
            {
                case ProjectileType.BOMB:
                    Upcomming_1.texture = Resources.Load("Textures/Bomb") as Texture2D;
                    break;

                case ProjectileType.TOMATO:
                    Upcomming_1.texture = Resources.Load("Textures/Tomate") as Texture2D;
                    break;
            }
        }
        else
        {
            Upcomming_1.texture = Resources.Load("Textures/Red-X") as Texture2D;

        }

        // Image du +2
        if (Ammos.Length > projectileShooted + 2)
        {
            ProjectileType type2 = (ProjectileType)((int)char.GetNumericValue(Ammos[projectileShooted +2]));
            switch (type2)
            {
                case ProjectileType.BOMB:
                    Upcomming_2.texture = Resources.Load("Textures/Bomb") as Texture2D;
                    break;

                case ProjectileType.TOMATO:
                    Upcomming_2.texture = Resources.Load("Textures/Tomate") as Texture2D;
                    break;
            }
        }
        else
        {
            Upcomming_2.texture = Resources.Load("Textures/Red-X") as Texture2D;

        }
        // Image du +3
        if (Ammos.Length > projectileShooted + 3)
        {
            ProjectileType type3 = (ProjectileType)((int)char.GetNumericValue(Ammos[projectileShooted +3]));
            switch (type3)
            {
                case ProjectileType.BOMB:
                    Upcomming_3.texture = Resources.Load("Textures/Bomb") as Texture2D;
                    break;

                case ProjectileType.TOMATO:
                    Upcomming_3.texture = Resources.Load("Textures/Tomate") as Texture2D;
                    break;
            }
        }
        else
        {
            Upcomming_3.texture = Resources.Load("Textures/Red-X") as Texture2D;

        }

        // Image du +4
        if (Ammos.Length > projectileShooted + 4)
        {
            ProjectileType type4 = (ProjectileType)((int)char.GetNumericValue(Ammos[projectileShooted + 4]));
            switch (type4)
            {
                case ProjectileType.BOMB:
                    Upcomming_4.texture = Resources.Load("Textures/Bomb") as Texture2D;
                    break;

                case ProjectileType.TOMATO:
                    Upcomming_4.texture = Resources.Load("Textures/Tomate") as Texture2D;
                    break;
            }
        }
        else
        {
            Upcomming_4.texture = Resources.Load("Textures/Red-X") as Texture2D;

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

    public void ShootProjectile(Vector3 screenPosition, Vector3 velocity)
    {
        screenPosition.z = 1;

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        velocity = worldPosition - Camera.main.gameObject.transform.position;
        if (projectileShooted < Ammos.Length)
        {
            ProjectileType type = equippedProjectile;
            switch (type)
            {
                case ProjectileType.BOMB:
                    CreateProjectile(bombPrefab, worldPosition, velocity.normalized);
                    break;

                case ProjectileType.TOMATO:
                    CreateProjectile(tomatoPrefab, worldPosition, velocity.normalized);
                    break;
            }
            projectileShooted++;
        }
    }


}
