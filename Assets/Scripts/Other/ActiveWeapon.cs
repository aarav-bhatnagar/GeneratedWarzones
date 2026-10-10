using UnityEngine;
//using UnityEditor.Animations;
using UnityEngine.Animations.Rigging;
using Unity.Netcode;

public class ActiveWeapon : NetworkBehaviour
{
    RaycastWeapon raycastWeapon;
    public Transform CrossHairTarget;
    [SerializeField] private UnityEngine.Animations.Rigging.Rig HandIK;
    private NetworkVariable<bool> HandIKWeight = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] private Transform weaponHolder;
    private CharacterAiming characterAiming;
    public Camera _camera;
    [SerializeField] private Transform recoilPose;
    private bool canShoot;
 

    public Transform weaponLeftGrip;
    public Transform weaponRightGrip;

    Animator anim;
    AnimatorOverrideController overrides;

    public NetworkVariable<int> currentWeapon = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        characterAiming = GetComponent<CharacterAiming>();
        overrides = anim.runtimeAnimatorController as AnimatorOverrideController;
        CrossHairTarget = FindFirstObjectByType<CrossHairTarget>().transform;
        _camera = Camera.main;
        currentWeapon.OnValueChanged += (oldValue, newValue) => ChangeWeapon(newValue);

    }

    // Update is called once per frameS
    void Update()
    {
        if (HandIKWeight.Value)
        {
            HandIK.weight = 1;
        }
        else
        {
            HandIK.weight = 0;
        }
        if (!IsOwner) return;

        if (raycastWeapon && canShoot)
        {
            if (Input.GetButtonDown("Fire1") && raycastWeapon.isFiring == false) 
            {
                raycastWeapon.StartFiring();
            }

            if (raycastWeapon.isFiring)
            {
                raycastWeapon.UpdateFiring(Time.deltaTime);
            }

            raycastWeapon.UpdateBullets(Time.deltaTime);

            if (Input.GetButtonUp("Fire1") && raycastWeapon.isFiring == true)
            {
                raycastWeapon.StopFiring();
            }
        }
        else
        {
            HandIKWeight.Value = false;
            anim.SetLayerWeight(1, 0.0f);
        }
    }

    public void Equip(int weaponNumber)
    {
        currentWeapon.Value = weaponNumber;
    }

    private void ChangeWeapon(int weaponNumber)
    {
        if (raycastWeapon)
        {
            HideWeapon(raycastWeapon.gameObject);
        }
        print("WeaponNumber: " + weaponNumber.ToString());
        ShowWeapon(weaponHolder.GetChild(weaponNumber).gameObject);
        RaycastWeapon newWeapon = weaponHolder.GetChild(weaponNumber).gameObject.GetComponent<RaycastWeapon>();
        raycastWeapon = newWeapon;
        raycastWeapon.raycastDestination = CrossHairTarget;
        raycastWeapon.weaponRecoil._camera = _camera;

        raycastWeapon.overrideTransform = recoilPose.GetComponent<OverrideTransform>();
        raycastWeapon.overrideTransform.weight = 0; // Disable the override temporarily
        raycastWeapon.transform.localPosition = Vector3.zero;
        raycastWeapon.transform.localRotation = Quaternion.identity;
        HandIKWeight.Value = true;
        anim.SetLayerWeight(1, 1.0f);
        Invoke(nameof(SetAnimationDelayed), 0.05f);
        canShoot = true;
    }

    public void ResetAllWeapons()
    {
        if (raycastWeapon)
        {
            HideWeapon(raycastWeapon.gameObject);
            HandIKWeight.Value = false;
            anim.SetLayerWeight(1, 0.0f);
            canShoot = false;
        }
    }

    private void HideWeapon(GameObject weapon)
    {
        weapon.SetActive(false);
    }

    private void ShowWeapon(GameObject weapon)
    {

        weapon.SetActive(true);
    }

    private void SetAnimationDelayed()
    {
        overrides["weapon_anim_empty"] = raycastWeapon.weaponAnimation;

    }

    /*
    [ContextMenu("Save Weapon Pose")]
    private void SaveWeaponPose()
    {
        GameObjectRecorder recorder = new GameObjectRecorder(gameObject);
        recorder.BindComponentsOfType<Transform>(weaponHolder.gameObject, false);
        recorder.BindComponentsOfType<Transform>(weaponLeftGrip.gameObject, false);
        recorder.BindComponentsOfType<Transform>(weaponRightGrip.gameObject, false);
        recorder.TakeSnapshot(0.0f);
        recorder.SaveToClip(raycastWeapon.weaponAnimation);
        UnityEditor.AssetDatabase.SaveAssets();

    }
    */
}
