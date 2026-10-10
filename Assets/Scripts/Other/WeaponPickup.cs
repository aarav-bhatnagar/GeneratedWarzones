using UnityEngine;
using Unity.Netcode;

public class WeaponPickup : NetworkBehaviour
{
    [SerializeField] private RaycastWeapon weaponPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            ActiveWeapon activeWeapon = other.gameObject.GetComponentInParent<ActiveWeapon>();
            if (activeWeapon == null) print("NOT WORKING");
            activeWeapon.Equip(weaponPrefab.weaponNumber);
            DespawnObjectServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnObjectServerRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn(true);

    }

}
