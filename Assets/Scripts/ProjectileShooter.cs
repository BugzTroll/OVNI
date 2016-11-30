using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public class ProjectileShooter : MonoBehaviour
{
    public static event UnityAction<float, float> ClickDetected;
    public static event UnityAction<ProjectileType> ProjectileShooted;

    public enum ProjectileType
    {
        NONE = -1,
        TOMATO = 0,
        BOMB = 1,
        GOLFBALL = 2,
        MISSILE = 3,
        FIREBALL = 4,
        COWBALL = 5,
        TYPE_COUNT = 6
        // TO BE CONTINUED
    }
    public GameObject TomatoPrefab;
    public GameObject BombPrefab;
    public GameObject GolfBallPrefab;
    public GameObject MissilePrefab;
    public GameObject FireballPrefab;
    public GameObject CowballPrefab;

    public string Ammo;
    //public float Speed = 10; // change per prefab

    private int _currentProjectileIdx;
    private ProjectileType _equippedProjectile;

    public int GetRemainingAmmoCount()
    {
        return Ammo.Length - _currentProjectileIdx;
    }

    public int GetCurrentAmmoIdx()
    {
        return _currentProjectileIdx;
    }

    private void Start()
    {
        Random.InitState((int)(Time.deltaTime/1000.0f));
        _equippedProjectile = (ProjectileType) ((int) char.GetNumericValue(Ammo[_currentProjectileIdx]));
    }

    private void Update()
    {
        // Avoids registering clicks when on ui (i.e. pause panel buttons) when in game (so it works when in GameOver or GameSuccess)
        if (GameManager.Instance.CurrentState == GameManager.GameState.InGame &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // TODO find a better place to handle click event
        if (Input.GetMouseButtonDown(0))
        {
            ClickDetected(Input.mousePosition.x, Input.mousePosition.y);
        }
    }

    public void ShootProjectile(Vector2 screenPos, float speed)
    {
        //TODO ajouter Speed en y et velocity
        Vector3 screenPosition = new Vector3(screenPos.x, screenPos.y, 1);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        Vector3 velocity = worldPosition - Camera.main.gameObject.transform.position;
        velocity.Normalize();

        if (_currentProjectileIdx < Ammo.Length)
        {
            CreateProjectile(worldPosition, velocity, speed);
            _currentProjectileIdx++;
            _equippedProjectile = _currentProjectileIdx < Ammo.Length ? (ProjectileType)char.GetNumericValue(Ammo[_currentProjectileIdx]) : ProjectileType.NONE;

            if (ProjectileShooted != null)
            {
                ProjectileShooted(_equippedProjectile);
            }
        }
    }

    private void CreateProjectile(Vector3 startPosition, Vector3 startTrajectory, float speed)
    {
        GameObject projectilePrefab = null;
        switch (_equippedProjectile)
        {
            case ProjectileType.TOMATO:
                projectilePrefab = TomatoPrefab;
                break;
            case ProjectileType.BOMB:
                projectilePrefab = BombPrefab;
                break;
            case ProjectileType.MISSILE:
                projectilePrefab = MissilePrefab;
                break;
            case ProjectileType.FIREBALL:
                projectilePrefab = FireballPrefab;
                break;
            case ProjectileType.GOLFBALL:
                projectilePrefab = GolfBallPrefab;
                break;
            case ProjectileType.COWBALL:
                projectilePrefab = CowballPrefab;
                break;
        }
        GameObject projectile = Instantiate(projectilePrefab);

        // Initial Projectile Position
        projectile.transform.position = startPosition;
        var angle = -Mathf.Acos(Vector3.Dot(startTrajectory, Vector3.forward)) * Mathf.Rad2Deg;
        var axis = Vector3.Cross(startTrajectory, Vector3.forward);
        Quaternion quat = Quaternion.AngleAxis(angle, axis);
        projectile.transform.rotation *= quat;

        //Velocity
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (_equippedProjectile != ProjectileType.MISSILE)
            rb.AddTorque(new Vector3(MyHelper.GetRandom(10), MyHelper.GetRandom(10), MyHelper.GetRandom(10)));

        // temp (will come from the "Speed" of the tracking)
        rb.velocity = startTrajectory * speed;
    }
}