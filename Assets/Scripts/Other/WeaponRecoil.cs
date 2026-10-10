using UnityEngine;
using Cinemachine;


public class WeaponRecoil : MonoBehaviour
{
    
    public Camera _camera;
    [SerializeField] private CharacterAiming characterAiming;
    [HideInInspector] public CinemachineImpulseSource cameraShake;
    [SerializeField] private float verticalRecoil = 7.5f;
    [SerializeField] private float minVerticalRecoilHeight;
    [SerializeField] private float maxHorizontalRecoil = 7.5f;
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private float recoilModifier;


    float time;

    private void Awake()
    {
        cameraShake = GetComponent<Cinemachine.CinemachineImpulseSource>();
    }
    public void GenerateRecoil()
    {
        time = duration;

        cameraShake.GenerateImpulse(_camera.transform.forward);

    }

    private void Update()
    {

        float mod = 1f;
        if (characterAiming.isAiming)
        {
             mod = recoilModifier;
        }

        if (time > 0)
        {
            if (characterAiming.yAxis.Value > minVerticalRecoilHeight)
            {
                characterAiming.yAxis.Value -= (((verticalRecoil / 10) * Time.deltaTime) / duration) * mod;
            }

            characterAiming.xAxis.Value -= (((Random.Range(-maxHorizontalRecoil, maxHorizontalRecoil) / 10) * Time.deltaTime) / duration) * mod;
            time -= Time.deltaTime;
            
        }

    }
    
}

