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

    private const float INFLUENCE_DECAY = 0.7f; // Decay per distance
    private const int INFLUENCE_RANGE = 3;

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

        // Calculate influence for each unit
        foreach (var unit in allUnits)
        {
            if (!unit.IsAlive())
                continue;

            bool isFriendly = unit.OwnerPlayerID == playerID;
            float baseInfluence = unit.Stats.attackPower; // Influence based on attack power

            // Propagate influence to nearby cells
            PropagateInfluence(unit.CurrentCell, baseInfluence, isFriendly);
        }
    }

    private void PropagateInfluence(HexCell source, float baseInfluence, bool isFriendly)
    {
        Dictionary<HexCoordinates, float> targetMap = isFriendly ? friendlyInfluence : enemyInfluence;

        // Add influence to source cell
        AddInfluence(targetMap, source.Coordinates, baseInfluence);

        // Propagate to nearby cells with decay
        List<HexCell> cellsInRange = hexGrid.GetCellsInRange(source.Coordinates, INFLUENCE_RANGE);

        foreach (var cell in cellsInRange)
        {
            if (cell == source)
                continue;

            int distance = HexCoordinates.Distance(source.Coordinates, cell.Coordinates);
            float influence = baseInfluence * Mathf.Pow(INFLUENCE_DECAY, distance);

            AddInfluence(targetMap, cell.Coordinates, influence);
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
