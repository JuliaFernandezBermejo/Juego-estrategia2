using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the hexagonal grid and all cells.
/// </summary>
public class HexGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float hexSize = 1f;
    [SerializeField] private GameObject hexCellPrefab;

    private Dictionary<HexCoordinates, HexCell> cells = new Dictionary<HexCoordinates, HexCell>();

    public int Width => gridWidth;
    public int Height => gridHeight;
    public bool IsGridReady { get; private set; }

    void Start()
    {
        CreateGrid();
    }

    private void CreateGrid()
    {
        // Create prefab if it doesn't exist
        if (hexCellPrefab == null)
        {
            hexCellPrefab = CreateHexPrefab();
        }

        // Generate grid using simple 0-based coordinates
        // Visual offset is handled in HexCoordinates.ToWorldPosition()
        for (int r = 0; r < gridHeight; r++)
        {
            for (int q = 0; q < gridWidth; q++)
            {
                CreateCell(q, r);
            }
        }

        GenerateRandomTerrain();
        IsGridReady = true;
        Debug.Log($"HexGrid created: {gridWidth}x{gridHeight} = {cells.Count} cells");
    }

    private void CreateCell(int q, int r)
    {
        HexCoordinates coords = new HexCoordinates(q, r);
        Vector3 position = coords.ToWorldPosition(hexSize);

        // Debug: Log first cell creation
        if (q == 0 && r == 0)
        {
            Debug.Log($"[DEBUG] Creating first cell at ({q}, {r})");
            Debug.Log($"[DEBUG]   World position: {position}");
            Debug.Log($"[DEBUG]   HexGrid transform: {transform.name}");
        }

        GameObject cellObject = Instantiate(hexCellPrefab, position, Quaternion.Euler(0, 30, 0), transform);
        cellObject.name = $"Hex {coords}";
        cellObject.SetActive(true); // Activate instantiated cell (prefab is inactive)

        // Debug: Check parenting
        if (q == 0 && r == 0)
        {
            Debug.Log($"[DEBUG]   Cell created: {cellObject.name}");
            Debug.Log($"[DEBUG]   Cell parent: {(cellObject.transform.parent != null ? cellObject.transform.parent.name : "NULL")}");
            Debug.Log($"[DEBUG]   Cell active: {cellObject.activeInHierarchy}");
            Debug.Log($"[DEBUG]   Cell has MeshRenderer: {cellObject.GetComponent<MeshRenderer>() != null}");
            Debug.Log($"[DEBUG]   Cell has MeshFilter: {cellObject.GetComponent<MeshFilter>() != null}");
            MeshFilter mf = cellObject.GetComponent<MeshFilter>();
            if (mf != null)
            {
                Debug.Log($"[DEBUG]   Mesh assigned: {mf.mesh != null}");
                if (mf.mesh != null)
                {
                    Debug.Log($"[DEBUG]   Mesh vertex count: {mf.mesh.vertexCount}");
                }
            }
        }

        HexCell cell = cellObject.GetComponent<HexCell>();
        if (cell == null)
        {
            cell = cellObject.AddComponent<HexCell>();
        }

        cell.Initialize(coords, TerrainType.Plains);

        // Create colored border for this cell
        CreateBorderForCell(cellObject, TerrainType.Plains);

        // Debug: Check color after initialization
        if (q == 0 && r == 0)
        {
            MeshRenderer mr = cellObject.GetComponent<MeshRenderer>();
            if (mr != null && mr.material != null)
            {
                Debug.Log($"[DEBUG]   Material color: {mr.material.color}");
            }
        }

        cells[coords] = cell;
    }

    private void CreateBorderForCell(GameObject cellObject, TerrainType terrain)
    {
        // Create border hexagon larger for outline effect
        GameObject border = new GameObject("Border");
        border.transform.parent = cellObject.transform;
        border.transform.localPosition = new Vector3(0, -0.02f, 0); // Further below main hex
        border.transform.localRotation = Quaternion.identity;
        border.transform.localScale = new Vector3(1.08f, 1f, 1.08f); // 8% larger for visible border

        MeshFilter borderMF = border.AddComponent<MeshFilter>();
        MeshRenderer borderMR = border.AddComponent<MeshRenderer>();

        borderMF.mesh = HexMesh.CreateHexagonMesh();

        Material borderMat = new Material(Shader.Find("Unlit/Color"));
        Color terrainColor = terrain.GetTerrainColor();
        borderMat.color = terrainColor * 0.3f; // Much darker (30% instead of 50%)
        borderMR.material = borderMat;
    }

    private GameObject CreateHexPrefab()
    {
        GameObject prefab = new GameObject("HexCell");

        // Add mesh components
        MeshFilter meshFilter = prefab.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = prefab.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = prefab.AddComponent<MeshCollider>();

        // Create hexagon mesh
        meshFilter.mesh = HexMesh.CreateHexagonMesh();
        meshCollider.sharedMesh = meshFilter.mesh;

        // Create simple material (Unlit/Color works in all render pipelines)
        Material material = new Material(Shader.Find("Unlit/Color"));
        meshRenderer.material = material;

        // Deactivate prefab so it doesn't appear in scene (only used as template)
        prefab.SetActive(false);

        return prefab;
    }

    private void GenerateRandomTerrain()
    {
        foreach (var cell in cells.Values)
        {
            float random = Random.value;

            if (random < 0.1f)
                cell.SetTerrain(TerrainType.Water);
            else if (random < 0.3f)
                cell.SetTerrain(TerrainType.Mountain);
            else if (random < 0.6f)
                cell.SetTerrain(TerrainType.Forest);
            else
                cell.SetTerrain(TerrainType.Plains);
        }
    }

    public HexCell GetCell(HexCoordinates coordinates)
    {
        cells.TryGetValue(coordinates, out HexCell cell);
        return cell;
    }

    public HexCell GetCell(int q, int r)
    {
        return GetCell(new HexCoordinates(q, r));
    }

    public List<HexCell> GetNeighbors(HexCoordinates coordinates)
    {
        List<HexCell> neighbors = new List<HexCell>();

        for (int i = 0; i < 6; i++)
        {
            HexCoordinates neighborCoords = HexCoordinates.GetNeighbor(coordinates, i);
            HexCell neighbor = GetCell(neighborCoords);
            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    public List<HexCell> GetCellsInRange(HexCoordinates center, int range)
    {
        List<HexCell> cellsInRange = new List<HexCell>();

        foreach (var cell in cells.Values)
        {
            int distance = HexCoordinates.Distance(center, cell.Coordinates);
            if (distance <= range)
            {
                cellsInRange.Add(cell);
            }
        }

        return cellsInRange;
    }

    public HexCell GetCellFromWorldPosition(Vector3 worldPosition)
    {
        // Convert world position to hex coordinates (approximate)
        // This is a simplified conversion
        float q = (worldPosition.x * 2f / 3f) / hexSize;
        float r = (-worldPosition.x / 3f + Mathf.Sqrt(3f) / 3f * worldPosition.z) / hexSize;

        int qi = Mathf.RoundToInt(q);
        int ri = Mathf.RoundToInt(r);

        return GetCell(qi, ri);
    }

    public Dictionary<HexCoordinates, HexCell> GetAllCells()
    {
        return cells;
    }
}
