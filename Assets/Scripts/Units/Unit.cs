using UnityEngine;

/// <summary>
/// Represents a single unit on the battlefield.
/// </summary>
public class Unit : MonoBehaviour
{
    [Header("Unit Configuration")]
    [SerializeField] private HardcodedUnitStats stats;

    [Header("Runtime Data")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int remainingMovement;
    [SerializeField] private bool hasAttacked;
    [SerializeField] private bool hasMovedThisTurn;

    public HardcodedUnitStats Stats => stats;
    public int CurrentHealth => currentHealth;
    public int RemainingMovement => remainingMovement;
    public bool HasAttacked => hasAttacked;
    public bool HasMovedThisTurn => hasMovedThisTurn;
    public int OwnerPlayerID { get; private set; }
    public HexCell CurrentCell { get; private set; }

    // For AI
    public string CurrentOrder { get; set; } // Strategic order from AI
    public HexCell TargetCell { get; set; } // Target destination

    private MeshRenderer meshRenderer;
    private Color playerColor;
    private HexGrid hexGrid;

    public void Initialize(HardcodedUnitStats unitStats, int ownerPlayerID, HexCell startingCell, HexGrid grid)
    {
        stats = unitStats;
        OwnerPlayerID = ownerPlayerID;
        currentHealth = stats.maxHealth;
        hexGrid = grid;

        SetCell(startingCell);
        RefreshTurn();

        // Set visual appearance
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            playerColor = ownerPlayerID == 0 ? Color.blue : Color.red;
            // Create a unique material instance for this unit to prevent color sharing
            meshRenderer.material = new Material(meshRenderer.material);
            meshRenderer.material.color = playerColor;
        }
    }

    public void RefreshTurn()
    {
        remainingMovement = stats.movementPoints;
        hasAttacked = false;
        hasMovedThisTurn = false;
    }

    public void SetCell(HexCell cell)
    {
        // Clear previous cell
        if (CurrentCell != null)
        {
            CurrentCell.OccupyingUnit = null;
        }

        // Set new cell
        CurrentCell = cell;
        if (cell != null)
        {
            // Safety check: warn if overwriting another unit
            if (cell.OccupyingUnit != null && cell.OccupyingUnit != this)
            {
                Debug.LogError($"[CRITICAL BUG] SetCell is overwriting {cell.OccupyingUnit.Stats.unitName} (Player {cell.OccupyingUnit.OwnerPlayerID}) at {cell.Coordinates} with {stats.unitName} (Player {OwnerPlayerID})! This should never happen!");
            }

            cell.OccupyingUnit = this;
            transform.position = cell.transform.position + Vector3.up * 0.5f;
        }
    }

    public bool CanMoveTo(HexCell targetCell)
    {
        if (targetCell == null)
        {
            Debug.LogError($"Cannot move {stats.unitName}: Target cell is null");
            return false;
        }

        if (hexGrid == null)
        {
            Debug.LogError($"Cannot move {stats.unitName}: HexGrid reference is null");
            return false;
        }

        // Use Dijkstra pathfinding to find the actual path
        DijkstraPathfinding pathfinding = new DijkstraPathfinding(hexGrid);
        System.Collections.Generic.List<HexCell> path = pathfinding.FindPath(CurrentCell, targetCell, this);

        if (path == null || path.Count == 0)
        {
            Debug.LogError($"Cannot move {stats.unitName} to {targetCell.Coordinates}: No valid path exists");
            return false;
        }

        // Calculate total cost by summing each hex in the path (skip starting hex at index 0)
        float totalCost = 0;

        for (int i = 0; i < path.Count; i++)
        {
            float cellCost = path[i].GetMovementCost();
            if (i > 0) // Skip starting hex
            {
                totalCost += cellCost;
            }
        }

        if (remainingMovement < Mathf.CeilToInt(totalCost))
        {
            Debug.LogError($"Cannot move {stats.unitName} to {targetCell.Coordinates}: Insufficient movement points\n" +
                           $"  - Path length: {path.Count - 1} hex(es) (excluding start)\n" +
                           $"  - Total movement cost: {totalCost:F1}\n" +
                           $"  - Available: {remainingMovement} / Required: {Mathf.CeilToInt(totalCost)}");
            return false;
        }

        // Check if destination cell is occupied
        if (targetCell.IsOccupied())
        {
            Debug.LogError($"Cannot move {stats.unitName} to {targetCell.Coordinates}: Cell is occupied by {targetCell.OccupyingUnit.Stats.unitName}");
            return false;
        }

        return true;
    }

    public void MoveTo(HexCell targetCell)
    {
        // Validation checks
        if (targetCell == null)
        {
            Debug.LogWarning($"Cannot move {stats.unitName}: Target cell is null");
            return;
        }

        if (hexGrid == null)
        {
            Debug.LogWarning($"Cannot move {stats.unitName}: HexGrid reference is null");
            return;
        }

        // Check if destination cell is occupied
        if (targetCell.IsOccupied())
        {
            Debug.LogWarning($"Cannot move {stats.unitName} to {targetCell.Coordinates}: Cell is occupied by {targetCell.OccupyingUnit.Stats.unitName}");
            return;
        }

        // Find path (only calculated once)
        DijkstraPathfinding pathfinding = new DijkstraPathfinding(hexGrid);
        System.Collections.Generic.List<HexCell> path = pathfinding.FindPath(CurrentCell, targetCell, this);

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning($"Cannot move {stats.unitName} to {targetCell.Coordinates}: No valid path exists");
            return;
        }

        // Calculate total cost for the entire path (skip starting hex at index 0)
        float totalPathCost = 0;
        for (int i = 1; i < path.Count; i++)
        {
            totalPathCost += path[i].GetMovementCost();
        }

        // Round once at the end
        int totalCost = Mathf.CeilToInt(totalPathCost);

        // Check if enough movement points
        if (remainingMovement < totalCost)
        {
            Debug.LogWarning($"Cannot move {stats.unitName} to {targetCell.Coordinates}: Insufficient movement points (need {totalCost}, have {remainingMovement})");
            return;
        }

        // Deduct the total cost
        remainingMovement -= totalCost;

        // Move step-by-step along the path (visual only, no per-step cost)
        for (int i = 1; i < path.Count; i++)
        {
            HexCell nextCell = path[i];
            SetCell(nextCell);
        }

        hasMovedThisTurn = true;
    }

    private float CalculateMoveCost(HexCell cell)
    {
        return cell.GetMovementCost();
    }

    public bool CanAttack(Unit target)
    {
        if (target == null || hasAttacked || target.OwnerPlayerID == OwnerPlayerID)
            return false;

        int distance = HexCoordinates.Distance(CurrentCell.Coordinates, target.CurrentCell.Coordinates);
        return distance <= stats.attackRange;
    }

    public void Attack(Unit target)
    {
        if (!CanAttack(target))
        {
            Debug.LogWarning($"Cannot attack target");
            return;
        }

        int damage = stats.attackPower; // Pure attack damage

        target.TakeDamage(damage);
        hasAttacked = true;

        Debug.Log($"{stats.unitName} attacks {target.stats.unitName} for {damage} damage!");
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{stats.unitName} takes {damage} damage. Health: {currentHealth}/{stats.maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{stats.unitName} has been destroyed!");
        if (CurrentCell != null)
        {
            CurrentCell.OccupyingUnit = null;
        }
        Destroy(gameObject);
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / stats.maxHealth;
    }
}
