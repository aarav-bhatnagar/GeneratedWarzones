using UnityEngine;
using UnityEngine.Animations.Rigging;
using Cinemachine;
using Unity.Netcode;

public class CharacterAiming : NetworkBehaviour
{
    [SerializeField] private float TurnSpeed = 15f;
    public Camera playerCamera;

    [SerializeField] private Transform cameraLookAt;

    public Cinemachine.AxisState xAxis;
    public Cinemachine.AxisState yAxis;
    [SerializeField] private Rig aimLayer;

    [SerializeField] private float AimingCharacterDistance;
    private float DefaultCharacterDistance;
    private CinemachineVirtualCamera cinemachine;
    private CinemachineComponentBase componentBase;
    public bool isAiming;
    private CharacterLocomotion characterLocomotion;
    

    private void Start()
    {

        playerCamera = Camera.main;
        cinemachine = GetComponentInChildren<CinemachineVirtualCamera>();
        componentBase = cinemachine.GetCinemachineComponent(CinemachineCore.Stage.Body);
        DefaultCharacterDistance = (componentBase as Cinemachine3rdPersonFollow).CameraDistance;
        characterLocomotion = GetComponent<CharacterLocomotion>();
    }


    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetButtonDown("Fire2") && isAiming == false && !characterLocomotion.isSprinting)
        {
            isAiming = true;

            (componentBase as Cinemachine3rdPersonFollow).CameraDistance = AimingCharacterDistance;

        }
        else if (Input.GetButtonUp("Fire2") && isAiming == true)
        {
            isAiming = false;

            (componentBase as Cinemachine3rdPersonFollow).CameraDistance = DefaultCharacterDistance;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (playerCamera == null) return;
        xAxis.Update(Time.fixedDeltaTime);
        yAxis.Update(Time.fixedDeltaTime);

        cameraLookAt.eulerAngles = new Vector3(yAxis.Value, xAxis.Value, 0);

        float yawCamera = playerCamera.transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, yawCamera, 0), TurnSpeed * Time.deltaTime);
    }

}
