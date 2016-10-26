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

    public GameObject tomatoPrefab;
    public GameObject bombPrefab;

    public int tomatoCount;
    public int bombCount;

    // change per prefab
    public float speed;

    private ProjectileType equippedProjectile;

    // Used to create projectiles and create the GUI for remaining ammo
    void Start()
    {

        equippedProjectile = ProjectileType.TOMATO;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space")) // find kinect gesture eventually ?
        {
            // cycle between available projectile types
            int nextType = ((int)equippedProjectile) + 1;
            nextType = nextType % ((int)ProjectileType.TYPE_COUNT);
            equippedProjectile = (ProjectileType)nextType;
            if(DebugManager.Debug)
                Debug.Log("Switched to: " + equippedProjectile.ToString());
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Make sure the click was not on a UI object (an object linked to an EventSystem)
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            ShootProjectile(Input.mousePosition, Vector3.forward);
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

        ProjectileType type = equippedProjectile;
        switch (type)
        {
            case ProjectileType.BOMB:
                if (bombCount > 0)
                {
                    CreateProjectile(bombPrefab, worldPosition, velocity.normalized);

                    bombCount--;
                }
                break;

            case ProjectileType.TOMATO:
                if (tomatoCount > 0)
                {
                    CreateProjectile(tomatoPrefab, worldPosition, velocity.normalized);
                    tomatoCount--;
                }
                break;
        }
    }


}
