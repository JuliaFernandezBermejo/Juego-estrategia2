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

        // Generate grid using axial coordinates
        // For a rectangular-shaped hex grid
        for (int r = 0; r < gridHeight; r++)
        {
            int qOffset = r / 2; // Offset for odd rows
            for (int q = -qOffset; q < gridWidth - qOffset; q++)
            {
                CreateCell(q, r);
            }
        }

        GenerateRandomTerrain();
    }

    private void CreateCell(int q, int r)
    {
        HexCoordinates coords = new HexCoordinates(q, r);
        Vector3 position = coords.ToWorldPosition(hexSize);

        GameObject cellObject = Instantiate(hexCellPrefab, position, Quaternion.identity, transform);
        cellObject.name = $"Hex {coords}";

        HexCell cell = cellObject.GetComponent<HexCell>();
        if (cell == null)
        {
            cell = cellObject.AddComponent<HexCell>();
        }

        cell.Initialize(coords, TerrainType.Plains);
        cells[coords] = cell;
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

        // Create simple material
        Material material = new Material(Shader.Find("Standard"));
        meshRenderer.material = material;

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
