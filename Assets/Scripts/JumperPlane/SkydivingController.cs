using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using TMPro;

public class SkydivingController : NetworkBehaviour
{
    private CharacterLocomotion characterLocomotion;
    public PlaneController planeController;
    private bool jumpedFromPlane = false;
    [SerializeField] private GameObject mesh;
    [SerializeField] private GameObject root;

    private float timeToAutoJump = 32f; // Time before automatic jump
    public TMP_Text timerText;

    private float remainingTime;

    [SerializeField] private GameObject Arrow;
    [SerializeField] private GameObject playerToZone;
    [SerializeField] private GameObject jumpDisplay;

    private void OnEnable()
    {
        characterLocomotion = GetComponent<CharacterLocomotion>();
        planeController = FindFirstObjectByType<PlaneController>();
        timerText.gameObject.SetActive(true);
        mesh.SetActive(false);
        root.SetActive(false);
        Arrow.SetActive(false);
        playerToZone.SetActive(false);

        remainingTime = timeToAutoJump;


    }

    private void Update()
    {
        if (!IsOwner) return;

        if (!jumpedFromPlane)
        {
            remainingTime -= Time.deltaTime;
            timerText.text = "Time Remaining " + Mathf.Max(remainingTime, 0).ToString("F2") + "s To Jump!";

            if (remainingTime <= 0)
            {
                AutoJump();
                timerText.gameObject.SetActive(false);
            }


            if (Input.GetKeyDown(KeyCode.Space) && planeController.isAllowedToJump.Value)
            {
                PerformJump();
                timerText.gameObject.SetActive(false);
            }
        }
    }


    private void AutoJump()
    {
        timerText.text = "AUTO-JUMPING";
        PerformJump();
    }

    private void PerformJump()
    {
        ResetPlayerPositionServerRpc();
        jumpedFromPlane = true;
        Camera.main.gameObject.AddComponent<Cinemachine.CinemachineBrain>();
    }

    [ServerRpc]
    private void ResetPlayerPositionServerRpc()
    {
        ResetPlayerPositionClientRpc();

    }

    [ClientRpc]
    private void ResetPlayerPositionClientRpc()
    {

        transform.localPosition = transform.localPosition + new Vector3(0, -5, 0);
        transform.SetParent(null);

        characterLocomotion.SetMovementEnabled(true);

        mesh.SetActive(true);
        root.SetActive(true);
        Arrow.SetActive(true);
        playerToZone.SetActive(true);
    }

}
