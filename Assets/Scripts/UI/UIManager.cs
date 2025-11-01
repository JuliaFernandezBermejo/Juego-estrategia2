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
    [SerializeField] private Text turnText;
    [SerializeField] private Text resourcesText;
    [SerializeField] private Text infoText;

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
}
