using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main game manager handling turns, players, and game flow.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private GameObject unitPrefab;

    [Header("Game Settings")]
    [SerializeField] private int numPlayers = 2;
    [SerializeField] private int startingResources = 100;
    [SerializeField] private int resourcesPerTurn = 10;

    // Game state
    private int currentPlayerID = 0;
    private int turnNumber = 0;
    private List<Unit> allUnits = new List<Unit>();
    private Dictionary<int, int> playerResources = new Dictionary<int, int>();
    private Dictionary<int, HexCell> playerBases = new Dictionary<int, HexCell>();

    // Selection state
    private Unit selectedUnit;

    // Win condition
    private int winner = -1;

    void Start()
    {
        if (hexGrid == null)
            hexGrid = FindObjectOfType<HexGrid>();

        InitializeGame();
    }

    private void InitializeGame()
    {
        // Wait for grid to be ready
        InvokeRepeating(nameof(CheckGridReady), 0.1f, 0.1f);
    }

    private void CheckGridReady()
    {
        if (hexGrid != null && hexGrid.IsGridReady)
        {
            CancelInvoke(nameof(CheckGridReady));
            SetupGame();
        }
    }

    private void SetupGame()
    {
        // Initialize player resources
        for (int i = 0; i < numPlayers; i++)
        {
            playerResources[i] = startingResources;
        }

        // Set up player bases (opposite corners)
        SetupPlayerBases();

        // Spawn starting units
        SpawnStartingUnits();

        // Place resource nodes
        PlaceResourceNodes();

        Debug.Log("Game initialized! Player 0's turn.");
    }

    private void SetupPlayerBases()
    {
        // Player 0 base at bottom-left
        HexCell p0Base = hexGrid.GetCell(0, 0);
        if (p0Base != null)
        {
            p0Base.IsBase = true;
            p0Base.OwnerPlayerID = 0;
            p0Base.SetTerrain(TerrainType.Plains);

            // Give base a distinct blue color
            MeshRenderer renderer = p0Base.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.3f, 0.3f, 0.8f);
            }

            playerBases[0] = p0Base;

            // Ensure neighbors are passable for unit spawning
            EnsurePassableNeighbors(p0Base);
        }

        // Player 1 base at top-right
        HexCell p1Base = hexGrid.GetCell(hexGrid.Width - 1, hexGrid.Height - 1);
        if (p1Base != null)
        {
            p1Base.IsBase = true;
            p1Base.OwnerPlayerID = 1;
            p1Base.SetTerrain(TerrainType.Plains);

            // Give base a distinct red color
            MeshRenderer renderer = p1Base.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.8f, 0.3f, 0.3f);
            }

            playerBases[1] = p1Base;

            // Ensure neighbors are passable for unit spawning
            EnsurePassableNeighbors(p1Base);
        }
    }

    private void EnsurePassableNeighbors(HexCell baseCell)
    {
        List<HexCell> neighbors = hexGrid.GetNeighbors(baseCell.Coordinates);
        int passableCount = 0;

        foreach (var neighbor in neighbors)
        {
            if (neighbor.IsPassable())
            {
                passableCount++;
            }
            else if (passableCount < 2)
            {
                // Convert impassable neighbors to plains until we have at least 2 passable
                neighbor.SetTerrain(TerrainType.Plains);
                passableCount++;
            }
        }
    }

    private void SpawnStartingUnits()
    {
        // Spawn 2 infantry near each base
        for (int playerID = 0; playerID < numPlayers; playerID++)
        {
            HexCell baseCell = playerBases[playerID];
            List<HexCell> neighbors = hexGrid.GetNeighbors(baseCell.Coordinates);

            int spawnCount = 0;
            foreach (var cell in neighbors)
            {
                if (cell.IsPassable() && spawnCount < 2)
                {
                    SpawnUnit(HardcodedUnitStats.Infantry, playerID, cell);
                    spawnCount++;
                }
            }
        }
    }

    private void PlaceResourceNodes()
    {
        // Place 5-7 random resource nodes on the map
        var allCells = hexGrid.GetAllCells();
        List<HexCell> availableCells = new List<HexCell>();

        foreach (var cell in allCells.Values)
        {
            if (!cell.IsOccupied() && !cell.IsBase && cell.Terrain != TerrainType.Water)
            {
                availableCells.Add(cell);
            }
        }

        int nodeCount = Random.Range(5, 8);
        for (int i = 0; i < nodeCount && availableCells.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableCells.Count);
            HexCell cell = availableCells[randomIndex];
            cell.HasResourceNode = true;
            availableCells.RemoveAt(randomIndex);

            Debug.Log($"Resource node placed at {cell.Coordinates}");
        }
    }

    public void SpawnUnit(HardcodedUnitStats stats, int playerID, HexCell cell)
    {
        if (unitPrefab == null)
        {
            unitPrefab = CreateUnitPrefab();
        }

        GameObject unitObj = Instantiate(unitPrefab, cell.transform.position, Quaternion.identity);
        Unit unit = unitObj.GetComponent<Unit>();
        if (unit == null)
        {
            unit = unitObj.AddComponent<Unit>();
        }

        unit.Initialize(stats, playerID, cell);
        allUnits.Add(unit);

        Debug.Log($"Spawned {stats.unitName} for Player {playerID} at {cell.Coordinates}");
    }

    private GameObject CreateUnitPrefab()
    {
        GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        prefab.name = "Unit";
        prefab.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        return prefab;
    }

    public void OnCellSelected(HexCell cell)
    {
        if (winner != -1)
        {
            Debug.Log($"Game over! Player {winner} wins!");
            return;
        }

        // If clicking on own unit, select it
        if (cell.IsOccupied() && cell.OccupyingUnit.OwnerPlayerID == currentPlayerID)
        {
            selectedUnit = cell.OccupyingUnit;
            Debug.Log($"Selected {selectedUnit.Stats.unitName}");
            return;
        }

        // If a unit is selected
        if (selectedUnit != null)
        {
            // Try to move or attack
            if (cell.IsOccupied())
            {
                // Try to attack enemy unit
                Unit target = cell.OccupyingUnit;
                if (target.OwnerPlayerID != currentPlayerID && selectedUnit.CanAttack(target))
                {
                    selectedUnit.Attack(target);
                    selectedUnit = null;
                }
            }
            else
            {
                // Try to move
                if (selectedUnit.CanMoveTo(cell))
                {
                    selectedUnit.MoveTo(cell);

                    // Collect resources if on resource node
                    if (cell.HasResourceNode)
                    {
                        CollectResources(currentPlayerID, cell);
                    }
                }
            }
        }
    }

    private void CollectResources(int playerID, HexCell cell)
    {
        int amount = 10;
        playerResources[playerID] += amount;
        Debug.Log($"Player {playerID} collected {amount} resources. Total: {playerResources[playerID]}");
    }

    public void EndTurn()
    {
        // Refresh all units for current player
        foreach (var unit in allUnits)
        {
            if (unit.OwnerPlayerID == currentPlayerID)
            {
                unit.RefreshTurn();
            }
        }

        selectedUnit = null;

        // Next player
        currentPlayerID = (currentPlayerID + 1) % numPlayers;

        if (currentPlayerID == 0)
        {
            turnNumber++;
        }

        // Give resources per turn
        playerResources[currentPlayerID] += resourcesPerTurn;

        // Check win condition
        CheckWinCondition();

        Debug.Log($"Turn {turnNumber} - Player {currentPlayerID}'s turn. Resources: {playerResources[currentPlayerID]}");

        // Execute AI turn if it's Player 1 (AI player)
        if (currentPlayerID == 1)
        {
            StrategicManager strategicAI = FindObjectOfType<StrategicManager>();
            if (strategicAI != null)
            {
                Invoke(nameof(ExecuteAITurn), 0.5f); // Small delay so you can see the turn change
            }
        }
    }

    private void ExecuteAITurn()
    {
        StrategicManager strategicAI = FindObjectOfType<StrategicManager>();
        if (strategicAI != null)
        {
            strategicAI.ExecuteAITurn();
            // Auto end AI turn after execution
            Invoke(nameof(EndTurn), 1.0f);
        }
    }

    private void CheckWinCondition()
    {
        // Win by capturing enemy base
        foreach (var kvp in playerBases)
        {
            int baseOwner = kvp.Key;
            HexCell baseCell = kvp.Value;

            if (baseCell.IsOccupied())
            {
                int occupyingPlayer = baseCell.OccupyingUnit.OwnerPlayerID;
                if (occupyingPlayer != baseOwner)
                {
                    winner = occupyingPlayer;
                    Debug.Log($"GAME OVER! Player {winner} captured the base and wins!");
                    return;
                }
            }
        }
    }

    void Update()
    {
        // Press Space to end turn
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();
        }
    }

    public int GetCurrentPlayerID()
    {
        return currentPlayerID;
    }

    public int GetPlayerResources(int playerID)
    {
        return playerResources.ContainsKey(playerID) ? playerResources[playerID] : 0;
    }

    public List<Unit> GetAllUnits()
    {
        return allUnits;
    }

    public HexCell GetPlayerBase(int playerID)
    {
        return playerBases.ContainsKey(playerID) ? playerBases[playerID] : null;
    }
}
