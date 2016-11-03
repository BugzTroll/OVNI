using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class ProjectileShooter : MonoBehaviour
{
    public static event UnityAction<float, float> ClickDetected;

    public enum ProjectileType
    {
        TOMATO,
        BOMB,
        ACID,
        ROCK,
        MISSILE,
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
    public GameObject acidePrefab;
    public GameObject rockPrefab;
    public GameObject missilePrefab;
    public string Ammo;


    // change per prefab
    public  float speed = 10;
    private int projectilesShooted;
    private ProjectileType equippedProjectile;

    // Used to create projectiles and create the GUI for remaining ammo
    void Start()
    {
        equippedProjectile = (ProjectileType)((int)char.GetNumericValue(Ammo[projectilesShooted]));
        updateAmmoPictures();


    }

    // Update is called once per frame
    void Update()
    {
        if (Ammo.Length > projectilesShooted)
        {
            equippedProjectile = (ProjectileType)((int)char.GetNumericValue(Ammo[projectilesShooted]));    
        }
        updateAmmoPictures();

        if (Input.GetMouseButtonDown(0))
        {
            ClickDetected(Input.mousePosition.x, Input.mousePosition.y);
        }
    }

    // temp fix; ammo should be a dynamic array (remove a projectile from it when it is used)
    public int GetCurrentAmmoCount()
    {
        return Ammo.Length - projectilesShooted;
    }

    void updateAmmoPictures()
    {
        // Image du prochain projectile
        if (Ammo.Length > projectilesShooted)
        {
            ProjectileType type = (ProjectileType)((int)char.GetNumericValue(Ammo[projectilesShooted]));
            switch (type)
            {
                case ProjectileType.BOMB:
                    Upcomming_0.texture = Resources.Load("Textures/Bomb") as Texture2D;
                    break;
                case ProjectileType.TOMATO:
                    Upcomming_0.texture = Resources.Load("Textures/Tomate") as Texture2D;
                    break;
                case ProjectileType.ACID:
                    Upcomming_0.texture = Resources.Load("Textures/Acid") as Texture2D;
                    break;
                case ProjectileType.ROCK:
                    Upcomming_0.texture = Resources.Load("Textures/rock") as Texture2D;
                    break;
                case ProjectileType.MISSILE:
                    Upcomming_0.texture = Resources.Load("Textures/missile") as Texture2D;
                    break;
            }
        }
        else
        {
            Upcomming_0.texture = Resources.Load("Textures/Red-X") as Texture2D;

        }

        // Image du +1
        if (Ammo.Length > projectilesShooted + 1)
        {
            ProjectileType type1 = (ProjectileType)((int)char.GetNumericValue(Ammo[projectilesShooted +1]));
            switch (type1)
            {
                case ProjectileType.BOMB:
                    Upcomming_1.texture = Resources.Load("Textures/Bomb") as Texture2D;
                    break;
                case ProjectileType.TOMATO:
                    Upcomming_1.texture = Resources.Load("Textures/Tomate") as Texture2D;
                    break;
                case ProjectileType.ACID:
                    Upcomming_1.texture = Resources.Load("Textures/Acid") as Texture2D;
                    break;
                case ProjectileType.ROCK:
                    Upcomming_1.texture = Resources.Load("Textures/rock") as Texture2D;
                    break;
                case ProjectileType.MISSILE:
                    Upcomming_1.texture = Resources.Load("Textures/missile") as Texture2D;
                    break;
            }
        }
        else
        {
            Upcomming_1.texture = Resources.Load("Textures/Red-X") as Texture2D;

        }

        // Image du +2
        if (Ammo.Length > projectilesShooted + 2)
        {
            ProjectileType type2 = (ProjectileType)((int)char.GetNumericValue(Ammo[projectilesShooted +2]));
            switch (type2)
            {
                case ProjectileType.BOMB:
                    Upcomming_2.texture = Resources.Load("Textures/Bomb") as Texture2D;
                    break;
                case ProjectileType.TOMATO:
                    Upcomming_2.texture = Resources.Load("Textures/Tomate") as Texture2D;
                    break;
                case ProjectileType.ACID:
                    Upcomming_2.texture = Resources.Load("Textures/Acid") as Texture2D;
                    break;
                case ProjectileType.ROCK:
                    Upcomming_2.texture = Resources.Load("Textures/rock") as Texture2D;
                    break;
                case ProjectileType.MISSILE:
                    Upcomming_2.texture = Resources.Load("Textures/missile") as Texture2D;
                    break;
            }
        }
        else
        {
            Upcomming_2.texture = Resources.Load("Textures/Red-X") as Texture2D;

        }
        // Image du +3
        if (Ammo.Length > projectilesShooted + 3)
        {
            ProjectileType type3 = (ProjectileType)((int)char.GetNumericValue(Ammo[projectilesShooted +3]));
            switch (type3)
            {
                case ProjectileType.BOMB:
                    Upcomming_3.texture = Resources.Load("Textures/Bomb") as Texture2D;
                    break;
                case ProjectileType.TOMATO:
                    Upcomming_3.texture = Resources.Load("Textures/Tomate") as Texture2D;
                    break;
                case ProjectileType.ACID:
                    Upcomming_3.texture = Resources.Load("Textures/Acid") as Texture2D;
                    break;
                case ProjectileType.ROCK:
                    Upcomming_3.texture = Resources.Load("Textures/rock") as Texture2D;
                    break;
                case ProjectileType.MISSILE:
                    Upcomming_3.texture = Resources.Load("Textures/missile") as Texture2D;
                    break;
            }
        }
        else
        {
            Upcomming_3.texture = Resources.Load("Textures/Red-X") as Texture2D;

        }

        // Image du +4
        if (Ammo.Length > projectilesShooted + 4)
        {
            ProjectileType type4 = (ProjectileType)((int)char.GetNumericValue(Ammo[projectilesShooted + 4]));
            switch (type4)
            {
                case ProjectileType.BOMB:
                    Upcomming_4.texture = Resources.Load("Textures/Bomb") as Texture2D;
                    break;
                case ProjectileType.TOMATO:
                    Upcomming_4.texture = Resources.Load("Textures/Tomate") as Texture2D;
                    break;
                case ProjectileType.ACID:
                    Upcomming_4.texture = Resources.Load("Textures/Acid") as Texture2D;
                    break;
                case ProjectileType.ROCK:
                    Upcomming_4.texture = Resources.Load("Textures/rock") as Texture2D;
                    break;
                case ProjectileType.MISSILE:
                    Upcomming_4.texture = Resources.Load("Textures/missile") as Texture2D;
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
        if (projectilesShooted < Ammo.Length)
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
                case ProjectileType.ACID:
                    CreateProjectile(acidePrefab, worldPosition, velocity.normalized);
                    break;
                case ProjectileType.ROCK:
                    CreateProjectile(rockPrefab, worldPosition, velocity.normalized);
                    break;
                case ProjectileType.MISSILE:
                    CreateProjectile(missilePrefab, worldPosition, velocity.normalized);
                    break;
            }
            projectilesShooted++;
        }
    }


}
