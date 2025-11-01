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

    public HardcodedUnitStats Stats => stats;
    public int CurrentHealth => currentHealth;
    public int RemainingMovement => remainingMovement;
    public bool HasAttacked => hasAttacked;
    public int OwnerPlayerID { get; private set; }
    public HexCell CurrentCell { get; private set; }

    // For AI
    public string CurrentOrder { get; set; } // Strategic order from AI
    public HexCell TargetCell { get; set; } // Target destination

    private MeshRenderer meshRenderer;
    private Color playerColor;

    public void Initialize(HardcodedUnitStats unitStats, int ownerPlayerID, HexCell startingCell)
    {
        stats = unitStats;
        OwnerPlayerID = ownerPlayerID;
        currentHealth = stats.maxHealth;

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
        if (targetCell == null || !targetCell.IsPassable())
            return false;

        int distance = HexCoordinates.Distance(CurrentCell.Coordinates, targetCell.Coordinates);
        float moveCost = CalculateMoveCost(targetCell);

        return remainingMovement >= distance * moveCost;
    }

    public void MoveTo(HexCell targetCell)
    {
        if (!CanMoveTo(targetCell))
        {
            Debug.LogWarning($"Cannot move unit to {targetCell.Coordinates}");
            return;
        }

        int distance = HexCoordinates.Distance(CurrentCell.Coordinates, targetCell.Coordinates);
        float moveCost = CalculateMoveCost(targetCell);
        int totalCost = Mathf.CeilToInt(distance * moveCost);

        remainingMovement -= totalCost;
        SetCell(targetCell);
    }

    private float CalculateMoveCost(HexCell cell)
    {
        float baseCost = cell.GetMovementCost();
        float terrainModifier = stats.GetTerrainMovementModifier(cell.Terrain);
        return baseCost * terrainModifier;
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

        int attackBonus = stats.GetTerrainAttackBonus(CurrentCell.Terrain);
        int defenseBonus = target.CurrentCell.GetDefenseBonus();

        int damage = stats.attackPower + attackBonus - target.stats.defensePower - defenseBonus;
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
