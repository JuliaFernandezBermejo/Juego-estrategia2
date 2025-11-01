using UnityEngine;

/// <summary>
/// Production building (base) that can create units.
/// </summary>
public class ProductionBuilding : MonoBehaviour
{
    public int OwnerPlayerID { get; private set; }
    public HexCell Cell { get; private set; }

    private GameManager gameManager;

    public void Initialize(int ownerPlayerID, HexCell cell, GameManager manager)
    {
        OwnerPlayerID = ownerPlayerID;
        Cell = cell;
        gameManager = manager;
    }

    public bool CanProduceUnit(UnitStats stats)
    {
        if (gameManager == null)
            return false;

        int resources = gameManager.GetPlayerResources(OwnerPlayerID);
        return resources >= stats.cost;
    }

    public void ProduceUnit(UnitStats stats, HexGrid hexGrid)
    {
        if (!CanProduceUnit(stats))
        {
            Debug.LogWarning($"Not enough resources to produce {stats.unitName}");
            return;
        }

        // Find empty cell adjacent to base
        var neighbors = hexGrid.GetNeighbors(Cell.Coordinates);

        foreach (var neighbor in neighbors)
        {
            if (neighbor.IsPassable())
            {
                // Deduct resources
                int currentResources = gameManager.GetPlayerResources(OwnerPlayerID);
                // Note: GameManager needs a method to deduct resources

                // Spawn unit
                gameManager.SpawnUnit(stats, OwnerPlayerID, neighbor);

                Debug.Log($"Player {OwnerPlayerID} produced {stats.unitName}");
                return;
            }
        }

        Debug.LogWarning("No empty cell to produce unit!");
    }
}
