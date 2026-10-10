using UnityEngine;
using TMPro;
using UnityEngine.VFX;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Collections.Generic;

public class Gun : NetworkBehaviour
{
    public GunData currentGunData;

    //Gun stats
    private int damage;
    private float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots, impactForce;
    private int magazineSize, bulletsPerTap;
    private bool allowButtonHold;
    private int bulletsLeft, bulletsShot;
    private Transform leftPistolPosition, rightPistolPosition;
    private VisualEffect muzzleFlash;
    private Transform attackPoint;


    //bools 
    bool shooting, readyToShoot, reloading;

    //Reference
    public Camera fpsCam;
    public RaycastHit rayHit;

    //Graphics
    public GameObject bulletHoleGraphic;
    //public CamShake camShake;
    //public float camShakeMagnitude, camShakeDuration;
    public TMP_Text ammoText;


    //public Transform hipPosition;
    //public Transform adsPosition;
    //public Transform sprintPosition;
    public Vector3 weaponPosition = new Vector3(0, 0, 0);

    public float aimSpeed = 0.25f; // time to enter ADS
    public float _defaultFOV = 80f; // FOV in degrees
    public float zoomRatio = 0.5f; // 1/zoom times

    //public Recoil Recoil_Script;



    [SerializeField] private Transform leftTarget, rightTarget;

    private List<GameObject> activeGunObjects;
    private List<GameObject> allGunObjects;

    public void OnGunPickedUp(GameObject gunBody)
    {
        currentGunData = gunBody.GetComponent<GunData>();
        
    }

    

    public void OnNewGunLoaded()
    {
        damage = currentGunData.damage;
        timeBetweenShooting = currentGunData.timeBetweenShooting;
        spread = currentGunData.spread;
        range = currentGunData.range;
        reloadTime = currentGunData.reloadTime;
        timeBetweenShots = currentGunData.timeBetweenShots;
        impactForce = currentGunData.impactForce;
        magazineSize = currentGunData.magazineSize;
        bulletsPerTap = currentGunData.bulletsPerTap;
        allowButtonHold = currentGunData.allowButtonHold;
        leftPistolPosition = currentGunData.leftPistolPosition;
        rightPistolPosition = currentGunData.rightPistolPosition;
        attackPoint = currentGunData.attackPoint;
        muzzleFlash = currentGunData.muzzleFlash;


        bulletsLeft = currentGunData.bulletsLeft;
        bulletsShot = currentGunData.bulletsShot;
    }

    public void OnGunRemoved()
    {
        currentGunData.bulletsLeft = bulletsLeft;
        currentGunData.bulletsLeft = bulletsShot;
    }

    public void SetCursor(bool active)
    {
        if (!IsOwner) return;
        if(active)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
        OnNewGunLoaded();
    }
    private void Update()
    {
        MyInput();

        //SetText
        ammoText.text = (bulletsLeft + " / " + magazineSize);


        leftTarget.SetPositionAndRotation(leftPistolPosition.position, leftPistolPosition.rotation);
        rightTarget.SetPositionAndRotation(rightPistolPosition.position, rightPistolPosition.rotation);
    }

    private void MyInput()
    {
        if (!IsOwner) return;
        if (allowButtonHold) shooting = Input.GetButton("Fire1");
        else shooting = Input.GetButtonDown("Fire1");

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();

        //Shoot
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
        }
    }
    private void Shoot()
    {
        readyToShoot = false;

        //Spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);


        RaycastHit hitDestination;
        Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hitDestination);


        //Calculate Direction with Spread
        Vector3 direction = hitDestination.point - attackPoint.position;//+ new Vector3(x, y, 0);

        //RayCast
        if (Physics.Raycast(attackPoint.position, direction, out rayHit, range))
        {

            PlayerMovement player = rayHit.collider.GetComponent<PlayerMovement>();

            if (player != null)
            {

                GameHandler gameHandler = player.gameHandler;
                if (IsServer)
                {
                    gameHandler.TakeDamageClientRpc(damage);
                }
                else if (IsClient)
                {
                    gameHandler.TakeDamageServerRpc(damage);
                }
            }
            
            if (rayHit.rigidbody != null)
            {
                rayHit.rigidbody.AddForce(-rayHit.normal * impactForce);
            }
        }

        //ShakeCamera
        //camShake.Shake(camShakeDuration, camShakeMagnitude);


        
        muzzleFlash.Play();
        //Recoil_Script.RecoilFire();

        bulletsLeft--;
        bulletsShot--;

        Invoke("ResetShot", timeBetweenShooting);

        if (bulletsShot > 0 && bulletsLeft > 0)
            Invoke("Shoot", timeBetweenShots);

        if (rayHit.transform != null)
        {
            if (rayHit.collider.gameObject.layer != 8 && rayHit.collider.CompareTag("ZoneWall") != true)
            {
                GameObject bulletHole = Instantiate(bulletHoleGraphic, rayHit.point, Quaternion.LookRotation(rayHit.normal));
                bulletHole.transform.SetParent(rayHit.transform, true);
            }
        }
    }
    private void ResetShot()
    {
        readyToShoot = true;
    }
    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }
    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}