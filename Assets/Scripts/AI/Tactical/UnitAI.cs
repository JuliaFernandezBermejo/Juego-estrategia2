using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AI controller for individual units.
/// Interprets strategic orders and makes tactical decisions.
/// </summary>
public class UnitAI : MonoBehaviour
{
    private Unit unit;
    private GameManager gameManager;
    private HexGrid hexGrid;
    private TacticalPathfinding pathfinding;
    private BTNode behaviorTree;

    // Current state
    private string currentOrder = "Idle";
    private HexCell targetCell;
    private List<HexCell> currentPath;

    void Start()
    {
        unit = GetComponent<Unit>();
        gameManager = FindObjectOfType<GameManager>();
        hexGrid = FindObjectOfType<HexGrid>();
        pathfinding = new TacticalPathfinding(hexGrid, gameManager);

        BuildBehaviorTree();
    }

    private void BuildBehaviorTree()
    {
        // Root: Selector (try orders in priority)
        behaviorTree = new BTSelector(new List<BTNode>
        {
            // High priority: React to immediate threats
            new BTSequence(new List<BTNode>
            {
                new BTCondition(() => IsUnderAttack()),
                new BTAction(() => ExecuteDefend())
            }),

            // Execute strategic order
            new BTSequence(new List<BTNode>
            {
                new BTCondition(() => HasOrder()),
                new BTSelector(new List<BTNode>
                {
                    // Attack Base order
                    new BTSequence(new List<BTNode>
                    {
                        new BTCondition(() => currentOrder == "AttackBase"),
                        new BTAction(() => ExecuteAttackBase())
                    }),

                    // Defend Zone order
                    new BTSequence(new List<BTNode>
                    {
                        new BTCondition(() => currentOrder == "DefendZone"),
                        new BTAction(() => ExecuteDefendZone())
                    }),

                    // Gather Resources order
                    new BTSequence(new List<BTNode>
                    {
                        new BTCondition(() => currentOrder == "GatherResources"),
                        new BTAction(() => ExecuteGatherResources())
                    }),

                    // Retreat order
                    new BTSequence(new List<BTNode>
                    {
                        new BTCondition(() => currentOrder == "Retreat"),
                        new BTAction(() => ExecuteRetreat())
                    })
                })
            }),

            // Default: Idle
            new BTAction(() => ExecuteIdle())
        });
    }

    public void ExecuteTurn()
    {
        if (unit == null || !unit.IsAlive())
            return;

        // Evaluate behavior tree
        behaviorTree.Evaluate();
    }

    public void GiveOrder(string order, HexCell target = null)
    {
        currentOrder = order;
        targetCell = target;
        Debug.Log($"{unit.Stats.unitName} received order: {order}");
    }

    private bool HasOrder()
    {
        return currentOrder != "Idle";
    }

    private bool IsUnderAttack()
    {
        // Check if enemy units are in attack range
        List<Unit> enemies = GetNearbyEnemies(unit.Stats.attackRange + 1);
        return enemies.Count > 0 && unit.GetHealthPercentage() < 0.7f;
    }

    private BTNode.NodeState ExecuteAttackBase()
    {
        // Get enemy base
        int enemyPlayerID = unit.OwnerPlayerID == 0 ? 1 : 0;
        HexCell enemyBase = gameManager.GetPlayerBase(enemyPlayerID);

        if (enemyBase == null)
            return BTNode.NodeState.Failure;

        // Check if at base
        if (unit.CurrentCell == enemyBase)
        {
            currentOrder = "Idle";
            return BTNode.NodeState.Success;
        }

        // Try to attack nearby enemies
        Unit nearbyEnemy = GetClosestEnemy(unit.Stats.attackRange);
        if (nearbyEnemy != null && unit.CanAttack(nearbyEnemy))
        {
            unit.Attack(nearbyEnemy);
            return BTNode.NodeState.Running;
        }

        // Move toward base
        if (unit.RemainingMovement > 0)
        {
            MoveToward(enemyBase);
        }

        return BTNode.NodeState.Running;
    }

    private BTNode.NodeState ExecuteDefendZone()
    {
        if (targetCell == null)
        {
            // Default to own base
            targetCell = gameManager.GetPlayerBase(unit.OwnerPlayerID);
        }

        // Attack nearby enemies
        Unit nearbyEnemy = GetClosestEnemy(unit.Stats.attackRange);
        if (nearbyEnemy != null && unit.CanAttack(nearbyEnemy))
        {
            unit.Attack(nearbyEnemy);
            return BTNode.NodeState.Running;
        }

        // Stay near defense zone
        int distance = HexCoordinates.Distance(unit.CurrentCell.Coordinates, targetCell.Coordinates);
        if (distance > 2 && unit.RemainingMovement > 0)
        {
            MoveToward(targetCell);
        }

        return BTNode.NodeState.Running;
    }

    private BTNode.NodeState ExecuteGatherResources()
    {
        // Find nearest resource node
        HexCell resourceNode = FindNearestResourceNode();

        if (resourceNode == null)
        {
            currentOrder = "Idle";
            return BTNode.NodeState.Failure;
        }

        // If at resource node, collect and finish
        if (unit.CurrentCell == resourceNode)
        {
            currentOrder = "Idle";
            return BTNode.NodeState.Success;
        }

        // Move toward resource
        if (unit.RemainingMovement > 0)
        {
            MoveToward(resourceNode);
        }

        return BTNode.NodeState.Running;
    }

    private BTNode.NodeState ExecuteRetreat()
    {
        // Move toward own base
        HexCell ownBase = gameManager.GetPlayerBase(unit.OwnerPlayerID);

        if (ownBase == null)
            return BTNode.NodeState.Failure;

        int distance = HexCoordinates.Distance(unit.CurrentCell.Coordinates, ownBase.Coordinates);

        if (distance <= 2)
        {
            currentOrder = "DefendZone";
            return BTNode.NodeState.Success;
        }

        if (unit.RemainingMovement > 0)
        {
            MoveToward(ownBase);
        }

        return BTNode.NodeState.Running;
    }

    private BTNode.NodeState ExecuteDefend()
    {
        // Attack nearest enemy
        Unit nearbyEnemy = GetClosestEnemy(unit.Stats.attackRange);
        if (nearbyEnemy != null && unit.CanAttack(nearbyEnemy))
        {
            unit.Attack(nearbyEnemy);
            return BTNode.NodeState.Success;
        }

        // Retreat if low health
        if (unit.GetHealthPercentage() < 0.3f)
        {
            currentOrder = "Retreat";
            return BTNode.NodeState.Success;
        }

        return BTNode.NodeState.Failure;
    }

    private BTNode.NodeState ExecuteIdle()
    {
        // Do nothing
        return BTNode.NodeState.Success;
    }

    private void MoveToward(HexCell target)
    {
        if (target == null || unit.RemainingMovement <= 0)
            return;

        // Find path
        List<HexCell> path = pathfinding.FindTacticalPath(unit.CurrentCell, target, unit);

        if (path != null && path.Count > 1)
        {
            // Move to next cell in path
            HexCell nextCell = path[1];
            if (unit.CanMoveTo(nextCell))
            {
                unit.MoveTo(nextCell);
            }
        }
    }

    private Unit GetClosestEnemy(int maxRange)
    {
        List<Unit> enemies = GetNearbyEnemies(maxRange);

        if (enemies.Count == 0)
            return null;

        Unit closest = enemies[0];
        int closestDist = HexCoordinates.Distance(unit.CurrentCell.Coordinates, closest.CurrentCell.Coordinates);

        foreach (var enemy in enemies)
        {
            int dist = HexCoordinates.Distance(unit.CurrentCell.Coordinates, enemy.CurrentCell.Coordinates);
            if (dist < closestDist)
            {
                closest = enemy;
                closestDist = dist;
            }
        }

        return closest;
    }

    private List<Unit> GetNearbyEnemies(int range)
    {
        List<Unit> enemies = new List<Unit>();
        List<Unit> allUnits = gameManager.GetAllUnits();

        foreach (var other in allUnits)
        {
            if (other.OwnerPlayerID != unit.OwnerPlayerID && other.IsAlive())
            {
                int distance = HexCoordinates.Distance(unit.CurrentCell.Coordinates, other.CurrentCell.Coordinates);
                if (distance <= range)
                {
                    enemies.Add(other);
                }
            }
        }

        return enemies;
    }

    private HexCell FindNearestResourceNode()
    {
        HexCell nearest = null;
        int nearestDist = int.MaxValue;

        var allCells = hexGrid.GetAllCells();
        foreach (var cell in allCells.Values)
        {
            if (cell.HasResourceNode && !cell.IsOccupied())
            {
                int dist = HexCoordinates.Distance(unit.CurrentCell.Coordinates, cell.Coordinates);
                if (dist < nearestDist)
                {
                    nearest = cell;
                    nearestDist = dist;
                }
            }
        }

        return nearest;
    }
}
