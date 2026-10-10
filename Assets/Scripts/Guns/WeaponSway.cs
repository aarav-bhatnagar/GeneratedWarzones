using UnityEngine;
using Unity.Netcode;

public class WeaponSway : NetworkBehaviour
{
    [SerializeField] private float intensity;
    [SerializeField] private float smooth;
    private Quaternion origin_rotation;

    private void Start()
    {
        origin_rotation = transform.localRotation;
    }

    private void Update()
    {
        if (!IsOwner) return;
        UpdateSway();
    }

    private void UpdateSway()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Quaternion x_adjustment = Quaternion.AngleAxis(intensity * x, Vector3.up * -1);
        Quaternion y_adjustment = Quaternion.AngleAxis((intensity * 0.2f) * y, Vector3.right);

        Quaternion target_rotation = origin_rotation * x_adjustment * y_adjustment;


        transform.localRotation = Quaternion.Lerp(transform.localRotation, target_rotation, Time.deltaTime * smooth);
    }
}
