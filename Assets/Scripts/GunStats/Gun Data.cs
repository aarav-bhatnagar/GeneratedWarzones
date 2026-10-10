using UnityEngine;
using UnityEngine.VFX;

public class GunData : MonoBehaviour
{
    //Gun stats

    public string WeaponName;
    public int damage;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots, impactForce;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;

    public Transform leftPistolPosition, rightPistolPosition, attackPoint;
    public VisualEffect muzzleFlash;
    public int bulletsLeft, bulletsShot;
}
