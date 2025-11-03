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
            cell.OccupyingUnit = this;
            transform.position = cell.transform.position + Vector3.up * 0.5f;
        }
    }

    public bool CanMoveTo(HexCell targetCell)
    {
        Debug.Log($"[CanMoveTo] START - Checking if {stats.unitName} can move to {targetCell?.Coordinates}");

        Debug.Log($"[CanMoveTo] Check 1: hasMovedThisTurn = {hasMovedThisTurn}");
        if (hasMovedThisTurn)
        {
            Debug.LogError($"Cannot move {stats.unitName}: Already moved this turn");
            return false;
        }

        Debug.Log($"[CanMoveTo] Check 2: targetCell null check");
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

        // Use A* pathfinding to find the actual path
        Debug.Log($"[CanMoveTo] Check 3: Finding path using A* pathfinding");
        AStarPathfinding pathfinding = new AStarPathfinding(hexGrid);
        System.Collections.Generic.List<HexCell> path = pathfinding.FindPath(CurrentCell, targetCell, this);

        if (path == null || path.Count == 0)
        {
            Debug.LogError($"Cannot move {stats.unitName} to {targetCell.Coordinates}: No valid path exists");
            return false;
        }

        // Calculate total cost by summing each hex in the path (skip starting hex at index 0)
        float totalCost = 0;
        System.Text.StringBuilder pathDetails = new System.Text.StringBuilder();
        pathDetails.AppendLine($"[CanMoveTo] Path found with {path.Count} cells:");

        for (int i = 0; i < path.Count; i++)
        {
            float cellCost = path[i].GetMovementCost();
            if (i > 0) // Skip starting hex
            {
                totalCost += cellCost;
            }
            pathDetails.AppendLine($"  {i}: {path[i].Coordinates} - Terrain: {path[i].Terrain}, Cost: {cellCost:F1}{(i == 0 ? " (starting hex, not counted)" : "")}");
        }
        Debug.Log(pathDetails.ToString());

        Debug.Log($"[CanMoveTo] Check 4: Movement cost calculation - TotalCost={totalCost:F1}, RemainingMP={remainingMovement}, Required={Mathf.CeilToInt(totalCost)}");

        if (remainingMovement < Mathf.CeilToInt(totalCost))
        {
            Debug.LogError($"Cannot move {stats.unitName} to {targetCell.Coordinates}: Insufficient movement points\n" +
                           $"  - Path length: {path.Count - 1} hex(es) (excluding start)\n" +
                           $"  - Total movement cost: {totalCost:F1}\n" +
                           $"  - Available: {remainingMovement} / Required: {Mathf.CeilToInt(totalCost)}");
            return false;
        }

        Debug.Log($"[CanMoveTo] SUCCESS - Movement is valid");
        return true;
    }

    public void MoveTo(HexCell targetCell)
    {
        if (!CanMoveTo(targetCell))
        {
            // CanMoveTo already logged the reason for failure
            return;
        }

        // Find path again (we know it exists because CanMoveTo succeeded)
        AStarPathfinding pathfinding = new AStarPathfinding(hexGrid);
        System.Collections.Generic.List<HexCell> path = pathfinding.FindPath(CurrentCell, targetCell, this);

        if (path == null || path.Count == 0)
        {
            Debug.LogError($"[MoveTo] Path not found (this shouldn't happen after CanMoveTo succeeded)");
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

        Debug.Log($"[MoveTo] Moving {stats.unitName} along path. Total cost: {totalPathCost:F1} (rounded to {totalCost})");

        // Deduct the total cost
        remainingMovement -= totalCost;

        // Move step-by-step along the path (visual only, no per-step cost)
        for (int i = 1; i < path.Count; i++)
        {
            HexCell nextCell = path[i];
            SetCell(nextCell);
            Debug.Log($"[MoveTo] Step {i}: Moved to {nextCell.Coordinates} (Terrain: {nextCell.Terrain}, Cost: {nextCell.GetMovementCost():F1})");
        }

        hasMovedThisTurn = true;
        Debug.Log($"[MoveTo] Movement complete. Final position: {CurrentCell.Coordinates}, Remaining MP: {remainingMovement}");
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

        int defenseBonus = target.CurrentCell.GetDefenseBonus();
        int damage = stats.attackPower - target.stats.defensePower - defenseBonus;
        damage = Mathf.Max(1, damage); // Minimum 1 damage

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
