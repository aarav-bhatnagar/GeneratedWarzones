using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GeometryGrassPainter : MonoBehaviour
{
    private Mesh mesh;
    private MeshFilter filter;

    public Color AdjustedColor;

    [Range(1, 600000)]
    public int grassLimit = 50000;

    [SerializeField] private List<Vector3> positions = new List<Vector3>();
    [SerializeField] private List<Color> colors = new List<Color>();
    [SerializeField] private List<int> indices = new List<int>();
    [SerializeField] private List<Vector3> normals = new List<Vector3>();
    [SerializeField] private List<Vector2> length = new List<Vector2>();

    public float sizeWidth = 1f;
    public float sizeLength = 1f;
    public float density = 1f;

    public float rangeR, rangeG, rangeB;
    public LayerMask hitMask = 1;
    public LayerMask paintMask = 1;

    public float heightMin = 0;
    public float heightMax = 10;

    public float noiseVariationLevel = 0.15f;
    public float noiseScale = 10f;
    public Vector2 noiseOffset = Vector2.zero;
    public float noiseThreshold = 0.5f;

    private bool isGenerated = false;
    private int grassCount = 0;

    private void Start()
    {
        filter = GetComponent<MeshFilter>();
        GenerateGrass();
    }

    public void ClearMesh()
    {
        grassCount = 0;
        positions.Clear();
        indices.Clear();
        colors.Clear();
        normals.Clear();
        length.Clear();

        if (filter != null && filter.mesh != null)
        {
            filter.mesh.Clear();
        }
    }


    public void GenerateGrass()
    {
        return;
        if (isGenerated) return;
        isGenerated = true;

        ClearMesh();

        // Find required components
        GameLoader gameLoader = FindObjectOfType<GameLoader>();
        if (gameLoader == null)
        {
            Debug.LogError("[Grass] GameLoader not found in scene");
            return;
        }

        MeshCollider targetMeshCollider = gameLoader.GetNewMeshCollider();
        if (targetMeshCollider == null)
        {
            Debug.LogError("[Grass] Target MeshCollider is null");
            return;
        }

        // Get object bounds if available
        List<Bounds> spawnedObjectBounds = new List<Bounds>();
        ObjectSpawner spawner = FindObjectOfType<ObjectSpawner>();
        if (spawner != null)
        {
            spawnedObjectBounds = spawner.spawnedObjectBounds;
        }

        // Get mesh data
        Mesh targetMesh = targetMeshCollider.sharedMesh;
        if (targetMesh == null)
        {
            Debug.LogError("[Grass] Target mesh is missing");
            return;
        }

        Vector3[] vertices = targetMesh.vertices;
        Vector3[] targetNormals = targetMesh.normals;
        int vertexCount = vertices.Length;

        // Pre-calculate world positions
        Vector3[] worldPositions = new Vector3[vertexCount];
        for (int i = 0; i < vertexCount; i++)
        {
            worldPositions[i] = targetMeshCollider.transform.TransformPoint(vertices[i]);
        }

        // Place grass
        for (int k = 0; k < vertexCount; k++)
        {
            if (grassCount >= grassLimit) break;

            Vector3 worldPos = worldPositions[k];

            // Skip based on height
            if (worldPos.y < heightMin || worldPos.y > heightMax) continue;

            // Skip positions inside objects
            bool insideObject = false;
            foreach (Bounds bounds in spawnedObjectBounds)
            {
                if (bounds.Contains(worldPos))
                {
                    insideObject = true;
                    break;
                }
            }
            if (insideObject) continue;

            // Apply noise-based placement
            float noiseValue = Mathf.PerlinNoise(
                (worldPos.x + noiseOffset.x) / noiseScale,
                (worldPos.z + noiseOffset.y) / noiseScale
            );

            float threshold = noiseThreshold + Random.Range(-noiseVariationLevel, noiseVariationLevel);
            if (noiseValue >= threshold && Random.value < density)
            {
                AddGrassBlade(worldPos, targetNormals[k]);
            }
        }

        UpdateMesh();
        Debug.Log($"[Grass] Generated {grassCount} blades");
    }

    private void AddGrassBlade(Vector3 worldPosition, Vector3 normal)
    {
        Vector3 localPos = worldPosition - transform.position;

        positions.Add(localPos);
        indices.Add(grassCount);
        length.Add(new Vector2(sizeWidth, sizeLength));

        colors.Add(new Color(
            AdjustedColor.r + (Random.Range(0f, 1f) * rangeR),
            AdjustedColor.g + (Random.Range(0f, 1f) * rangeG),
            AdjustedColor.b + (Random.Range(0f, 1f) * rangeB),
            1f
        ));

        normals.Add(normal);
        grassCount++;
    }

    private void UpdateMesh()
    {
        if (filter == null)
        {
            filter = GetComponent<MeshFilter>();
            if (filter == null) return;
        }

        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "GeneratedGrassMesh";
        }
        else
        {
            mesh.Clear();
        }

        mesh.SetVertices(positions);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Points, 0);
        mesh.SetUVs(0, length);
        mesh.SetColors(colors);
        mesh.SetNormals(normals);

        filter.mesh = mesh;
    }
}