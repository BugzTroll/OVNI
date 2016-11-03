using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public class ProjectileShooter : MonoBehaviour
{
    public static event UnityAction<float, float> ClickDetected;

    public enum ProjectileType
    {
        NONE = -1,
        TOMATO = 0,
        BOMB = 1,
        ACID = 2,
        ROCK = 3,
        MISSILE = 4,
        TYPE_COUNT = 5
        // TO BE CONTINUED
    }

    public UnityEngine.UI.RawImage Upcomming_0;
    public UnityEngine.UI.RawImage Upcomming_1;
    public UnityEngine.UI.RawImage Upcomming_2;
    public UnityEngine.UI.RawImage Upcomming_3;
    public UnityEngine.UI.RawImage Upcomming_4;
    public GameObject tomatoPrefab;
    public GameObject bombPrefab;
    public GameObject acidPrefab;
    public GameObject rockPrefab;
    public GameObject missilePrefab;
    public string Ammo;
    public float speed = 10; // change per prefab

    private int currentProjectileIdx;
    private ProjectileType equippedProjectile;
    private List<Texture2D> AmmoPictures;

    // Used to create projectiles and create the GUI for remaining ammo
    void Start()
    {
        Random.InitState((int)(Time.deltaTime/1000.0f));
        equippedProjectile = (ProjectileType) ((int) char.GetNumericValue(Ammo[currentProjectileIdx]));
      
        AmmoPictures = new List<Texture2D>();
        AmmoPictures.Add(Resources.Load("Textures/Tomate") as Texture2D);
        AmmoPictures.Add(Resources.Load("Textures/Bomb") as Texture2D);
        AmmoPictures.Add(Resources.Load("Textures/Acid") as Texture2D);
        AmmoPictures.Add(Resources.Load("Textures/rock") as Texture2D);
        AmmoPictures.Add(Resources.Load("Textures/missile") as Texture2D);
        AmmoPictures.Add(Resources.Load("Textures/Red-X") as Texture2D);

        UpdateAmmoPictures();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ClickDetected(Input.mousePosition.x, Input.mousePosition.y);
        }
    }

    // temp fix; ammo should be a dynamic array (remove a projectile from it when it is used)
    public int GetCurrentAmmoCount()
    {
        return Ammo.Length - currentProjectileIdx;
    }

    void UpdateAmmoPictures()
    {
        // Image du projectile courant
        Upcomming_0.texture = currentProjectileIdx < Ammo.Length
            ? AmmoPictures[(int) char.GetNumericValue(Ammo[currentProjectileIdx])]
            : AmmoPictures.Last();

        // Image du projectile 1
        Upcomming_1.texture = currentProjectileIdx + 1 < Ammo.Length
            ? AmmoPictures[(int) char.GetNumericValue(Ammo[currentProjectileIdx + 1])]
            : AmmoPictures.Last();

        // Image du projectile 2
        Upcomming_2.texture = currentProjectileIdx + 2 < Ammo.Length
            ? AmmoPictures[(int) char.GetNumericValue(Ammo[currentProjectileIdx + 2])]
            : AmmoPictures.Last();

        // Image du projectile 3
        Upcomming_3.texture = currentProjectileIdx + 3 < Ammo.Length
            ? AmmoPictures[(int) char.GetNumericValue(Ammo[currentProjectileIdx + 3])]
            : AmmoPictures.Last();

        // Image du projectile 4
        Upcomming_4.texture = currentProjectileIdx + 4 < Ammo.Length
            ? AmmoPictures[(int) char.GetNumericValue(Ammo[currentProjectileIdx + 4])]
            : AmmoPictures.Last();
    }

    void CreateProjectile(Vector3 startPosition, Vector3 startTrajectory)
    {
        GameObject projectilePrefab = null;
        switch (equippedProjectile)
        {    
            case ProjectileType.TOMATO:
                projectilePrefab = tomatoPrefab;
                break;
            case ProjectileType.BOMB:
                projectilePrefab = bombPrefab;
                break;
            case ProjectileType.ACID:
                projectilePrefab = acidPrefab;
                break;
            case ProjectileType.ROCK:
                projectilePrefab = rockPrefab;
                break;
            case ProjectileType.MISSILE:
                projectilePrefab = missilePrefab;
                break;
        }
        GameObject projectile = Instantiate(projectilePrefab);

        // Initial Projectile Position
        projectile.transform.position = startPosition;
        var angle = -Mathf.Acos(Vector3.Dot(startTrajectory, Vector3.forward))*Mathf.Rad2Deg;
        var axis = Vector3.Cross(startTrajectory, Vector3.forward);
        Quaternion quat = Quaternion.AngleAxis(angle, axis);
        projectile.transform.rotation *= quat;

        //Velocity
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if(equippedProjectile != ProjectileType.MISSILE)
            rb.AddTorque(new Vector3(GetRandom(10), GetRandom(10), GetRandom(10)));

        // temp (will come from the "speed" of the tracking)
        rb.velocity = startTrajectory*speed;
    }

    float GetRandom(float max)
    {
        return Random.Range(-max, max);
    }

    //TODO ajouter speed en y et velocity
    public void ShootProjectile(Vector3 screenPosition)
    {
        screenPosition.z = 1;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        Vector3 velocity = worldPosition - Camera.main.gameObject.transform.position;
        velocity.Normalize();
        if (currentProjectileIdx < Ammo.Length)
        {
            CreateProjectile(worldPosition, velocity);
            currentProjectileIdx++;
            equippedProjectile = currentProjectileIdx < Ammo.Length ? (ProjectileType)char.GetNumericValue(Ammo[currentProjectileIdx]) : ProjectileType.NONE;
        }
        UpdateAmmoPictures();
    }
}