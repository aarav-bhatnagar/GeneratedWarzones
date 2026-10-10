using UnityEngine;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine.Animations.Rigging;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class RaycastWeapon : NetworkBehaviour
{
    class Bullet
    {
        public float time;
        public Vector3 initialPosition;
        public Vector3 initialVelocity;
    }

    public int weaponNumber;
    public bool isFiring = false;
    [SerializeField] private ParticleSystem[] muzzleFlashess;
    [SerializeField] private ParticleSystem hitEffectPrefab;
    [SerializeField] private Transform raycastOrigin;
    //public bool canShoot = true;
    public Transform raycastDestination;
    [SerializeField] private float impactForce;
    [SerializeField] private int fireRate = 25;
    [SerializeField] private float bulletSpeed = 1000f;
    [SerializeField] private float bulletDrop = 300f;
    [SerializeField] private int damage;
    public AnimationClip weaponAnimation;
    public WeaponRecoil weaponRecoil;
    private CharacterLocomotion characterLocomotion;

    public OverrideTransform overrideTransform;
    [SerializeField] private Image image;
    [SerializeField] private Color newColor = Color.red;
    private float changeDuration = 0.2f;
    [SerializeField] private TMP_Text killCount;
    private int kill = 0;


    Ray ray;
    RaycastHit hitInfo;
    float accumulatedTime;
    List<Bullet> bullets = new List<Bullet>();
    float maxLifeTimeOfBullet = 5f;
    private bool canCountKill = true; // flag to control kill counting


    private IEnumerator KillCooldown()
    {
        canCountKill = false;
        yield return new WaitForSeconds(1f); // wait 1 second
        canCountKill = true;
    }

    private void Awake()
    {
        weaponRecoil = GetComponent<WeaponRecoil>();


    }

    private void OnEnable()
    {
        characterLocomotion = GetComponentInParent<CharacterLocomotion>();
    }


    Vector3 GetPosition (Bullet bullet)
    {
        //p + v*t + 0.5*g*t*t

        Vector3 gravity = Vector3.down * bulletDrop;
        return (bullet.initialPosition) + (bullet.initialVelocity * bullet.time) + (0.5f * gravity * bullet.time * bullet.time);
    }

    Bullet CreateBullet(Vector3 position, Vector3 velocity)
    {
        Bullet bullet = new Bullet();
        bullet.initialPosition = position;
        bullet.initialVelocity = velocity;
        bullet.time = 0.0f;
        return bullet;
    }

    public void StartFiring()
    {
        //if (canShoot == false)
        isFiring = true;
        accumulatedTime = 0.0f;
        FireBullet();
    }

    public void UpdateFiring(float deltaTime)
    {
        accumulatedTime += deltaTime;
        float fireInterval = 1.0f / fireRate;
        while(accumulatedTime >= 0.0f)
        {
            FireBullet();
            accumulatedTime -= fireInterval;
        }
    }

    public void UpdateBullets(float deltaTime)
    {
        SimulateBullets(deltaTime);
        DestroyBullets();
    }

    private void DestroyBullets()
    {
        bullets.RemoveAll(bullet => bullet.time > maxLifeTimeOfBullet);
    }

    private void SimulateBullets(float deltaTime)
    {
        bullets.ForEach(bullet =>
        {
            Vector3 p0 = GetPosition(bullet);   //currentPosition
            bullet.time += deltaTime;
            Vector3 p1 = GetPosition(bullet);
            RaycastSegment(p0, p1, bullet);
        });


    }

    private void RaycastSegment(Vector3 start, Vector3 end, Bullet bullet)
    {
        {
            Vector3 direction = end - start;
            float distance = direction.magnitude;
            ray.origin = start;
            ray.direction = direction;
            if (Physics.Raycast(ray, out hitInfo, distance))
            {
                if (hitInfo.collider.gameObject.layer != 8)
                {
                    ParticleSystem hitEffect = Instantiate(hitEffectPrefab);
                    hitEffect.transform.SetParent(hitInfo.collider.transform);
                    hitEffect.transform.localPosition = hitInfo.collider.transform.InverseTransformPoint(hitInfo.point);
                    hitEffect.transform.forward = hitInfo.normal;
                    hitEffect.Emit(1);
                    Destroy(hitEffect, 10.0f);

                    bullet.time = maxLifeTimeOfBullet;


                    if (hitInfo.rigidbody != null)
                    {
                        hitInfo.rigidbody.AddForce(-hitInfo.normal * impactForce);
                    }
                }
                else
                {
                    if (PlayerLoader.inLobby) return;

                    CharacterLocomotion player = hitInfo.collider.GetComponentInParent<CharacterLocomotion>();

                    if (player != null)
                    {
                        GameHandler gameHandler = player.gameHandler;
                        bool Kill = gameHandler.TakeDamage(damage);
                        print("isKill: " + Kill.ToString());

                        if (Kill && canCountKill)
                        {
                            GetComponentInParent<winManager>().AddKill();
                            kill++;
                            killCount.text = kill.ToString();

                            // start cooldown so another kill isn't counted right away
                            StartCoroutine(KillCooldown());
                        }

                        StartCoroutine(ChangeColorTemporarily());
                    }
                }

            }
        }

    }

    IEnumerator ChangeColorTemporarily()
    {
        // Change the color to the new color
        image.color = newColor;

        // Wait for the specified duration (2 seconds)
        yield return new WaitForSeconds(changeDuration);

        // Reset the color back to white
        image.color = Color.white;
    }

    private void FireBullet()
    {
        foreach (ParticleSystem muzzleFlash in muzzleFlashess)
        {
            muzzleFlash.Emit(1);
        }

        Vector3 velocity = (raycastDestination.position - raycastOrigin.position).normalized * bulletSpeed;
        var bullet = CreateBullet(raycastOrigin.position, velocity);
        bullets.Add(bullet);

        weaponRecoil.GenerateRecoil();
        overrideTransform.weight = 1;
    }

    public void StopFiring()
    {
        isFiring = false;
        overrideTransform.weight = 0;
    }
}
