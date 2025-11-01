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
            return null;
        }

        if (start == goal)
        {
            return new List<HexCell> { start };
        }

        List<TacticalNode> openSet = new List<TacticalNode>();
        HashSet<HexCell> closedSet = new HashSet<HexCell>();

        TacticalNode startNode = new TacticalNode(start, null, 0, GetHeuristic(start, goal), 0);
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            TacticalNode currentNode = GetLowestFCostNode(openSet);

            if (currentNode.cell == goal)
            {
                return ConstructPath(currentNode);
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
        return HexCoordinates.Distance(from.Coordinates, to.Coordinates);
    }

    private float GetMovementCost(HexCell from, HexCell to, Unit unit)
    {
        float cost = to.GetMovementCost();
        cost *= unit.Stats.GetTerrainMovementModifier(to.Terrain);
        return cost;
    }

    private float GetTacticalCost(HexCell cell, Unit unit)
    {
        float tacticalCost = 0;

        // 1. Danger cost: avoid cells near enemy units
        float dangerCost = CalculateDangerCost(cell, unit);
        tacticalCost += dangerCost * DANGER_WEIGHT;

        // 2. Terrain preference cost
        float terrainCost = CalculateTerrainPreferenceCost(cell, unit);
        tacticalCost += terrainCost * TERRAIN_PREFERENCE_WEIGHT;

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

    private float CalculateTerrainPreferenceCost(HexCell cell, Unit unit)
    {
        // Prefer cells with good terrain for this unit type
        if (cell.Terrain == unit.Stats.preferredTerrain)
        {
            return -1.0f; // Bonus (negative cost)
        }
        else if (cell.Terrain == unit.Stats.penalizedTerrain)
        {
            return 1.0f; // Penalty
        }

        return 0;
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
