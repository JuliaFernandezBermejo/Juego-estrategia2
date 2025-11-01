using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages tactical waypoints for strategic decision making.
/// </summary>
public class TacticalWaypoints
{
    public enum WaypointType
    {
        Attack,     // Offensive objectives
        Defense,    // Defensive positions
        Rally,      // Safe regrouping points
        Resource    // Resource collection points
    }

    public class Waypoint
    {
        public HexCell cell;
        public WaypointType type;
        public int priority;

        public Waypoint(HexCell cell, WaypointType type, int priority = 1)
        {
            this.cell = cell;
            this.type = type;
            this.priority = priority;
        }
    }

    private List<Waypoint> waypoints;
    private HexGrid hexGrid;

    public TacticalWaypoints(HexGrid grid)
    {
        hexGrid = grid;
        waypoints = new List<Waypoint>();
    }

    public void UpdateWaypoints(GameManager gameManager, InfluenceMap influenceMap, int playerID)
    {
        waypoints.Clear();

        // Attack waypoint: Enemy base
        int enemyPlayerID = playerID == 0 ? 1 : 0;
        HexCell enemyBase = gameManager.GetPlayerBase(enemyPlayerID);
        if (enemyBase != null)
        {
            AddWaypoint(enemyBase, WaypointType.Attack, 5);
        }

        // Defense waypoint: Own base
        HexCell ownBase = gameManager.GetPlayerBase(playerID);
        if (ownBase != null)
        {
            AddWaypoint(ownBase, WaypointType.Defense, 4);
        }

        // Resource waypoints: Resource nodes
        var allCells = hexGrid.GetAllCells();
        foreach (var cell in allCells.Values)
        {
            if (cell.HasResourceNode)
            {
                AddWaypoint(cell, WaypointType.Resource, 2);
            }
        }

        // Rally waypoints: Safe zones with high friendly influence
        FindRallyPoints(influenceMap, playerID);
    }

    private void FindRallyPoints(InfluenceMap influenceMap, int playerID)
    {
        var allCells = hexGrid.GetAllCells();
        List<HexCell> safeCells = new List<HexCell>();

        foreach (var cell in allCells.Values)
        {
            if (cell.IsPassable() && influenceMap.IsSafeZone(cell.Coordinates))
            {
                safeCells.Add(cell);
            }
        }

        // Add top 3 safest cells as rally points
        safeCells.Sort((a, b) =>
            influenceMap.GetNetInfluence(b.Coordinates).CompareTo(
            influenceMap.GetNetInfluence(a.Coordinates)));

        for (int i = 0; i < Mathf.Min(3, safeCells.Count); i++)
        {
            AddWaypoint(safeCells[i], WaypointType.Rally, 3);
        }
    }

    public void AddWaypoint(HexCell cell, WaypointType type, int priority)
    {
        waypoints.Add(new Waypoint(cell, type, priority));
    }

    public List<Waypoint> GetWaypointsByType(WaypointType type)
    {
        return waypoints.FindAll(w => w.type == type);
    }

    public Waypoint GetHighestPriorityWaypoint(WaypointType type)
    {
        List<Waypoint> filtered = GetWaypointsByType(type);
        if (filtered.Count == 0)
            return null;

        Waypoint highest = filtered[0];
        foreach (var wp in filtered)
        {
            if (wp.priority > highest.priority)
            {
                highest = wp;
            }
        }

        return highest;
    }

    public List<Waypoint> GetAllWaypoints()
    {
        return waypoints;
    }
}
