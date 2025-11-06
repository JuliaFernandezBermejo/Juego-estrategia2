using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Influence map system for strategic decision making.
/// Tracks friendly and enemy influence across the map.
/// </summary>
public class InfluenceMap
{
    private HexGrid hexGrid;
    private Dictionary<HexCoordinates, float> friendlyInfluence;
    private Dictionary<HexCoordinates, float> enemyInfluence;

    private const float MAX_INFLUENCE_RANGE = 8.0f; // Maximum movement cost range for influence propagation

    public InfluenceMap(HexGrid grid)
    {
        hexGrid = grid;
        friendlyInfluence = new Dictionary<HexCoordinates, float>();
        enemyInfluence = new Dictionary<HexCoordinates, float>();
    }

    public void UpdateInfluence(List<Unit> allUnits, int playerID)
    {
        // Clear previous influence
        friendlyInfluence.Clear();
        enemyInfluence.Clear();

        // Calculate both friendly and enemy influence
        foreach (var unit in allUnits)
        {
            if (!unit.IsAlive())
                continue;

            if (unit.OwnerPlayerID == playerID)
            {
                // Propagate friendly control influence
                PropagateFriendlyControl(unit);
            }
            else
            {
                // Propagate enemy threat influence
                PropagateEnemyThreat(unit);
            }
        }
    }

    private void PropagateEnemyThreat(Unit enemyUnit)
    {
        PropagateCostAwareInfluence(enemyUnit, enemyInfluence);
    }

    private void PropagateFriendlyControl(Unit friendlyUnit)
    {
        PropagateCostAwareInfluence(friendlyUnit, friendlyInfluence);
    }

    /// <summary>
    /// Propagates influence from a unit using cost-aware Dijkstra expansion.
    /// Uses cheapest movement cost (not geometric distance) for linear decay.
    /// </summary>
    private void PropagateCostAwareInfluence(Unit unit, Dictionary<HexCoordinates, float> influenceMap)
    {
        HexCell source = unit.CurrentCell;
        if (source == null) return;

        // Base influence is attack power scaled by health percentage
        float healthPercent = (float)unit.CurrentHealth / unit.Stats.maxHealth;
        float baseInfluence = unit.Stats.attackPower * healthPercent;

        // Priority queue for Dijkstra expansion: (cell, movementCost)
        var openSet = new SortedDictionary<float, List<HexCell>>();
        var costs = new Dictionary<HexCoordinates, float>();

        // Initialize with source
        openSet[0] = new List<HexCell> { source };
        costs[source.Coordinates] = 0;

        // Dijkstra expansion
        while (openSet.Count > 0)
        {
            // Get cell with lowest cost
            var lowestEntry = openSet.GetEnumerator();
            lowestEntry.MoveNext();
            float currentCost = lowestEntry.Current.Key;
            List<HexCell> cellsAtCost = lowestEntry.Current.Value;

            HexCell current = cellsAtCost[0];
            cellsAtCost.RemoveAt(0);
            if (cellsAtCost.Count == 0)
            {
                openSet.Remove(currentCost);
            }

            // Stop if beyond influence range
            if (currentCost > MAX_INFLUENCE_RANGE)
                continue;

            // Apply influence with linear decay based on movement cost
            float decayFactor = 1.0f - (currentCost / MAX_INFLUENCE_RANGE);
            float influence = baseInfluence * decayFactor;

            if (influence > 0)
            {
                AddInfluence(influenceMap, current.Coordinates, influence);
            }

            // Expand to neighbors
            List<HexCell> neighbors = hexGrid.GetNeighbors(current.Coordinates);
            foreach (var neighbor in neighbors)
            {
                if (!neighbor.IsPassable())
                    continue;

                float movementCost = neighbor.GetMovementCost();
                float newCost = currentCost + movementCost;

                // Only process if we found a cheaper path or haven't visited
                if (newCost <= MAX_INFLUENCE_RANGE &&
                    (!costs.ContainsKey(neighbor.Coordinates) || newCost < costs[neighbor.Coordinates]))
                {
                    costs[neighbor.Coordinates] = newCost;

                    // Add to open set
                    if (!openSet.ContainsKey(newCost))
                    {
                        openSet[newCost] = new List<HexCell>();
                    }
                    openSet[newCost].Add(neighbor);
                }
            }
        }
    }

    private void AddInfluence(Dictionary<HexCoordinates, float> map, HexCoordinates coords, float influence)
    {
        if (map.ContainsKey(coords))
        {
            map[coords] += influence;
        }
        else
        {
            map[coords] = influence;
        }
    }

    public float GetFriendlyInfluence(HexCoordinates coords)
    {
        return friendlyInfluence.ContainsKey(coords) ? friendlyInfluence[coords] : 0;
    }

    public float GetEnemyInfluence(HexCoordinates coords)
    {
        return enemyInfluence.ContainsKey(coords) ? enemyInfluence[coords] : 0;
    }

    public float GetNetInfluence(HexCoordinates coords)
    {
        return GetFriendlyInfluence(coords) - GetEnemyInfluence(coords);
    }

    public bool IsSafeZone(HexCoordinates coords)
    {
        return GetNetInfluence(coords) > 0;
    }

    public bool IsDangerZone(HexCoordinates coords)
    {
        return GetNetInfluence(coords) < -5;
    }

    public HexCell FindSafestNearbyCell(HexCell currentCell, int searchRange)
    {
        List<HexCell> nearbyСells = hexGrid.GetCellsInRange(currentCell.Coordinates, searchRange);

        HexCell safest = currentCell;
        float safestInfluence = GetNetInfluence(currentCell.Coordinates);

        foreach (var cell in nearbyСells)
        {
            if (!cell.IsPassable())
                continue;

            float influence = GetNetInfluence(cell.Coordinates);
            if (influence > safestInfluence)
            {
                safest = cell;
                safestInfluence = influence;
            }
        }

        return safest;
    }
}
