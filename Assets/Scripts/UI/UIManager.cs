using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI manager for displaying game information.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI resourcesText;
    [SerializeField] private TextMeshProUGUI infoText;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (gameManager == null)
            return;

        int currentPlayer = gameManager.GetCurrentPlayerID();
        int resources = gameManager.GetPlayerResources(currentPlayer);

        if (turnText != null)
        {
            turnText.text = $"Player {currentPlayer}'s Turn";
        }

        if (resourcesText != null)
        {
            resourcesText.text = $"Resources: {resources}";
        }

        if (infoText != null)
        {
            infoText.text = "Press SPACE to end turn\nClick units to select\nClick cells to move/attack";
        }
    }

    // Button handlers for unit production
    public void ProduceInfantry()
    {
        ProduceUnit(HardcodedUnitStats.Infantry);
    }

    public void ProduceCavalry()
    {
        ProduceUnit(HardcodedUnitStats.Cavalry);
    }

    public void ProduceArtillery()
    {
        ProduceUnit(HardcodedUnitStats.Artillery);
    }

    private void ProduceUnit(HardcodedUnitStats stats)
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }

        int currentPlayer = gameManager.GetCurrentPlayerID();
        int resources = gameManager.GetPlayerResources(currentPlayer);

        // Check if player has enough resources
        if (resources < stats.cost)
        {
            Debug.Log($"Not enough resources! Need {stats.cost}, have {resources}");
            return;
        }

        // Check if any unit has moved this turn
        if (gameManager.AnyUnitMovedThisTurn)
        {
            Debug.Log("Cannot produce units after moving! Production must happen before movement.");
            return;
        }

        // Get player base and find adjacent empty cell
        HexCell baseCell = gameManager.GetPlayerBase(currentPlayer);
        if (baseCell == null)
        {
            Debug.LogError("Player base not found!");
            return;
        }

        // Find an empty neighbor cell to spawn unit
        HexGrid hexGrid = gameManager.GetHexGrid();
        List<HexCell> neighbors = hexGrid.GetNeighbors(baseCell.Coordinates);

        HexCell spawnCell = null;
        foreach (var neighbor in neighbors)
        {
            if (!neighbor.IsOccupied() && neighbor.IsPassable())
            {
                spawnCell = neighbor;
                break;
            }
        }

        if (spawnCell == null)
        {
            Debug.Log("No available space to spawn unit near base!");
            return;
        }

        // Spawn the unit and deduct resources
        gameManager.SpawnUnit(stats, currentPlayer, spawnCell);
        gameManager.DeductPlayerResources(currentPlayer, stats.cost);

        Debug.Log($"Produced {stats.unitName} for {stats.cost} resources. Remaining: {gameManager.GetPlayerResources(currentPlayer)}");
    }
}
