using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A* pathfinding algorithm for hexagonal grids.
/// </summary>
public class AStarPathfinding
{
    private class PathNode
    {
        public HexCell cell;
        public PathNode parent;
        public float gCost; // Cost from start
        public float hCost; // Heuristic cost to target
        public float fCost => gCost + hCost;

        public PathNode(HexCell cell, PathNode parent, float gCost, float hCost)
        {
            this.cell = cell;
            this.parent = parent;
            this.gCost = gCost;
            this.hCost = hCost;
        }
    }

    private HexGrid hexGrid;

    public AStarPathfinding(HexGrid grid)
    {
        hexGrid = grid;
    }

    /// <summary>
    /// Find path from start to goal using A* algorithm.
    /// </summary>
    public List<HexCell> FindPath(HexCell start, HexCell goal, Unit unit = null)
    {
        if (start == null || goal == null)
        {
            return null;
        }

        if (start == goal)
        {
            return new List<HexCell> { start };
        }

        // Initialize open and closed sets
        List<PathNode> openSet = new List<PathNode>();
        HashSet<HexCell> closedSet = new HashSet<HexCell>();

        // Add start node
        PathNode startNode = new PathNode(start, null, 0, GetHeuristic(start, goal));
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // Get node with lowest f cost
            PathNode currentNode = GetLowestFCostNode(openSet);

            // Check if reached goal
            if (currentNode.cell == goal)
            {
                return ConstructPath(currentNode);
            }

            // Move current to closed set
            openSet.Remove(currentNode);
            closedSet.Add(currentNode.cell);

            // Check neighbors
            List<HexCell> neighbors = hexGrid.GetNeighbors(currentNode.cell.Coordinates);

            foreach (HexCell neighbor in neighbors)
            {
                // Skip if already evaluated or impassable
                if (closedSet.Contains(neighbor))
                    continue;

                // Skip water and occupied cells (except goal)
                if (neighbor != goal && (!neighbor.IsPassable()))
                    continue;

                // Calculate g cost (cost to reach this neighbor)
                float movementCost = GetMovementCost(currentNode.cell, neighbor, unit);
                float newGCost = currentNode.gCost + movementCost;

                // Check if this path is better
                PathNode existingNode = openSet.Find(n => n.cell == neighbor);

                if (existingNode == null)
                {
                    // Add new node to open set
                    float hCost = GetHeuristic(neighbor, goal);
                    PathNode newNode = new PathNode(neighbor, currentNode, newGCost, hCost);
                    openSet.Add(newNode);
                }
                else if (newGCost < existingNode.gCost)
                {
                    // Update existing node with better path
                    existingNode.gCost = newGCost;
                    existingNode.parent = currentNode;
                }
            }
        }

        // No path found
        return null;
    }

    private PathNode GetLowestFCostNode(List<PathNode> nodes)
    {
        PathNode lowest = nodes[0];
        for (int i = 1; i < nodes.Count; i++)
        {
            // Pick node with lower fCost, or if tied, pick one closer to goal (lower hCost)
            if (nodes[i].fCost < lowest.fCost ||
                (nodes[i].fCost == lowest.fCost && nodes[i].hCost < lowest.hCost))
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

    private List<HexCell> ConstructPath(PathNode endNode)
    {
        List<HexCell> path = new List<HexCell>();
        PathNode currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode.cell);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }
}
