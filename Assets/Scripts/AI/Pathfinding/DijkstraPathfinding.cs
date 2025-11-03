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

        Debug.Log($"[Dijkstra] START: from {start.Coordinates} to {goal.Coordinates}");

        // Initialize open and closed sets
        List<PathNode> openSet = new List<PathNode>();
        HashSet<HexCell> closedSet = new HashSet<HexCell>();

        // Add start node with zero cost
        PathNode startNode = new PathNode(start, null, 0);
        openSet.Add(startNode);

        int iteration = 0;
        while (openSet.Count > 0)
        {
            iteration++;
            // Get node with lowest cost
            PathNode currentNode = GetLowestCostNode(openSet);

            bool debug = iteration <= 3; // Debug first 3 iterations only
            if (debug)
            {
                Debug.Log($"[Dijkstra] Iter {iteration}: Processing {currentNode.cell.Coordinates} (Terrain: {currentNode.cell.Terrain}, gCost={currentNode.gCost:F1})");
            }

            // Check if reached goal
            if (currentNode.cell == goal)
            {
                Debug.Log($"[Dijkstra] GOAL REACHED! Cost: {currentNode.gCost:F1}");
                return ConstructPath(currentNode);
            }

            // Move current to closed set
            openSet.Remove(currentNode);
            closedSet.Add(currentNode.cell);

            // Check neighbors
            List<HexCell> neighbors = hexGrid.GetNeighbors(currentNode.cell.Coordinates);
            if (debug)
            {
                Debug.Log($"[Dijkstra]   Found {neighbors.Count} neighbors");
            }

            foreach (HexCell neighbor in neighbors)
            {
                // Skip if already evaluated
                if (closedSet.Contains(neighbor))
                {
                    if (debug) Debug.Log($"[Dijkstra]     {neighbor.Coordinates}: SKIP (closed set)");
                    continue;
                }

                // Skip water, occupied cells, and friendly bases (except goal)
                if (neighbor != goal && unit != null && (!neighbor.IsPassableForPlayer(unit.OwnerPlayerID)))
                {
                    if (debug) Debug.Log($"[Dijkstra]     {neighbor.Coordinates}: SKIP (impassable - Water={neighbor.Terrain == TerrainType.Water}, Occupied={neighbor.IsOccupied()}, FriendlyBase={neighbor.IsBase && neighbor.OwnerPlayerID == unit.OwnerPlayerID})");
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
                    if (debug) Debug.Log($"[Dijkstra]     {neighbor.Coordinates} (Terrain: {neighbor.Terrain}): ADDED with gCost={newGCost:F1}");
                }
                else if (newGCost < existingNode.gCost)
                {
                    // Update existing node with better path
                    if (debug) Debug.Log($"[Dijkstra]     {neighbor.Coordinates}: UPDATED from {existingNode.gCost:F1} to {newGCost:F1}");
                    existingNode.gCost = newGCost;
                    existingNode.parent = currentNode;
                }
                else
                {
                    if (debug) Debug.Log($"[Dijkstra]     {neighbor.Coordinates}: NO UPDATE (existing {existingNode.gCost:F1} <= new {newGCost:F1})");
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
