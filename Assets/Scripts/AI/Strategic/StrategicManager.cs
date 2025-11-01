using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Strategic AI Manager - the brain of the AI player.
/// Makes high-level decisions and gives orders to units.
/// </summary>
public class StrategicManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private int playerID = 1; // AI player ID

    [Header("AI Settings")]
    [SerializeField] private int aggressiveness = 5; // 1-10, how aggressive the AI is
    [SerializeField] private int resourcePriority = 5; // 1-10, resource gathering priority

    // AI Systems
    private InfluenceMap influenceMap;
    private TacticalWaypoints tacticalWaypoints;

    // Strategic state
    private int turnsSinceLastProduction = 0;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (hexGrid == null)
            hexGrid = FindObjectOfType<HexGrid>();

        influenceMap = new InfluenceMap(hexGrid);
        tacticalWaypoints = new TacticalWaypoints(hexGrid);

        // Wait for game to initialize
        Invoke(nameof(Initialize), 0.5f);
    }

    private void Initialize()
    {
        Debug.Log($"Strategic AI initialized for Player {playerID}");
    }

    public void ExecuteAITurn()
    {
        Debug.Log($"Strategic AI executing turn for Player {playerID}");

        // 1. Update strategic information
        UpdateStrategicInfo();

        // 2. Make strategic decisions
        MakeStrategicDecisions();

        // 3. Give orders to units
        AssignOrdersToUnits();

        // 4. Execute unit actions
        ExecuteUnitActions();

        // 5. Try to produce units
        ConsiderProduction();

        turnsSinceLastProduction++;
    }

    private void UpdateStrategicInfo()
    {
        List<Unit> allUnits = gameManager.GetAllUnits();

        // Update influence map
        influenceMap.UpdateInfluence(allUnits, playerID);

        // Update tactical waypoints
        tacticalWaypoints.UpdateWaypoints(gameManager, influenceMap, playerID);
    }

    private void MakeStrategicDecisions()
    {
        // Analyze current game state
        int ownUnitCount = GetOwnUnitCount();
        int enemyUnitCount = GetEnemyUnitCount();
        int resources = gameManager.GetPlayerResources(playerID);
        HexCell enemyBase = GetEnemyBase();

        // Log strategic assessment
        Debug.Log($"AI Assessment - Own units: {ownUnitCount}, Enemy units: {enemyUnitCount}, Resources: {resources}");

        // Strategic decisions are made based on these factors
        // and translated into orders in AssignOrdersToUnits()
    }

    private void AssignOrdersToUnits()
    {
        List<Unit> ownUnits = GetOwnUnits();

        if (ownUnits.Count == 0)
            return;

        // Calculate strategic priorities
        int ownUnitCount = ownUnits.Count;
        int enemyUnitCount = GetEnemyUnitCount();
        bool isStronger = ownUnitCount > enemyUnitCount;
        bool hasResources = gameManager.GetPlayerResources(playerID) < 50;

        // Divide units into roles
        int attackerCount = 0;
        int defenderCount = 0;
        int gathererCount = 0;

        foreach (var unit in ownUnits)
        {
            UnitAI unitAI = unit.GetComponent<UnitAI>();
            if (unitAI == null)
            {
                unitAI = unit.gameObject.AddComponent<UnitAI>();
            }

            // Decide order based on strategic situation
            string order = DetermineUnitOrder(unit, isStronger, hasResources, ref attackerCount, ref defenderCount, ref gathererCount);

            // Get target cell for the order
            HexCell targetCell = GetOrderTarget(order);

            // Give order to unit
            unitAI.GiveOrder(order, targetCell);
        }
    }

    private string DetermineUnitOrder(Unit unit, bool isStronger, bool needsResources,
                                       ref int attackerCount, ref int defenderCount, ref int gathererCount)
    {
        int ownUnitCount = GetOwnUnitCount();

        // Low health units should retreat
        if (unit.GetHealthPercentage() < 0.3f)
        {
            return "Retreat";
        }

        // If strong and aggressive, prioritize attack
        if (isStronger && aggressiveness >= 7)
        {
            attackerCount++;
            return "AttackBase";
        }

        // If need resources, assign some units to gather
        if (needsResources && gathererCount < ownUnitCount / 3)
        {
            gathererCount++;
            return "GatherResources";
        }

        // Balanced approach: mix of attack and defense
        float attackRatio = aggressiveness / 10f;

        if (attackerCount < ownUnitCount * attackRatio)
        {
            attackerCount++;
            return "AttackBase";
        }
        else
        {
            defenderCount++;
            return "DefendZone";
        }
    }

    private HexCell GetOrderTarget(string order)
    {
        switch (order)
        {
            case "AttackBase":
                var attackWP = tacticalWaypoints.GetHighestPriorityWaypoint(TacticalWaypoints.WaypointType.Attack);
                return attackWP?.cell;

            case "DefendZone":
                var defenseWP = tacticalWaypoints.GetHighestPriorityWaypoint(TacticalWaypoints.WaypointType.Defense);
                return defenseWP?.cell;

            case "GatherResources":
                var resourceWP = tacticalWaypoints.GetHighestPriorityWaypoint(TacticalWaypoints.WaypointType.Resource);
                return resourceWP?.cell;

            case "Retreat":
                var rallyWP = tacticalWaypoints.GetHighestPriorityWaypoint(TacticalWaypoints.WaypointType.Rally);
                return rallyWP?.cell ?? gameManager.GetPlayerBase(playerID);

            default:
                return null;
        }
    }

    private void ExecuteUnitActions()
    {
        List<Unit> ownUnits = GetOwnUnits();

        foreach (var unit in ownUnits)
        {
            UnitAI unitAI = unit.GetComponent<UnitAI>();
            if (unitAI != null)
            {
                unitAI.ExecuteTurn();
            }
        }
    }

    private void ConsiderProduction()
    {
        int resources = gameManager.GetPlayerResources(playerID);
        HexCell ownBase = gameManager.GetPlayerBase(playerID);

        if (ownBase == null)
            return;

        // Simple production logic: produce if we have resources and it's been a few turns
        if (resources >= 30 && turnsSinceLastProduction >= 2)
        {
            // Find empty cell near base
            List<HexCell> neighbors = hexGrid.GetNeighbors(ownBase.Coordinates);

            foreach (var cell in neighbors)
            {
                if (cell.IsPassable())
                {
                    // Produce infantry (cheapest unit)
                    // Note: This requires GameManager to have a SpawnUnit method that costs resources
                    Debug.Log($"AI producing unit at {cell.Coordinates}");
                    turnsSinceLastProduction = 0;
                    break;
                }
            }
        }
    }

    private List<Unit> GetOwnUnits()
    {
        List<Unit> ownUnits = new List<Unit>();
        List<Unit> allUnits = gameManager.GetAllUnits();

        foreach (var unit in allUnits)
        {
            if (unit.OwnerPlayerID == playerID && unit.IsAlive())
            {
                ownUnits.Add(unit);
            }
        }

        return ownUnits;
    }

    private int GetOwnUnitCount()
    {
        return GetOwnUnits().Count;
    }

    private int GetEnemyUnitCount()
    {
        List<Unit> allUnits = gameManager.GetAllUnits();
        int count = 0;

        foreach (var unit in allUnits)
        {
            if (unit.OwnerPlayerID != playerID && unit.IsAlive())
            {
                count++;
            }
        }

        return count;
    }

    private HexCell GetEnemyBase()
    {
        int enemyPlayerID = playerID == 0 ? 1 : 0;
        return gameManager.GetPlayerBase(enemyPlayerID);
    }

    public void SetPlayerID(int id)
    {
        playerID = id;
    }
}
