using UnityEngine;

public class Recoil : MonoBehaviour
{
    /*
    private Vector3 curentRotation;
    private Vector3 targetRotation;

    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;

    [SerializeField] private float aim_recoilX;
    [SerializeField] private float aim_recoilY;
    [SerializeField] private float aim_recoilZ;

    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    [SerializeField] private Gun Gun_Script;

    private bool isAiming;
    void Start()
    {
        
    }

    void Update()
    {
        isAiming = Gun_Script.isAiming;

        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        curentRotation = Vector3.Slerp(curentRotation, targetRotation, snappiness * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(curentRotation);
    }

    public void RecoilFire()
    {
        if (isAiming == true)
        {
            targetRotation = new Vector3(aim_recoilX, Random.Range(-aim_recoilY, aim_recoilY), Random.Range(-aim_recoilZ, aim_recoilZ));
        }
        else
        {
            targetRotation = new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
        }
    }
    */
}
