using UnityEngine;

/// <summary>
/// Represents a single hexagonal cell in the grid.
/// </summary>
public class HexCell : MonoBehaviour
{
    public HexCoordinates Coordinates { get; private set; }
    public TerrainType Terrain { get; private set; }
    public Unit OccupyingUnit { get; set; }
    public bool HasResourceNode { get; set; }
    public int OwnerPlayerID { get; set; } = -1; // -1 = neutral, 0/1 = player
    public bool IsBase { get; set; } // Is this cell a player base?

    private MeshRenderer meshRenderer;
    private Color originalColor;
    private Color highlightColor = new Color(1f, 1f, 0.5f, 1f);

    public void Initialize(HexCoordinates coordinates, TerrainType terrain)
    {
        Coordinates = coordinates;
        Terrain = terrain;

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalColor = Terrain.GetTerrainColor();
            meshRenderer.material.color = originalColor;
        }
    }

    public void SetTerrain(TerrainType terrain)
    {
        Terrain = terrain;
        if (meshRenderer != null)
        {
            originalColor = Terrain.GetTerrainColor();
            meshRenderer.material.color = originalColor;
        }
    }

    public void Highlight(bool highlight)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = highlight ? highlightColor : originalColor;
        }
    }

    public bool IsOccupied()
    {
        return OccupyingUnit != null;
    }

    public bool IsPassable()
    {
        return Terrain != TerrainType.Water && !IsOccupied();
    }

    public float GetMovementCost()
    {
        return Terrain.GetMovementCost();
    }

    public int GetDefenseBonus()
    {
        return Terrain.GetDefenseBonus();
    }

    private void OnMouseEnter()
    {
        Highlight(true);
    }

    private void OnMouseExit()
    {
        Highlight(false);
    }
}
