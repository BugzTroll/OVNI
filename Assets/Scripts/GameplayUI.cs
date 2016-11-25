using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameplayUI : MonoBehaviour
{
    public UnityEngine.UI.Text ScoreText;
    public UnityEngine.UI.Text AmmoText;
    public UnityEngine.UI.RawImage Upcoming0;
    public UnityEngine.UI.RawImage Upcoming1;
    public UnityEngine.UI.RawImage Upcoming2;
    public UnityEngine.UI.RawImage Upcoming3;
    public UnityEngine.UI.RawImage Upcoming4;

    private ProjectileShooter _shooter;
    private List<Texture2D> _ammoPictures;

    private void Start()
    {
        _shooter = GameObject.Find("PlayerController").GetComponent<ProjectileShooter>();

        ScoreText.text = "Score: ";
        GameLevelController.ScoreUpdated += UpdateScoreText;
        ProjectileShooter.ProjectileShooted += UpdateAmmoText;
        ProjectileShooter.ProjectileShooted += UpdateAmmoPictures;

        _ammoPictures = new List<Texture2D>();
        _ammoPictures.Add(Resources.Load("Textures/Tomate") as Texture2D);
        _ammoPictures.Add(Resources.Load("Textures/Bomb") as Texture2D);
        _ammoPictures.Add(Resources.Load("Textures/golf") as Texture2D);
        _ammoPictures.Add(Resources.Load("Textures/missile") as Texture2D);
        _ammoPictures.Add(Resources.Load("Textures/Fireball") as Texture2D);
        _ammoPictures.Add(Resources.Load("Textures/Red-X") as Texture2D);

        UpdateScoreText(0);
        UpdateAmmoText(ProjectileShooter.ProjectileType.NONE);
        UpdateAmmoPictures(ProjectileShooter.ProjectileType.NONE);
    }

    private void OnDestroy()
    {
        GameLevelController.ScoreUpdated -= UpdateScoreText;
        ProjectileShooter.ProjectileShooted -= UpdateAmmoText;
    }

    private void UpdateScoreText(float score)
    {
        ScoreText.text = "Score: " + score;
    }

    private void UpdateAmmoText(ProjectileShooter.ProjectileType projectile)
    {
        var shooter = GameObject.Find("PlayerController").GetComponent<ProjectileShooter>();
        Debug.Assert(shooter);
        AmmoText.text = "Projectiles restants: " + shooter.GetRemainingAmmoCount();
    }

    private void UpdateAmmoPictures(ProjectileShooter.ProjectileType proj)
    {
        var currentProjectileIdx = _shooter.GetCurrentAmmoIdx();

        // Image du projectile courant
        Upcoming0.texture = currentProjectileIdx < _shooter.Ammo.Length
            ? _ammoPictures[(int)char.GetNumericValue(_shooter.Ammo[currentProjectileIdx])]
            : _ammoPictures.Last();

        // Image du projectile 1
        Upcoming1.texture = currentProjectileIdx + 1 < _shooter.Ammo.Length
            ? _ammoPictures[(int)char.GetNumericValue(_shooter.Ammo[currentProjectileIdx + 1])]
            : _ammoPictures.Last();

        // Image du projectile 2
        Upcoming2.texture = currentProjectileIdx + 2 < _shooter.Ammo.Length
            ? _ammoPictures[(int)char.GetNumericValue(_shooter.Ammo[currentProjectileIdx + 2])]
            : _ammoPictures.Last();

        // Image du projectile 3
        Upcoming3.texture = currentProjectileIdx + 3 < _shooter.Ammo.Length
            ? _ammoPictures[(int)char.GetNumericValue(_shooter.Ammo[currentProjectileIdx + 3])]
            : _ammoPictures.Last();

        // Image du projectile 4
        Upcoming4.texture = currentProjectileIdx + 4 < _shooter.Ammo.Length
            ? _ammoPictures[(int)char.GetNumericValue(_shooter.Ammo[currentProjectileIdx + 4])]
            : _ammoPictures.Last();
    }
}