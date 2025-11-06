using TMPro;
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
    private GameObject resourceLabel;

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

            // Update border color to match terrain
            Transform borderTransform = transform.Find("Border");
            if (borderTransform != null)
            {
                MeshRenderer borderRenderer = borderTransform.GetComponent<MeshRenderer>();
                if (borderRenderer != null)
                {
                    // Use darker version of terrain color for border
                    borderRenderer.material.color = originalColor * 0.3f;
                }
            }
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

    public bool IsPassableForPlayer(int playerID)
    {
        // Water is always impassable
        if (Terrain == TerrainType.Water)
            return false;

        // Friendly bases are impassable (can't walk on your own base)
        // Enemy bases are passable (to capture them)
        if (IsBase && OwnerPlayerID == playerID)
            return false;

        // Regular occupied cells are impassable
        if (IsOccupied())
            return false;

        return true;
    }

    public float GetMovementCost()
    {
        return Terrain.GetMovementCost();
    }

    private void OnMouseEnter()
    {
        Highlight(true);
    }

    private void OnMouseExit()
    {
        Highlight(false);
    }

    public void CreateResourceLabel()
    {
        GameObject labelObj = new GameObject("ResourceLabel");
        labelObj.transform.position = transform.position + Vector3.up * 0.5f;
        labelObj.transform.SetParent(transform);

        TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
        tmp.text = "+10";
        tmp.fontSize = 5;
        tmp.color = new Color(0.8f, 0.7f, 0f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.transform.rotation = Quaternion.Euler(90, 0, 0);

        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = Color.black;

        resourceLabel = labelObj;
    }

    public void DestroyResourceLabel()
    {
        if (resourceLabel != null)
        {
            Destroy(resourceLabel);
            resourceLabel = null;
        }
    }
}
