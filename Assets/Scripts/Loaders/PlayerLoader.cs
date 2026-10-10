using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.SceneManagement;
using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Animations.Rigging;
using System.Collections;

public class PlayerLoader : NetworkBehaviour
{
    private RoomScreen _roomScreen;
    public GameObject Canvas;
    public GameObject minimapCamera;
    private ZoneDamage zoneDamagecs;
    private PlayerToZone playerToZonecs;
    private ZoneMinimap zoneMinimap;
    private bool firstSwitch;
    private ZoneUI zoneUIcs;
    private ActiveWeapon activeWeapon;
    private AimLookAt aimLookAt;
    private CrossHairTarget crossHairTarget;
    //private RigBuilder rigBuilder;

    private CharacterLocomotion characterLocomotion;

    [SerializeField] private Transform mesh;
    [SerializeField] private Transform headBone;
    [SerializeField] private CinemachineVirtualCamera playerCineCamera;

    private CharacterAiming characterAiming;
    private SkydivingController skydivingController;
    private GameLoader gameLoader;
    private bool inGame;
    public static bool inLobby = true;



    // Network- synchronized skin index
    private NetworkVariable<int> currentSkinIndex = new NetworkVariable<int>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkList<int> outfits;

    private void Awake()
    {
        outfits = new NetworkList<int>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        SceneManager.sceneLoaded += OnSceneLoaded;
        firstSwitch = false;
        characterLocomotion = GetComponent<CharacterLocomotion>();
        characterAiming = GetComponent<CharacterAiming>();
        skydivingController = GetComponent<SkydivingController>();
        activeWeapon = GetComponent<ActiveWeapon>();
        aimLookAt = GetComponentInChildren<AimLookAt>();
        //rigBuilder = GetComponent<RigBuilder>();

        zoneDamagecs = GetComponent<ZoneDamage>();
        playerToZonecs = GetComponentInChildren<PlayerToZone>();
        zoneMinimap = GetComponentInChildren<ZoneMinimap>();
        zoneUIcs = GetComponentInChildren<ZoneUI>();
        crossHairTarget = GetComponentInChildren<CrossHairTarget>();

        // Subscribe to skin changes
        currentSkinIndex.OnValueChanged += OnSkinIndexChanged;
        outfits.OnListChanged += OnOutfitsChanged;
    }

    public override void OnNetworkSpawn()
    {

        //rigBuilder.Build();

        // Initial sync for late-joining clients
        if (outfits.Count >= 5)
        {
            UpdateLocalOutfit(new int[] {
                outfits[0], outfits[1],
                outfits[2], outfits[3],
                outfits[4]
            });
        }

        // Common setup
        playerToZonecs.gameObject.SetActive(false);
        zoneMinimap.gameObject.SetActive(false);
        zoneUIcs.gameObject.SetActive(false);

        // Handle skin sync FIRST
        UpdateSkin(currentSkinIndex.Value);

        if (!IsOwner)
        {
            Canvas.SetActive(false);
            minimapCamera.SetActive(false);
            zoneMinimap.gameObject.SetActive(false);
            playerCineCamera.gameObject.SetActive(false);
            //rigBuilder.enabled = false;
            return;
        }

        // Owner-specific setup
        minimapCamera.SetActive(true);
        playerCineCamera.gameObject.SetActive(true);
        _roomScreen = FindFirstObjectByType<RoomScreen>();
        zoneDamagecs.enabled = false;
        ChangePlayerScreen();

        if (!IsOwner) return;
        if (IsServer)
        {
            currentSkinIndex.Value = Customizer.currentSkin;

            SetSkinServerRpc(Customizer.currentSkin);
            UpdateOutfitServerRpc(Customizer.storedOutfits);
        }
        else
        {
            SetSkinServerRpc(Customizer.currentSkin);
            UpdateOutfitServerRpc(Customizer.storedOutfits);
        }

    }


    private void OnSkinIndexChanged(int previous, int current)
    {
        UpdateSkin(current);
    }

    private void OnOutfitsChanged(NetworkListEvent<int> changeEvent)
    {
        // Handle ALL clients (including host)
        if (outfits.Count >= 5)
        {
            UpdateLocalOutfit(new int[] {
                outfits[0], outfits[1],
                outfits[2], outfits[3],
                outfits[4]
            });
        }
    }

    private void UpdateLocalOutfit(int[] outfitList)
    {
        foreach (Transform child in headBone)
        {
            Destroy(child.gameObject);
        }

        // Validate indices before instantiating
        // Hat

        Debug.Log($"Updating outfit for {gameObject.name}. Outfit IDs: {string.Join(", ", outfitList)}");

        if (Customizer.instance.hats[outfitList[1]] != null)
        {
            print("Hat");
            var hat = Instantiate(Customizer.instance.hats[outfitList[1]], headBone);
            //hat.AddComponent<RemoveOutfit>();
        }

        // Male Hair
        if (Customizer.instance.maleHair[outfitList[2]] != null && outfitList[0] == 1)
        {
            Instantiate(Customizer.instance.maleHair[outfitList[2]], headBone);
        }

        // Female Hair
        if (Customizer.instance.femaleHair[outfitList[3]] != null && outfitList[0] == 2)
        {
            Instantiate(Customizer.instance.femaleHair[outfitList[3]], headBone);
        }

        // Facial Hair
        if (Customizer.instance.facialHair[outfitList[4]] != null && outfitList[0] == 1)
        {
            Instantiate(Customizer.instance.facialHair[outfitList[4]], headBone);
        }
    }

    [ServerRpc]
    private void SetSkinServerRpc(int newSkinIndex)
    {
        // Validate skin index
        newSkinIndex = Mathf.Clamp(newSkinIndex, 0, mesh.childCount - 1);
        currentSkinIndex.Value = newSkinIndex;
    }
    private void UpdateSkin(int skinIndex)
    {
        for (int i = 0; i < mesh.childCount; i++)
        {
            mesh.GetChild(i).gameObject.SetActive(i == skinIndex);
        }
    }


    [ServerRpc]
    public void UpdateOutfitServerRpc(int[] newOutfit)
    {
        if (newOutfit.Length < 5) return;

        outfits.Clear();
        foreach (var item in newOutfit)
        {
            outfits.Add(item);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 2)
        {
            activeWeapon.ResetAllWeapons();
            Camera mainCamera = Camera.main;
            PlayerLoader.inLobby = false;
            gameLoader = FindFirstObjectByType<GameLoader>();
            inGame = true;
            skydivingController.enabled = true;
            characterAiming.playerCamera = mainCamera;
            zoneDamagecs.CameraPos = mainCamera.transform;
            activeWeapon.CrossHairTarget = FindFirstObjectByType<CrossHairTarget>().transform;
            activeWeapon._camera = mainCamera;
            aimLookAt.cameraTransform = mainCamera.transform;
            crossHairTarget.playerCamera = mainCamera;
            playerToZonecs.gameObject.SetActive(false);
            foreach (WeaponRecoil weaponRecoil in transform.GetComponentsInChildren<WeaponRecoil>())
            {
                weaponRecoil._camera = mainCamera;
            }

            if (!Canvas.activeInHierarchy)
            {
                ChangePlayerScreen();
            }

            zoneDamagecs.enabled = true;
            if (IsOwner)
            {
                zoneMinimap.gameObject.SetActive(true);
                zoneMinimap.zoneWallcs = FindAnyObjectByType<ZoneWall>();
            }
            zoneUIcs.gameObject.SetActive(true);

            
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        currentSkinIndex.OnValueChanged -= OnSkinIndexChanged;
        outfits.OnListChanged -= OnOutfitsChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private bool Parented;

    [ServerRpc]
    private void ParentServerRpc()
    {
        characterLocomotion.SetMovementEnabled(false);
        GameObject playerholder = FindFirstObjectByType<JumperPlanePlayerHolder>().gameObject;
        transform.SetParent(playerholder.transform);
        zoneUIcs.jumperPlanePlayerHolder = playerholder.transform;
        FindFirstObjectByType<ZoneWall>().jumperPlayerPlaneHolder = playerholder.transform;
        transform.localPosition = Vector3.zero;
        ParentClientRpc();
    }

    [ClientRpc]
    private void ParentClientRpc()
    {
        print("ClientRPC");
        characterLocomotion.SetMovementEnabled(false);
        GameObject playerholder = FindFirstObjectByType<JumperPlanePlayerHolder>().gameObject;
        transform.SetParent(playerholder.transform);
        zoneUIcs.jumperPlanePlayerHolder = playerholder.transform;
        FindFirstObjectByType<ZoneWall>().jumperPlayerPlaneHolder = playerholder.transform;
        transform.localPosition = Vector3.zero;
    }

    private void Update() 
    {



        if (!IsOwner) return;

        if (inGame)
        {
            if (gameLoader.CanParent.Value && Parented == false)
            {
                ParentServerRpc();
                Parented = true;
            }
        }



        if (_roomScreen != null)
        {
            if (_roomScreen.GetisReady() == true && firstSwitch == false)
            {
                ChangePlayerScreen();
                firstSwitch = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && firstSwitch == true)
        {
            _roomScreen.ChangeRoomScreen();
            ChangePlayerScreen();
        }
    }

    public void ChangePlayerScreen()
    {
        if (!IsOwner) return;
        bool isActive = !Canvas.activeInHierarchy;
        Canvas.SetActive(isActive);
        characterLocomotion.SetMovementEnabled(isActive);
    }

    [SerializeField] private GameObject respawnMenu, GeneralUI, ZoneUI;

    public void RespawnCharacter()
    {
        // Start coroutine to wait before respawning.
        StartCoroutine(WaitAndRespawn());
    }

    [ServerRpc(RequireOwnership = false)]
    private void RespawnCharacterServerRpc()
    {
        
        ZoneWall zoneWall = FindFirstObjectByType<ZoneWall>();
        Vector3 zoneWallPosition = zoneWall.transform.position;
        Vector3 newPos = RandomPointInCircle(new Vector3(zoneWallPosition.x, 60, zoneWallPosition.z), (400 * zoneWall.transform.localScale.x) - 5);
        // After setting position, trigger client-side respawn.
        RespawnCharacterClientRpc(newPos);

    }

    [ClientRpc]
    private void RespawnCharacterClientRpc(Vector3 newPos)
    {
        characterLocomotion.SetMovementEnabled(false);
        transform.position = newPos;
        characterLocomotion.SetMovementEnabled(true);
        activeWeapon.ResetAllWeapons();
        // Ensure you reset any other components that might need to be reset when the character respawns.
    }

    IEnumerator WaitAndRespawn()
    {
        // Wait for a bit before respawning the character.
        yield return new WaitForSeconds(0.1f);
        print("Waiting Done");

        // Trigger server-side respawn.
        RespawnCharacterServerRpc();

    }

    private Vector3 RandomPointInCircle(Vector3 center, float maxDistance)
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        float distance = Random.Range(0f, maxDistance);
        return new Vector3(
            center.x + distance * Mathf.Cos(angle),
            center.y,
            center.z + distance * Mathf.Sin(angle)
        );
    }


}