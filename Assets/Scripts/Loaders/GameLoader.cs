using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class GameLoader : NetworkBehaviour
{
    private MapGenerator mapGenerator;
    private GeometryGrassPainter grassPainter;
    [SerializeField] private GameObject mesh;
    private MeshCollider newMeshCollider;
    public NoiseData noiseData;
    private ZoneWall zoneWall;

    private NetworkVariable<int> seed = new NetworkVariable<int>();
    private NetworkVariable<Vector2> offset = new NetworkVariable<Vector2>();
    private NetworkVariable<float> angle = new NetworkVariable<float>();

    private ObjectSpawner objectSpawner;

    private PlaneController planeController;

    [SerializeField] private GameObject jumperPlanePlayerHolder;
    public NetworkVariable<bool> CanParent = new NetworkVariable<bool>();

    public NetworkVariable<int> maxKills = new NetworkVariable<int>(0);
    public NetworkVariable<bool> Disconnect = new NetworkVariable<bool>();

    public static GameLoader Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public override void OnNetworkSpawn()
    {
        mapGenerator = FindFirstObjectByType<MapGenerator>();
        grassPainter = FindFirstObjectByType<GeometryGrassPainter>();
        objectSpawner = FindFirstObjectByType<ObjectSpawner>();
        planeController = FindFirstObjectByType<PlaneController>();

        zoneWall = FindFirstObjectByType<ZoneWall>();

        if (IsOwner)
        {
            StartCoroutine(Spawn(jumperPlanePlayerHolder));
        }

        if (IsServer)
        {
            // Ensure ZonePositions is initialized
            if (zoneWall.ZonePositions == null || zoneWall.ZonePositions.Count == 0)
            {
                Debug.LogError("ZonePositions is not initialized.");
                return; // Prevent further execution if it's not set up
            }


            seed.Value = Random.Range(-30000, 30001);
            offset.Value = new Vector2(Random.Range(-3000, 3001), Random.Range(-3000, 3001));
            angle.Value = Random.Range(0, 360);

        }


        noiseData.seed = seed.Value;
        noiseData.offset = offset.Value;
        planeController.angle = angle.Value;


        StartCoroutine(SetupMeshColliderAndSpawn());
    }

    IEnumerator SetupMeshColliderAndSpawn()
    {
        Debug.Log("Starting MeshCollider setup...");

        mapGenerator.GenerateMesh();
        // Wait for MeshCollider to be added and fully processed
        yield return StartCoroutine(AddMeshColliderAndWait(mesh));

        Debug.Log("MeshCollider ready, spawning objects...");

        // Now proceed with the next steps
        if (IsOwner)
        {
            objectSpawner.SpawnObjects();

        }

        grassPainter.GenerateGrass();

        zoneWall.CalculateCentres();
    }

    bool Updated;
    private void Update()
    {
        if (zoneWall.centresCalculated.Value == true && Updated == false)
        {
            zoneWall.UpdateCentres();
            Updated = true;
        }
    }

    IEnumerator AddMeshColliderAndWait(GameObject mesh)
    {
        Debug.Log("Adding MeshCollider...");

        newMeshCollider = mesh.AddComponent<MeshCollider>();

        // Ensure there's a MeshFilter with a valid mesh
        MeshFilter meshFilter = mesh.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            newMeshCollider.sharedMesh = meshFilter.sharedMesh;
            Debug.Log("Assigned MeshFilter's mesh to MeshCollider.");
        }
        else
        {
            Debug.LogError("No MeshFilter found or MeshFilter has no mesh!");
        }

        // Wait for Unity to process the MeshCollider
        while (newMeshCollider.sharedMesh == null)
        {
            Debug.Log("Waiting for MeshCollider...");
            yield return null; // Wait until Unity assigns it
        }

        Debug.Log("MeshCollider is ready!");
    }

    /*
    [ServerRpc]
    private void AddMeshColliderServerRpc()
    {
        AddMeshColliderClientRpc();
    }

    [ClientRpc]

    private void AddMeshColliderClientRpc()
    {

        newMeshCollider = mesh.AddComponent<MeshCollider>();
    }
    */

    IEnumerator Spawn(GameObject parentPrefab)
    {
        if (CanParent.Value == false)
        {
            // Instantiate and spawn the parent
            NetworkObject parentObject = Instantiate(parentPrefab).GetComponent<NetworkObject>();
            parentObject.Spawn();

            // Wait until the parent is fully spawned
            yield return new WaitUntil(() => parentObject.IsSpawned);

            // Reparent the child
            CanParent.Value = true;
        }

    }

    public MeshCollider GetNewMeshCollider()
    {
        return newMeshCollider;
    }
}