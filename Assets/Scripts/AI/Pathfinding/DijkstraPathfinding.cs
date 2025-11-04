using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dijkstra's pathfinding algorithm for hexagonal grids.
/// Finds the optimal (minimum cost) path by exploring all directions equally.
/// </summary>
public class DijkstraPathfinding
{
    private class PathNode
    {
        public HexCell cell;
        public PathNode parent;
        public float gCost; // Cost from start (this is the only cost in Dijkstra)

        public PathNode(HexCell cell, PathNode parent, float gCost)
        {
            this.cell = cell;
            this.parent = parent;
            this.gCost = gCost;
        }
    }

    private HexGrid hexGrid;

    public DijkstraPathfinding(HexGrid grid)
    {
        hexGrid = grid;
    }

    /// <summary>
    /// Find path from start to goal using Dijkstra's algorithm.
    /// Always finds the cheapest path based on terrain movement costs.
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

        // Add start node with zero cost
        PathNode startNode = new PathNode(start, null, 0);
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // Get node with lowest cost
            PathNode currentNode = GetLowestCostNode(openSet);

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
                // Skip if already evaluated
                if (closedSet.Contains(neighbor))
                {
                    continue;
                }

                // Skip water, occupied cells, and friendly bases (except goal)
                if (neighbor != goal && unit != null && (!neighbor.IsPassableForPlayer(unit.OwnerPlayerID)))
                {
                    continue;
                }

                // Calculate cost to reach this neighbor
                float movementCost = GetMovementCost(currentNode.cell, neighbor, unit);
                float newGCost = currentNode.gCost + movementCost;

                // Check if this path is better
                PathNode existingNode = openSet.Find(n => n.cell == neighbor);

                if (existingNode == null)
                {
                    // Add new node to open set
                    PathNode newNode = new PathNode(neighbor, currentNode, newGCost);
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

    private PathNode GetLowestCostNode(List<PathNode> nodes)
    {
        PathNode lowest = nodes[0];
        for (int i = 1; i < nodes.Count; i++)
        {
            // Pick node with lowest cost
            if (nodes[i].gCost < lowest.gCost)
            {
                lowest = nodes[i];
            }
        }
        return lowest;
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
