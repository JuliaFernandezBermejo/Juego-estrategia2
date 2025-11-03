using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tactical pathfinding that considers danger, terrain preferences, and strategic factors.
/// Extends basic A* with tactical weights.
/// </summary>
public class TacticalPathfinding
{
    private class TacticalNode
    {
        public HexCell cell;
        public TacticalNode parent;
        public float gCost; // Movement cost
        public float hCost; // Heuristic
        public float tCost; // Tactical cost (danger, terrain, etc.)
        public float fCost => gCost + hCost + tCost;

        public TacticalNode(HexCell cell, TacticalNode parent, float gCost, float hCost, float tCost)
        {
            this.cell = cell;
            this.parent = parent;
            this.gCost = gCost;
            this.hCost = hCost;
            this.tCost = tCost;
        }
    }

    private HexGrid hexGrid;
    private GameManager gameManager;

    // Tactical weights
    private const float DANGER_WEIGHT = 2.0f;
    private const float TERRAIN_PREFERENCE_WEIGHT = 0.5f;

    public TacticalPathfinding(HexGrid grid, GameManager manager)
    {
        hexGrid = grid;
        gameManager = manager;
    }

    /// <summary>
    /// Find tactically optimal path considering danger and terrain preferences.
    /// </summary>
    public List<HexCell> FindTacticalPath(HexCell start, HexCell goal, Unit unit)
    {
        if (start == null || goal == null || unit == null)
        {
            Debug.LogWarning($"[Pathfinding] Null parameters: start={start}, goal={goal}, unit={unit}");
            return null;
        }

        if (start == goal)
        {
            return new List<HexCell> { start };
        }

        Debug.Log($"[Pathfinding] Finding path from {start.Coordinates} to {goal.Coordinates} for {unit.Stats.unitName}");

        List<TacticalNode> openSet = new List<TacticalNode>();
        HashSet<HexCell> closedSet = new HashSet<HexCell>();

        TacticalNode startNode = new TacticalNode(start, null, 0, GetHeuristic(start, goal), 0);
        openSet.Add(startNode);

        int iterations = 0;
        int maxIterations = 200; // Prevent infinite loops

        while (openSet.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            TacticalNode currentNode = GetLowestFCostNode(openSet);

            if (currentNode.cell == goal)
            {
                List<HexCell> path = ConstructPath(currentNode);
                Debug.Log($"[Pathfinding] Path found! Length: {path.Count} cells, iterations: {iterations}");
                return path;
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode.cell);

            List<HexCell> neighbors = hexGrid.GetNeighbors(currentNode.cell.Coordinates);

            foreach (HexCell neighbor in neighbors)
            {
                if (closedSet.Contains(neighbor))
                    continue;

                if (neighbor != goal && !neighbor.IsPassable())
                    continue;

                // Calculate costs
                float movementCost = GetMovementCost(currentNode.cell, neighbor, unit);
                float tacticalCost = GetTacticalCost(neighbor, unit);
                float newGCost = currentNode.gCost + movementCost;
                float newTCost = tacticalCost;

                TacticalNode existingNode = openSet.Find(n => n.cell == neighbor);

                if (existingNode == null)
                {
                    float hCost = GetHeuristic(neighbor, goal);
                    TacticalNode newNode = new TacticalNode(neighbor, currentNode, newGCost, hCost, newTCost);
                    openSet.Add(newNode);
                }
                else if (newGCost + newTCost < existingNode.gCost + existingNode.tCost)
                {
                    existingNode.gCost = newGCost;
                    existingNode.tCost = newTCost;
                    existingNode.parent = currentNode;
                }
            }
        }

        Debug.LogWarning($"[Pathfinding] NO PATH FOUND from {start.Coordinates} to {goal.Coordinates}! OpenSet exhausted after {iterations} iterations. Closed cells: {closedSet.Count}");
        return null;
    }

    private TacticalNode GetLowestFCostNode(List<TacticalNode> nodes)
    {
        TacticalNode lowest = nodes[0];
        for (int i = 1; i < nodes.Count; i++)
        {
            if (nodes[i].fCost < lowest.fCost)
            {
                lowest = nodes[i];
            }
        }
        return lowest;
    }

    private float GetHeuristic(HexCell from, HexCell to)
    {
        // Manhattan distance for hexagonal grid, scaled by minimum terrain cost
        // This ensures the heuristic is admissible (never overestimates)
        // Minimum cost is Plains (1.0), so even the cheapest path costs at least distance × 1.0
        return HexCoordinates.Distance(from.Coordinates, to.Coordinates) * 1.0f;
    }

    private float GetMovementCost(HexCell from, HexCell to, Unit unit)
    {
        return to.GetMovementCost();
    }

    private float GetTacticalCost(HexCell cell, Unit unit)
    {
        float tacticalCost = 0;

        // Danger cost: avoid cells near enemy units
        float dangerCost = CalculateDangerCost(cell, unit);
        tacticalCost += dangerCost * DANGER_WEIGHT;

        return tacticalCost;
    }

    private float CalculateDangerCost(HexCell cell, Unit unit)
    {
        float danger = 0;
        List<Unit> allUnits = gameManager.GetAllUnits();

        foreach (Unit enemyUnit in allUnits)
        {
            if (enemyUnit.OwnerPlayerID == unit.OwnerPlayerID)
                continue;

            int distance = HexCoordinates.Distance(cell.Coordinates, enemyUnit.CurrentCell.Coordinates);

            // High danger if within enemy attack range
            if (distance <= enemyUnit.Stats.attackRange)
            {
                danger += 5.0f;
            }
            // Medium danger if nearby
            else if (distance <= enemyUnit.Stats.attackRange + 2)
            {
                danger += 2.0f;
            }
        }

        return danger;
    }

    private List<HexCell> ConstructPath(TacticalNode endNode)
    {
        List<HexCell> path = new List<HexCell>();
        TacticalNode currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode.cell);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }
}
